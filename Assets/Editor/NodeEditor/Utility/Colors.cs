using UnityEngine;

namespace Kingmaker.Editor.NodeEditor.Utility
{
	public class Colors
	{
		public static readonly Color Default = Color.white;
//		public static readonly Color CheckWindow = Color.green;
//		public static readonly Color CueWindow = new Color(0.40f, 0.98f, 0.79f);
		public static readonly Color CueWindow = Color.green;
		public static readonly Color CheckWindow = rgb(255, 87, 34);
		public static readonly Color AnswerWindow = Color.yellow;
		public static readonly Color AnswerListWindow = rgb(200, 255, 0);
		public static readonly Color CueSequenceWindow = rgb(120, 0, 255);
		public static readonly Color Connection = Color.cyan*0.6f;
		public static readonly Color VirtualLink = new Color(1f, 0.08f, 0.575f) * 0.7f; // pink
		public static readonly Color ReferenceLink = new Color(0.575f, 0.08f, 1f) * 0.7f;

		public static readonly Color Condition = Color.green;
		public static readonly Color ConditionNot = rgb(255, 87, 34);
		public static readonly Color Action = Color.yellow;

		public static readonly Color ShadowDeleted = Color.red;

		public static Color GetHighlighColor(Color color)
		{
			return color + Color.grey;
		}

		public static Color GetFadeColor(Color color)
		{
			color.a = 0.3f;
			return color;
		}

		private static Color rgb(int r, int g, int b)
		{
			return new Color(r/255f, g/255f, b/255f);
		}
	}
}