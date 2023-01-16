using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SandboxPoi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public SandboxPoiData Data
    {
        get;
        private set;
    }

    public bool InRange
    {
        get;
        private set;
    }

    public bool Locked
    {
        get;
        protected set;
    }

    public RectTransform RectTransform => rectTransform;

    protected SandboxNode poiNode;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private Image raycastTarget = null;
    [SerializeField]
    private StudioEventEmitter inRangeEmitterSmall = null;
    [SerializeField]
    private StudioEventEmitter inRangeEmitterBig = null;

    private Vector2 baseSize;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
        baseSize = RectTransform.sizeDelta;
    }

    protected virtual void Update()
    {
        UpdateInRange(Vector2.SqrMagnitude(RectTransform.anchoredPosition - WorldMap.Instance.MapShip.Rect.anchoredPosition) <= SandboxManager.Instance.PoiManager.PoiRadiousSqr);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.PoiInfoPanel.Show(this);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.PoiInfoPanel.StartHiding();
    }

    public virtual void Setup(SandboxPoiData data)
    {
        Data = data;
        var sandMan = SandboxManager.Instance;
        poiNode = sandMan.PoiManager.GetNode(Data.NodeIndex);
        Data.RegionIndex = WorldMap.Instance.GetClosestSector(RectTransform.anchoredPosition);
    }

    public virtual void Load(SandboxPoiData data)
    {
        Setup(data);
        poiNode.Occupied = true;
    }

    public virtual bool Tick()
    {
        Data.TicksToObsolete -= TimeManager.Instance.WorldMapTickQuotient;
        if (Data.TicksToObsolete <= 0)
        {
            SandboxManager.Instance.PoiManager.RemovePoi(this);
            return true;
        }
        return false;
    }

    public void RemovePoi()
    {
        Data = null;
        poiNode = null;
        gameObject.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        raycastTarget.raycastTarget = interactable;
    }

    public virtual void OnClick()
    {
        if (InRange)
        {
            BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
        }
        else
        {
            BackgroundAudio.Instance.PlayEvent(EIntermissionClick.InactiveClick);
            WorldMap.Instance.OnLeftClick();
        }
    }

    private void UpdateInRange(bool isInRange)
    {
        RectTransform.sizeDelta = isInRange ? baseSize * 1.5f : baseSize;
        var old = InRange;
        InRange = isInRange;
        if (old != InRange)
        {
            if (InRange)
            {
                inRangeEmitterBig?.Play();
            }
            else
            {
                inRangeEmitterSmall?.Play();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        WorldMap.Instance.PointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        WorldMap.Instance.PointerUp(eventData);
    }
}
