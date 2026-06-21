using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace MyOwlcatModification
{
    public class MeshImporter : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject gameObject)
        {
            return;
            //try
            //{
            //    var renderer = gameObject.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            //    if (renderer == null)
            //        return;
            //    var mesh = renderer.sharedMesh;
            //    if (mesh == null)
            //        return;
            //    var rotation = renderer.transform.localRotation;
            //    var matrix = renderer.transform.worldToLocalMatrix;
            //    var isIdentity = rotation == Quaternion.identity;
            //    if (isIdentity)
            //        return;
            //    var inverse = Quaternion.Inverse( rotation );
            //    mesh.vertices = mesh.vertices.Select(v => rotation * v).ToArray();
            //    mesh.bindposes = mesh.bindposes.Select(b => b * matrix).ToArray();

            //    mesh.RecalculateBounds();
            //    mesh.RecalculateNormals();
            //    mesh.RecalculateTangents();

            //    renderer.transform.transform.localPosition = Vector3.zero;
            //    renderer.transform.transform.localRotation = Quaternion.identity;

            //}
            //catch (System.Exception ex)
            //{
            //    Debug.LogException(ex);
            //}
        }
    }
}
