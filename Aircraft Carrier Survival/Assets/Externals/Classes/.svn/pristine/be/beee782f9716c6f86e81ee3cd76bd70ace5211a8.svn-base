using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GambitUtils
{
    public struct MeshUtils
    {
        public static Bounds GetBounds(params MeshFilter[] meshes)
        {
            if (meshes.Length < 1)
            {
                return default;
            }
            var bounds = meshes[0].sharedMesh.bounds;
            foreach (var mesh in meshes)
            {
                var meshBounds = mesh.sharedMesh.bounds;
                var meshCenter = meshBounds.center;
                var meshExtents = Mathv.Mul(meshBounds.extents, mesh.transform.localScale);
                bounds.min = Vector3.Min(bounds.min, meshCenter - meshExtents);
                bounds.max = Vector3.Max(bounds.max, meshCenter + meshExtents);
            }
            return bounds;
        }

        public static Bounds GetBounds(params MeshRenderer[] renderers)
        {
            var filters = new MeshFilter[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                filters[i] = renderers[i].GetComponent<MeshFilter>();
            }
            return GetBounds(filters);
        }
    }
}

