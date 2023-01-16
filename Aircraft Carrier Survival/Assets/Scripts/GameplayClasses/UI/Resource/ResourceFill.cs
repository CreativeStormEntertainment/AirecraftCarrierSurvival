using UnityEngine;
using UnityEngine.UI;

public class ResourceFill : MonoBehaviour
{
    public Color OkColor = new Color(.54f, .745f, .28f);
    public Color LowColor = new Color(.57f, .38f, .19f);
    public Color CriticalColor = new Color(.69f, .255f, .15f);

    private Image fill;
    private ResourceIndicator indicator;

    protected virtual void Start()
    {
        fill = GetComponent<Image>();
        indicator = GetComponentInChildren<ResourceIndicator>();
    }

    public void SetFill(float progress)
    {
        fill.fillAmount = progress;
        if (progress > .5f)
            fill.color = OkColor;
        else if (progress > .25f)
            fill.color = LowColor;
        else
            fill.color = CriticalColor;

        indicator.SetIndicator(progress);
    }
}
