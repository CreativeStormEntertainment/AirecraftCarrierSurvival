using System.Collections.Generic;
using UnityEngine;

public class AircraftSkin : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials = null;

    [SerializeField]
    private List<Renderer> renderers = null;

    public void SetSkin(int index)
    {
        var material = materials[index];
        foreach (var renderer in renderers)
        {
            if (!renderer.sharedMaterial.name.Contains("glass"))
            {
                renderer.material = material;
            }
        }
    }
}
