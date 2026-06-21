using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Pathfinding.Serialization;

namespace Pathfinding
{
    using HarmonyLib;
    using Pathfinding.Util;
    using System.Collections;
    using System.Reflection;

    [CustomGraphEditor(typeof(RecastGraph), "Recast Graph")]
    public class RecastGraphEditor : GraphEditor
    {
        public override void OnInspectorGUI(NavGraph target)
        {
            DrawShape((RecastGraph)target); 
            Separator();
            DrawInputFiltering((RecastGraph)target);
            Separator();
            DrawAgentCharacteristic((RecastGraph)target);
            Separator();
            DrawRasterization((RecastGraph)target);
            Separator();
            DrawRuntimeSetting((RecastGraph)target);
            Separator();
            DrawLastSection((RecastGraph)target);
            Separator();
            base.OnInspectorGUI(target); 
        }

        void DrawShape(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Shape", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var newCenter = EditorGUILayout.Vector3Field("Center", graph.forcedBoundsCenter);
            var newSize = EditorGUILayout.Vector3Field("Size", graph.forcedBoundsSize);
            var newRotation = EditorGUILayout.Vector3Field("Rotation", graph.rotation); 
            
            if (EditorGUI.EndChangeCheck())
            {
                graph.forcedBoundsCenter = newCenter;
                graph.forcedBoundsSize = newSize;
                graph.rotation = newRotation;
            }
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.LabelField("Size", $"{graph.tileXCount * 6} x {graph.tileZCount * 6} voxels, divided into {graph.GetTiles()?.Count() ?? 0} tiles");

            EditorGUI.BeginChangeCheck();
            var check = GUILayout.Button("Snap bounds to Scene");
            if (EditorGUI.EndChangeCheck() && check)
            {
                graph.SnapForceBoundsToScene();
            }
        }

        void DrawInputFiltering(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Input Filtering", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var mask = EditorGUILayoutx.LayerMaskField("Layer Mask", graph.mask);
            var obstacleMask = EditorGUILayoutx.LayerMaskField("Obstacle Mask", graph.obstacleMask);
            if (EditorGUI.EndChangeCheck())
            {
                graph.mask = mask;
                graph.obstacleMask = obstacleMask;
            }

            EditorGUI.BeginChangeCheck();
            var rasterizeTerrain = EditorGUILayout.Toggle("Rasterize Terrain", graph.rasterizeTerrain);
            var rasterizeMeshes = EditorGUILayout.Toggle("Rasterize Meshes", graph.rasterizeMeshes);
            var rasterizeColliders = EditorGUILayout.Toggle("Rasterize Colliders", graph.rasterizeColliders);
            var rasterizeTrees = EditorGUILayout.Toggle("Rasterize Trees", graph.rasterizeTrees);

            if (!EditorGUI.EndChangeCheck())
                return;

            graph.rasterizeTerrain = rasterizeTerrain;
            graph.rasterizeMeshes = rasterizeMeshes;
            graph.rasterizeColliders = rasterizeColliders;
            graph.rasterizeTrees = rasterizeTrees;
        }

        void DrawAgentCharacteristic(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Agent Characteristic", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var characterRadius = EditorGUILayout.FloatField("Character Radius", graph.characterRadius);
            var walkableHeight = EditorGUILayout.FloatField("Character Height", graph.walkableHeight);
            var maxStepHeight = EditorGUILayout.Slider("Max Slope", graph.maxSlope, 0, 90);
            if (!EditorGUI.EndChangeCheck())
                return;

            graph.characterRadius = characterRadius;
            graph.walkableHeight = walkableHeight;
            graph.maxSlope = maxStepHeight;
        }

        void DrawRasterization(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Rasterization", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var voxelSize = EditorGUILayout.FloatField("Voxel Size", graph.cellSize);
            var useTiles = EditorGUILayout.Toggle("Use Tiles", graph.useTiles);

            if (EditorGUI.EndChangeCheck())
            {
                graph.cellSize = voxelSize;
                graph.useTiles = useTiles;
            }

            EditorGUI.indentLevel++;
            if (graph.useTiles)
            {
                EditorGUI.BeginChangeCheck();
                var tileSize = EditorGUILayout.IntField("Tile size (voxels)", graph.editorTileSize);
                if (EditorGUI.EndChangeCheck())
                    graph.editorTileSize = tileSize;
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var cellSize = EditorGUILayout.FloatField("Cell size (voxels)", graph.cellSize);
                if (EditorGUI.EndChangeCheck())
                    graph.cellSize = cellSize;
            }
            EditorGUI.indentLevel--;

            EditorGUI.BeginChangeCheck();
            var maxEdgeLength = EditorGUILayout.FloatField("Max Border Edge Length", graph.maxEdgeLength);
            var minRegionSize = EditorGUILayout.FloatField("Min Region Size", graph.minRegionSize);
            var colliderDetail = EditorGUILayout.FloatField("Round Collider Detail", graph.colliderRasterizeDetail);

            if (EditorGUI.EndChangeCheck())
            {
                graph.maxEdgeLength = maxEdgeLength;
                graph.minRegionSize = minRegionSize;
                graph.colliderRasterizeDetail = colliderDetail;
            }
        }

        void DrawRuntimeSetting(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Runtime Setting", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }
        void DrawLastSection(RecastGraph graph)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(18);
            graph.showMeshSurface = GUILayout.Toggle(graph.showMeshSurface, new GUIContent("Show surface", "Toggles gizmos for drawing the surface of the mesh"), EditorStyles.miniButtonLeft);
            graph.showMeshOutline = GUILayout.Toggle(graph.showMeshOutline, new GUIContent("Show outline", "Toggles gizmos for drawing an outline of the nodes"), EditorStyles.miniButtonMid);
            graph.showNodeConnections = GUILayout.Toggle(graph.showNodeConnections, new GUIContent("Show connections", "Toggles gizmos for drawing node connections"), EditorStyles.miniButtonRight);
            GUILayout.EndHorizontal();

        }

    }
}
