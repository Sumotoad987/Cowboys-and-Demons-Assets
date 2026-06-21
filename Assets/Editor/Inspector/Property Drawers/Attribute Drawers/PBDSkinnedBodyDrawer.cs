using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using System.Linq;
using static System.Reflection.BindingFlags;
using Unity.Mathematics;
using System.Reflection;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

namespace MyOwlcatModification
{
    [CustomEditor(typeof(PBDSkinnedBody), true)]
    public class PBDSkinnedBodyDrawer : UnityEditor.Editor
    {
        static FieldInfo BonesMap = typeof(PBDSkinnedBody).GetField("m_BoneIndicesMapOffsetCount", Instance | NonPublic);
        static GUIContent content = new GUIContent("Set Up Renderers");

        public override void OnInspectorGUI()
        {

            if (GUILayout.Button( content))
                SetUpComponent(target as PBDSkinnedBody);

            base.OnInspectorGUI();
        }

        static void SetUpComponent(PBDSkinnedBody body)
        {
            //for (int i = 0; i < body.SkinnedMeshRenderers.Length; i++)
            //{
            //    var ren = body.SkinnedMeshRenderers[i];
            //    var map = body.BoneIndicesMapOffsetCount[i];
            //    for (int j = map.x; j < map.y; j++)
            //    {
            //        Vector3 position = Vector3.zero;
            //        Quaternion orientation = Quaternion.identity;
            //        int currentBone = j;

            //        while (-1 != (currentBone))
            //        {
            //            var bone = body.Bones[currentBone];
            //            position = bone.localPosition + position;
            //            orientation = bone.localRotation* orientation;
            //            currentBone =  body.ParentMap[currentBone];
            //        }

            //        body.Particles[j] = new Particle()
            //        {
            //            BasePosition = position,
            //            Position = position,
            //            Orientation = orientation,
            //            PredictedOrientation = orientation,
            //            Radius = 0.1f,
            //            Mass = 2.5f
            //        };
            //    }
            //}
            //return;


            body.InitRenderers();
            body.LocalColliders.Clear();
            body.LocalColliders.AddRange(body.gameObject.GetComponentsInChildren<PBDColliderBase>());
            body.Bones.Clear();
            body.Bones.AddRange(body.SkinnedMeshRenderers.SelectMany(renderer => renderer.bones).Distinct());
            body.Bindposes.Clear();
            body.Bindposes.AddRange(body.SkinnedMeshRenderers.SelectMany(renderer => renderer.sharedMesh.bindposes));
            body.BonesPerVertex.Clear();
            body.BonesPerVertex.AddRange(body.SkinnedMeshRenderers.Select(renderer => (int)renderer.sharedMesh.GetBonesPerVertex().Max()));
            var bonesMap = new List<int2>();
            int sum = 0;
            int current = 0;
            foreach (var renderer in body.SkinnedMeshRenderers)
            {
                current = renderer.bones.Length;
                bonesMap.Add(new int2(sum, current));
                sum += current;
            }
            BonesMap.SetValue(body, bonesMap);

            body.BoneIndicesMap.Clear();
            foreach (var bone in body.SkinnedMeshRenderers.SelectMany(renderer => renderer.bones))
                body.BoneIndicesMap.Add(body.Bones.IndexOf(bone));

            body.ParentMap.Clear();
            body.ParentMap.AddRange(body.Bones.Select(bone => body.Bones.IndexOf(bone.parent)));

            body.Particles.Clear();
            foreach (var bone in body.Bones)
                body.Particles.Add(new Particle()
                {
                    BasePosition = bone.localPosition,
                    Position = bone.localPosition,
                    Predicted = bone.localPosition,
                    Orientation = bone.localRotation,
                    PredictedOrientation = bone.localRotation

                });
            AssetDatabase.Refresh();


        }
    }
}
