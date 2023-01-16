using UnityEngine;

public class DayNightSettings : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    public int W;
    public int H;

    private RenderTexture rend;

    private void Awake()
    {
        SetNewRenderTexture();
    }

    private void Update()
    {
        if (W != Camera.main.pixelWidth || H != Camera.main.pixelHeight)
        {
            rend.Release();
            SetNewRenderTexture();
        }
    }

    private void SetNewRenderTexture()
    {
        rend = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, -2);
        W = Camera.main.pixelWidth;
        H = Camera.main.pixelHeight;
        //rend.format = RenderTextureFormat.ARGB4444;
        cam.targetTexture = rend;
        mat.SetTexture("_MaskTexture", rend);
    }
}