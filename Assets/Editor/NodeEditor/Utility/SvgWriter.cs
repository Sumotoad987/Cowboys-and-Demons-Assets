using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using Ex.Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Ex.Kingmaker.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Editor.NodeEditor.Nodes;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
    public class SvgWriter
    {
        private const float Scale = 1.5f;
        private const string Namespace = "http://www.w3.org/2000/svg";

        [NotNull]
        private readonly XmlDocument m_XML;

        [NotNull]
        private readonly XmlElement m_Root;

        private float m_Height;
        private float m_Width;

        private readonly CultureInfo m_OldCulture;
        private readonly string m_NewLine;
        private readonly string m_IncorrectNewLine;

        public SvgWriter()
        {
            m_OldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            m_NewLine = TextWriter.Null.NewLine;
            m_XML = new XmlDocument();
            m_Root = m_XML.CreateElement("svg", Namespace);
            m_XML.AppendChild(m_Root);
        }

        public void DrawNode(EditorNode node, bool drawMarkers)
        {
            var color = node.GetWindowColor();
            if (color == Color.white)
                color = Color.grey;

            var rect = new Rect(TranslateCoords(node.Center), node.Size * Scale);
            rect.x -= rect.width / 2;
            rect.y -= rect.height / 2;

            m_Height = Math.Max(rect.yMax, m_Height);
            m_Width = Math.Max(rect.xMax, m_Width);

            var nameRect = rect;
            nameRect.height = 30;
            rect.height -= 30;
            rect.y += 30;

            string id = node.GetName().Replace(" ", "_");
            string href = "";
            if (node is VirtualNode)
            {
                href = "#" + id;
                id = "";
            }

            DrawText(nameRect, color, node.GetName(), 3, id, href);

            var bp = node.GetAsset() as SimpleBlueprint;
            switch (bp)
            {
                case BlueprintCue cue:
                    if (cue.Speaker.Blueprint != null)
                    {
                        var speakerRect = rect;
                        speakerRect.height = 30;
                        rect.height -= 30;
                        rect.y += 30;
                        DrawText(speakerRect, color, "speaker: " + cue.Speaker.Blueprint.CharacterName, 3);
                    }

                    break;
                case BlueprintAnswer answer:
                    //if (!answer.SoulMarkRequirement.Empty)
                    //{
                    //    var alignmentRect = rect;
                    //    alignmentRect.height = 30;
                    //    rect.height -= 30;
                    //    rect.y += 30;
                    //    DrawText(alignmentRect, color, "reinforced: " + answer.SoulMarkRequirement.Direction, 3);
                    //}
                    

                    break;
                case BlueprintQuest quest:
                {
                    var title = rect;
                    title.height = 30;
                    rect.height -= 30;
                    rect.y += 30;
                    DrawText(title, color, quest.Title, 3);
                }
                    break;
                case BlueprintQuestObjective objective:
                {
                    var title = rect;
                    title.height = 30;
                    rect.height -= 30;
                    rect.y += 30;
                    DrawText(title, color, objective.GetTitile(), 3);
                }
                    break;
            }

            string[] markerTexts = Array.Empty<string>();
            float[] markerHeights = Array.Empty<float>();

            if (drawMarkers)
            {
                bool extended = node.Graph.ShowExtendedMarkers;
                markerTexts = node.GetMarkers(extended).ToArray();
                markerHeights = new float[markerTexts.Length];
                for (int i = 0; i < markerTexts.Length; i++)
                {
                    string text = markerTexts[i];
                    markerHeights[i] = text.Split('\n', '\r').Length * 26 + 10;
                }
            }

            var textRect = rect;
            if (drawMarkers)
            {
                textRect.height -= markerHeights.Sum();
                rect.y += textRect.height;
                rect.height = markerHeights.Sum();
            }

            DrawText(textRect, color, node.GetText(), 5);

            for (int i = 0; i < markerTexts.Length; i++)
            {
                string markerText = markerTexts[i];
                var markerRect = rect;
                markerRect.height = markerHeights[i];

                rect.height -= markerRect.height;
                rect.y += markerRect.height;

                DrawText(markerRect, color, markerText, 4, "", "", true);
            }

            foreach (var child in node.GetReferencedNodes())
                DrawConnection(node, child);
        }

        private void DrawText(
            Rect rect, Color color, string content, int fontSize, string id = "", string href = "",
            bool keepLines = false)
        {
            var box = m_XML.CreateElement("rect", Namespace);

            m_Root.AppendChild(box);
            box.SetAttribute("x", rect.x + "");
            box.SetAttribute("y", rect.y + "");
            box.SetAttribute("width", rect.width + "");
            box.SetAttribute("height", rect.height + "");

            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);
            box.SetAttribute("fill", "none");
            box.SetAttribute("stroke", string.Format("rgb({0}, {1}, {2})", r, g, b));
            box.SetAttribute("stroke-width", "5");

            var textRect = rect;
            textRect.x += 5;
            textRect.y += 5;
            textRect.height -= 10;
            textRect.width -= 10;

            var text = m_XML.CreateElement("foreignObject", Namespace);
            m_Root.AppendChild(text);
            text.SetAttribute("x", textRect.x + "");
            text.SetAttribute("y", textRect.y + "");
            text.SetAttribute("width", textRect.width + "");
            text.SetAttribute("height", textRect.height + "");
            text.SetAttribute("requiredFeatures", "http://www.w3.org/TR/SVG11/feature#Extensibility");

            XmlElement el;
            if (href == "")
            {
                el = m_XML.CreateElement("font", "http://www.w3.org/1999/xhtml");
                el.SetAttribute("size", fontSize.ToString());
            }
            else
            {
                el = m_XML.CreateElement("a", "http://www.w3.org/1999/xhtml");
                el.SetAttribute("href", href);
            }

            if (id != "")
            {
                el.SetAttribute("id", id);
            }

            text.AppendChild(el);

            if (!keepLines)
            {
                string fixedContent = content
                    .Replace("\n", m_NewLine);
                el.InnerText = $"{m_NewLine}{fixedContent}{m_NewLine}";
            }
            else
            {
                string fixedContent = content
                    .Replace("\n", m_NewLine)
                    .Replace(m_NewLine, $"<br xmlns=\"http://www.w3.org/1999/xhtml\"/>{m_NewLine}")
                    .Replace("    ", "&#160;&#160;&#160;&#160;  ");

                el.InnerXml = $"{m_NewLine}{fixedContent}{m_NewLine}";
            }
        }

        private void DrawConnection(EditorNode from, EditorNode to)
        {
            Vector2 s = from.Center;
            s.x += from.Size.x / 2;
            s = TranslateCoords(s);

            Vector2 e = to.Center;
            e.x -= to.Size.x / 2;
            e = TranslateCoords(e);

            Vector2 tg1 = s + 0.5f * new Vector2(Math.Abs(e.x - s.x), 0);
            Vector2 tg2 = e - 0.5f * new Vector2(Math.Abs(e.x - s.x), 0);

            var path = m_XML.CreateElement("path", Namespace);
            m_Root.AppendChild(path);
            path.SetAttribute("d",
                string.Format("M {0} {1} C {2} {3} {4} {5} {6} {7}", s.x, s.y, tg1.x, tg1.y, tg2.x, tg2.y, e.x, e.y));
            path.SetAttribute("fill", "none");
            path.SetAttribute("stroke", "black");
            path.SetAttribute("stroke-width", "2");
        }

        public void Save(string fileName)
        {
            Directory.CreateDirectory(Directory.GetParent(fileName)?.FullName ?? "");
            m_Root.SetAttribute("width", m_Width + "");
            m_Root.SetAttribute("height", m_Height + "");
            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                m_XML.WriteTo(writer);
                writer.Flush();
            }

            Thread.CurrentThread.CurrentCulture = m_OldCulture;
        }

        private Vector2 TranslateCoords(Vector2 canvasCoords)
        {
            return canvasCoords * Scale + Vector2.right * 200;
        }
    }
}