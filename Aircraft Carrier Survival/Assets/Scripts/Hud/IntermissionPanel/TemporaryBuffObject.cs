using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TemporaryBuffObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool Blink
    {
        get;
        set;
    }

    [SerializeField]
    private CampaignMapPanel panel = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Text desc2 = null;

    [SerializeField]
    private GameObject hover = null;

    [SerializeField]
    private Image frame = null;

    [SerializeField]
    private GameObject tooltip = null;

    [SerializeField]
    private float blinkTime = .5f;

    [SerializeField]
    private StudioEventEmitter emitter = null;

    private float current;

    private void Update()
    {
        if (!Blink || Time.timeScale < .1f || frame == null)
        {
            return;
        }

        current += Time.unscaledDeltaTime / blinkTime;
        if (current > 1f)
        {
            current -= 1f;
        }
        var color = frame.color;
        color.a = 4f * current * (1f - current);
        frame.color = color;
    }

    public void Setup(ETemporaryBuff buff, bool active)
    {
        var data = panel.Buffs[buff];
        title.text = data.Name;
        desc.text = data.Description;

        desc2.text = active ? data.ActiveDescription : data.InactiveDescription;

        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover.SetActive(true);
        if (tooltip != null)
        {
            tooltip.SetActive(true);
            emitter.Play();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover.SetActive(false);
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
}
