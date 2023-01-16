using UnityEngine;

public class TutorialMeshBlinker : TutorialBlinker
{
    private readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    private Material material;
    private Color baseEmission;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        baseEmission = material.GetColor(EmissionID);
    }

    protected override void SetColor(float power)
    {
        material.SetColor(EmissionID, baseEmission * power);
    }
}
