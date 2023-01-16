using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MissionWaypoint : ShipWaypoint
{
    //private Animator animator = null;

    //private readonly int isDragingHash = Animator.StringToHash("IsDraging");
    //private readonly int dragAnimHash = Animator.StringToHash("Drag");

    [SerializeField]
    private bool isRecoveryPoint = false;

    [SerializeField]
    private GameObject disableOnDragging = null;

    [SerializeField]
    private GameObject tipText = null;

    private float range;
    private float currentRange;
    private Image rangeImage;
    private bool set;

    private Vector2 lastPos;
    private Vector2 missionWaypointOriginPos;

    private void Start()
    {
        ignoreRedraw = true;
        currentRange = range = RangeCircle.sizeDelta.x / 2f;
        rangeImage = RangeCircle.gameObject.GetComponent<Image>();
        if (tipText != null)
        {
            tipText.SetActive(false);
        }
        SetRaycastTarget(set);
    }

    public void SetRaycastTarget(bool value)
    {
        set = value;
        if (rangeImage != null)
        {
            rangeImage.raycastTarget = value;
        }
    }

    public override void SetMap(WaypointMap map)
    {
        base.SetMap(map);
        //animator = GetComponent<Animator>();
        //animator.Play(dragAnimHash);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        ignoreRedraw = true;
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        base.OnBeginDrag(eventData);
        //animator.SetBool(isDragingHash, true);
        if (disableOnDragging != null)
        {
            disableOnDragging.SetActive(false);
        }
        if (tipText != null)
        {
            tipText.SetActive(true);
        }
        Map.MissionWaypointOutlineTransform.position = transform.position;
        Map.MissionWaypointOutlineTransform.gameObject.SetActive(true);
        lastPos = RangeCircle.anchoredPosition;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        ignoreRedraw = true;
        if (isDragged)
        {
            ////Debug.Log ("OnDrag");
            var newPos = map.GetEndPointFromMousePosition() - RectTransform.anchoredPosition;

            var tacMap = TacticalMap.Instance;
            if (isRecoveryPoint)
            {
                if (tacMap.RecoveryOnCarrier)
                {
                    currentRange = .8f;
                }
            }
            else
            {
                if (tacMap.ReconWaypointInUoRange && !TacticManager.Instance.IsUoInReconRange(newPos))
                {
                    return;
                }
            }

            if (newPos.sqrMagnitude < currentRange * currentRange)
            {
                RangeCircle.anchoredPosition = newPos;
            }
            else
            {
                RangeCircle.anchoredPosition = newPos.normalized * currentRange;
            }
            currentRange = range;
            //map.RedrawPotentialTrack(this, false, true, true);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        ignoreRedraw = true;
        base.OnEndDrag(eventData);
        //animator.SetBool(isDragingHash, false);
        //Map.ChooseOrderType.UpdateCurrentOrderText();
        if (disableOnDragging != null)
        {
            disableOnDragging.SetActive(true);
        }
        Map.MissionWaypointOutlineTransform.gameObject.SetActive(false);
        var tacMan = TacticManager.Instance;
        if (isRecoveryPoint)
        {
            tacMan.ReturnPosition = RectTransform.anchoredPosition + RangeCircle.anchoredPosition;
        }
        else
        {
            tacMan.AttackPosition = RectTransform.anchoredPosition + RangeCircle.anchoredPosition;
        }
        tacMan.RecalculateDistances();
        if (tipText != null)
        {
            tipText.SetActive(false);
        }
    }
}
