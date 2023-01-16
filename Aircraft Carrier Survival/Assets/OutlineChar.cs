using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutlineChar : MonoBehaviour
{
    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            outlineColor = value;
            needsUpdate = true;
        }
    }
    public float OutlineWidth
    {
        get => outlineWidth;
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    public bool OutlineVisible
    {
        get => visibleOutline;
        set
        {
            visibleOutline = value;
            needsUpdate = true;
        }
    }

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    [SerializeField]
    private Material outlineMaskMaterial = null;

    [SerializeField]
    private Material outlineFillMaterial = null;

    [SerializeField]
    private bool visibleOutline = false;

    private bool needsUpdate;
    private Renderer[] renderers;

    private void Start()
    {
        // Cache renderers
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = Instantiate(outlineMaskMaterial);
        outlineFillMaterial = Instantiate(outlineFillMaterial);

        LoadSmoothNormals();
        needsUpdate = true;

        Init();
    }

    private void OnEnable()
    {
        if (renderers != null)
        {
            Init();
        }
    }

    private void OnValidate()
    {
        needsUpdate = true;

        if (bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (bakeKeys.Count == 0)
        {
            Bake();
        }

    }

    private void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    private void OnDisable()
    {
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                renderer.materials = new Material[0];
            }
        }
    }

    private void Bake()
    {
        // Generate smooth normals for each mesh
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            // Skip duplicates
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Serialize smooth normals
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    private void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
        }

        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
            }
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                continue;
            }

            var smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += mesh.normals[pair.Value];
            }

            smoothNormal.Normalize();

            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    private void UpdateMaterialProperties()
    {

        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
        if (visibleOutline)
        {
            outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
        else
        {
            outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
        }

        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlinePingPong", 0f);
    }

    private void Init()
    {
        foreach (var renderer in renderers)
        {
            // Append outline shaders
            var materials = new Material[2];

            materials[0] = outlineMaskMaterial;
            materials[1] = outlineFillMaterial;

            renderer.materials = materials;
        }
    }

    public void SetVisibility(bool value)
    {
        OutlineWidth = value ? 2f : 0f;
        //Debug.Log($"Hollos {(value ? "Show" : "Hide")}");
    }
}
