using UnityEngine;
using UnityEngine.UI;

public class SandboxPopup : MonoBehaviour
{

    [SerializeField]
    protected Button buttonA = null;
    [SerializeField]
    protected Button buttonB = null;

    protected SandboxPoi poi;

    protected virtual void Start()
    {
        buttonA.onClick.AddListener(OnClickA);
        buttonB.onClick.AddListener(OnClickB);
    }

    public virtual void Show(SandboxPoi poi)
    {
        this.poi = poi;
        gameObject.SetActive(true);
        var hudMan = HudManager.Instance;
        hudMan.UnsetBlockSpeed(false);
        hudMan.SetBlockSpeed(0, false);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        var hudMan = HudManager.Instance;
        hudMan.UnsetBlockSpeed(false);
        hudMan.PlayLastSpeed(false);
    }

    protected virtual bool CheckInRange()
    {
        return Vector2.SqrMagnitude(poi.RectTransform.anchoredPosition - WorldMap.Instance.MapShip.Rect.anchoredPosition) <= SandboxManager.Instance.PoiManager.PoiRadiousSqr;
    }

    protected virtual void OnClickA()
    {
        Hide();
    }

    protected virtual void OnClickB()
    {
        Hide();
    }
}
