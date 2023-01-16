using UnityEngine;

public class SubsectionIcons : Icon
{
    public GameObject TotalDestruction;
    public GameObject Destruction;
    public GameObject Water;
    public GameObject Fire;
    public GameObject Break;
    public Renderer WaterRenderer;
    public Renderer FireRenderer;
    public Renderer BreakRenderer;

    private void Awake()
    {
        Water.SetActive(false);
        Fire.SetActive(false);
        Break.SetActive(false);
    }

    public void SetBreakFill(float fill)
    {
        SetFill(BreakRenderer, fill);
    }

    public void SetFireFill(float fill)
    {
        SetFill(FireRenderer, fill);
    }

    public void SetWaterFill(float fill)
    {
        SetFill(WaterRenderer, fill);
    }
}
