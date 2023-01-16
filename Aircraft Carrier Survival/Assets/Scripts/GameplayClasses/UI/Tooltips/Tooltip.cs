using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using FMODUnity;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance = null;

    public string SegmentStatusDesc = "SegmentStatusDesc";
    public string SegmentStatusClear = "SegmentStatusClear";
    public string SegmentStatusFire = "SegmentStatusFire";
    public string SegmentStatusFlood = "SegmentStatusFlood";
    public string SegmentStatusInjured = "SegmentStatusInjured";
    public string SegmentStatusFault = "SegmentStatusFault";
    public string SegmentStatusDestroyed = "SegmentStatusDestroyed";

    public SubSectionRoom CurrentRoom
    {
        get;
        set;
    }

    [SerializeField]
    private Canvas canvas = null;
    [SerializeField]
    private RectTransform canvasRect = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private RectTransform textBoxContainer = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private Text sectionDescription = null;
    [SerializeField]
    private Text sectionTitle = null;
    [SerializeField]
    private Text sectionStatus = null;
    [SerializeField]
    private Color descriptionColor = new Color(1f, 1f, 1f);
    [SerializeField]
    private RectTransform arrow = null;
    [SerializeField]
    private RectTransform background = null;
    [SerializeField]
    private StudioEventEmitter enableSound = null;

    private Vector2 anchoredPosition = default;
    private Quaternion arrowDefaultRotation = default;

    private Transform targetTransform = null;
    private Transform tooltipLayer = null;
    private TooltipCaller caller = null;

    private bool isTargetAnimated = false;
    private Color normalDescriptionColor = new Color(0.196f, 0.196f, 0.196f);

    private Vector3 middlePointVector;

    private float baseMaxWidth;
    private ENeighbourDirection direction;

    private Quaternion upRotation = Quaternion.identity;
    private Quaternion downRotation = Quaternion.Euler(0f, 0f, 180f);
    private Quaternion leftRotation = Quaternion.Euler(0f, 0f, 90f);
    private Quaternion rightRotation = Quaternion.Euler(0f, 0f, -90f);

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        tooltipLayer = transform.parent;
        baseMaxWidth = textBoxContainer.rect.width;

        var locMan = LocalizationManager.Instance;
        SegmentStatusDesc = locMan.GetText(SegmentStatusDesc);
        SegmentStatusClear = locMan.GetText(SegmentStatusClear);
        SegmentStatusFire = locMan.GetText(SegmentStatusFire);
        SegmentStatusFlood = locMan.GetText(SegmentStatusFlood);
        SegmentStatusInjured = locMan.GetText(SegmentStatusInjured);
        SegmentStatusFault = locMan.GetText(SegmentStatusFault);
        SegmentStatusDestroyed = locMan.GetText(SegmentStatusDestroyed);
    }

    private void Start()
    {
        arrowDefaultRotation = arrow.rotation;
        gameObject.SetActive(false);
        middlePointVector = new Vector3(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f, 0f);
    }

    private void LateUpdate()
    {
        if (isTargetAnimated && targetTransform)
        {
            rectTransform.position = targetTransform.position + (Vector3)(canvas.scaleFactor * anchoredPosition);
        }
    }

    public void UpdateText(string title, string description)
    {
        this.title.text = title;
        if (string.IsNullOrWhiteSpace(description))
        {
            this.description.gameObject.SetActive(false);
        }
        else
        {
            this.description.gameObject.SetActive(true);
            this.description.text = description;
        }
        this.description.color = normalDescriptionColor;
    }

    public void UpdateSectionDesc(string desc, string status)
    {
        sectionDescription.text = desc;
        sectionStatus.text = status;
        //if (isWorking)
        //{
        //    sectionDescription.color = normalDescriptionColor;
        //}
        //else
        //{
        //    sectionDescription.color = descriptionColor;
        //}
    }

    public void SetupSectionTooltip(string header, string desc, string status, bool isWorking, SubSectionRoom room)
    {
        enableSound.Play();
        sectionTitle.text = header;
        sectionStatus.gameObject.SetActive(true);
        UpdateSectionDesc(desc, status);

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        var pos = Input.mousePosition / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x, -rectTransform.sizeDelta.y / 2f);
        rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(pos.x, -canvasRect.sizeDelta.x / 2f + rectTransform.sizeDelta.x, canvasRect.sizeDelta.x / 2f - rectTransform.sizeDelta.x), Mathf.Clamp(pos.y, -canvasRect.sizeDelta.y / 2f + rectTransform.sizeDelta.y, canvasRect.sizeDelta.y / 2f - rectTransform.sizeDelta.y));
        //rectTransform.anchoredPosition = Input.mousePosition / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x / 2f, -rectTransform.sizeDelta.y / 2f);
        arrow.gameObject.SetActive(false);
        gameObject.SetActive(true);
        CurrentRoom = room;
    }

    public void SetupSquadronTooltip(string title, string desc, Vector3 viewportPos)
    {
        enableSound.Play();
        sectionTitle.text = title;
        sectionDescription.text = desc;
        sectionStatus.gameObject.SetActive(false);

        rectTransform.anchorMax = new Vector2(.5f, .5f);
        rectTransform.anchorMin = rectTransform.anchorMax;
        rectTransform.pivot = rectTransform.anchorMin;
        var pos = viewportPos / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x, -rectTransform.sizeDelta.y / 2f);
        rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(pos.x, -canvasRect.sizeDelta.x / 2f + rectTransform.sizeDelta.x, canvasRect.sizeDelta.x / 2f - rectTransform.sizeDelta.x), Mathf.Clamp(pos.y, -canvasRect.sizeDelta.y / 2f + rectTransform.sizeDelta.y, canvasRect.sizeDelta.y / 2f - rectTransform.sizeDelta.y));
        //rectTransform.anchoredPosition = Input.mousePosition / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x / 2f, -rectTransform.sizeDelta.y / 2f);
        arrow.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ShowNotCanvasTooltip(string header, string desc)
    {
        enableSound.Play();
        sectionTitle.text = header;
        sectionDescription.text = desc;
        sectionStatus.gameObject.SetActive(false);

        CurrentRoom = null;
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        var pos = Input.mousePosition / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x, -rectTransform.sizeDelta.y / 2f);
        rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(pos.x, -canvasRect.sizeDelta.x / 2f + rectTransform.sizeDelta.x, canvasRect.sizeDelta.x / 2f - rectTransform.sizeDelta.x), Mathf.Clamp(pos.y, -canvasRect.sizeDelta.y / 2f + rectTransform.sizeDelta.y, canvasRect.sizeDelta.y / 2f - rectTransform.sizeDelta.y));
        //rectTransform.anchoredPosition = Input.mousePosition / canvasRect.localScale.x - middlePointVector + new Vector3(rectTransform.sizeDelta.x / 2f, -rectTransform.sizeDelta.y / 2f);
        arrow.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }


    public void Show(string title, string description, float arrowOffsetX, float arrowOffsetY, RectTransform parent, TooltipCaller caller, Vector2 bottomPos, bool isParentAnimated, float width = 0f, ENeighbourDirection direct = ENeighbourDirection.Up)
    {
        enableSound.Play();
        width = width == 0f ? baseMaxWidth : width;
        this.isTargetAnimated = isParentAnimated;

        UpdateText(title, description);

        this.caller = caller;
        caller.Disabled += OnCallerDisabled;
        arrow.gameObject.SetActive(true);
        arrow.SetParent(parent);
        arrow.gameObject.SetActive(true);
        arrow.pivot = new Vector2(.5f, 1f);
        arrow.localPosition = bottomPos;
        arrow.localScale = Vector3.one;
        background.anchoredPosition = Vector2.zero;
        CurrentRoom = null;
        //background.localPosition = Vector2.zero;
        switch (direct)
        {
            case ENeighbourDirection.Up:
                arrow.rotation = upRotation;
                background.localRotation = upRotation;
                background.anchorMin = Vector2.zero;
                background.anchorMax = Vector2.zero;
                background.pivot = new Vector2(0f, 1f);
                background.anchoredPosition = new Vector2(-Mathf.Lerp(0f, background.rect.width - arrow.rect.width, arrowOffsetX), 0f);
                break;
            case ENeighbourDirection.Down:
                arrow.rotation = downRotation;
                background.localRotation = downRotation;
                background.anchorMin = background.anchorMax = new Vector2(1f, 0f);
                background.pivot = Vector2.zero;
                background.anchoredPosition = new Vector2(Mathf.Lerp(0f, background.rect.width - arrow.rect.width, arrowOffsetX), 0f);
                break;
            case ENeighbourDirection.Left:
                arrow.rotation = leftRotation;
                background.localRotation = rightRotation;
                background.anchorMin = background.anchorMax = Vector2.zero;
                background.pivot = Vector2.zero;
                background.anchoredPosition = new Vector2(-Mathf.Lerp(0f, background.rect.height - arrow.rect.width, arrowOffsetY), 0f);
                break;
            case ENeighbourDirection.Right:
                //arrow.pivot = new Vector2(0f, .5f);
                //arrow.localPosition = Vector2.zero;
                //arrow.localScale = Vector3.one;
                arrow.rotation = rightRotation;
                background.localRotation = leftRotation;
                background.anchorMin = background.anchorMax = new Vector2(1f, 0f);
                background.pivot = new Vector2(1f, 0f);
                background.anchoredPosition = new Vector2(Mathf.Lerp(0f, background.rect.height - arrow.rect.width, arrowOffsetY), 0f);
                break;
            default:
                break;
        }
        arrow.SetParent(tooltipLayer, true);


        /*rectTransform.SetParent(parent, false);
        rectTransform.rotation = Quaternion.identity;
        rectTransform.localPosition = Vector3.zero;

        textBoxContainer.sizeDelta = new Vector2 (width, textBoxContainer.sizeDelta.y);

        Vector2 anchors = Vector2.zero;
        arrow.gameObject.SetActive(true);
        arrow.anchorMin = arrow.anchorMax = anchors;
        Vector3 arrowEuler = arrowDefaultRotation.eulerAngles;
        switch (direct)
        {
            case ENeighbourDirection.Up:
                anchors = arrow.anchorMin = arrow.anchorMax = new Vector2(0f, 1f);
                arrow.pivot = new Vector2(0.5f, 1f);
                arrow.anchoredPosition = new Vector2(textBoxContainer.rect.width * arrowOffsetX, textBoxContainer.rect.height * arrowOffsetY);
                arrow.rotation = arrowDefaultRotation;
                break;
            case ENeighbourDirection.Down:
                anchors = arrow.anchorMin = arrow.anchorMax = new Vector2(0f, 0f);
                arrow.pivot = new Vector2(0.5f, 1f);
                arrow.anchoredPosition = new Vector2(textBoxContainer.rect.width * arrowOffsetX, textBoxContainer.rect.height * arrowOffsetY);
                arrow.rotation = Quaternion.Euler(arrowEuler.x, arrowEuler.y, arrowEuler.z + 180f);
                break;
            case ENeighbourDirection.Left:
                anchors = arrow.anchorMin = arrow.anchorMax = arrow.pivot = new Vector2(0f, 0f);
                arrow.anchoredPosition = new Vector2(textBoxContainer.rect.width * arrowOffsetX, textBoxContainer.rect.height * arrowOffsetY);
                arrow.rotation = Quaternion.Euler(arrowEuler.x, arrowEuler.y, arrowEuler.z + 90f);
                break;
            case ENeighbourDirection.Right:
                anchors = arrow.anchorMin = arrow.anchorMax = new Vector2(1f, 0f);
                arrow.pivot = new Vector2(1f, 1f);
                arrow.anchoredPosition = new Vector2(textBoxContainer.rect.width * arrowOffsetX, textBoxContainer.rect.height * arrowOffsetY);
                arrow.rotation = Quaternion.Euler(arrowEuler.x, arrowEuler.y, arrowEuler.z + 270f);
                break;
            default:
                break;
        }

        rectTransform.pivot = anchors;
        //anchors.y = 0f;
        //bottomPos -= arrowPos;
        rectTransform.anchorMin = rectTransform.anchorMax = anchors;
        anchoredPosition = bottomPos;

        if (isParentAnimated)
        {
            rectTransform.position = transform.parent.position + (Vector3)(canvas.scaleFactor * anchoredPosition);
        }
        else
        {
            rectTransform.anchoredPosition = anchoredPosition;
        }

        targetTransform = parent;

        rectTransform.SetParent(tooltipLayer, true);
        rectTransform.localScale = Vector3.one;
        gameObject.SetActive(true);*/
    }

    public void Hide()
    {
        if (caller != null)
        {
            caller.Disabled -= OnCallerDisabled;
        }
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.rotation = Quaternion.identity;
        }
        targetTransform = null;
        isTargetAnimated = false;
        gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);
    }

    private void OnWillRenderObject()
    {
        rectTransform.sizeDelta = background.sizeDelta;
    }

    private void OnCallerDisabled()
    {
        caller.OnExit();
    }
}
