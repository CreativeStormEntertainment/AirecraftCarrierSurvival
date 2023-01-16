using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarkerHighlight : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public EMarkerType MarkerType
    {
        get;
        set;
    }
    [NonSerialized]
    public TacticalEnemyShip EnemyShip;
    [NonSerialized]
    public TacticalObject TacticalObject;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private EnemyInfoPanel infoPanel = null;
    [SerializeField]
    private Image image = null;

    private RectTransform rectTransform;
    private bool clickable;
    private bool allowClick;
    private bool showPanel;

    private int outdatedTicks;

    private void Start()
    {
        rectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        var tacMan = TacticManager.Instance;
        tacMan.Map.MarkerHighlightChanged -= OnMarkerHighlightChanged;
        tacMan.CurrentMissionChanged -= OnCurrentMissionChanged;
        tacMan.Map.MarkerHighlightChanged += OnMarkerHighlightChanged;
        tacMan.CurrentMissionChanged += OnCurrentMissionChanged;
        OnCurrentMissionChanged(tacMan.CurrentMission);
        //  rangeImage.sizeDelta = new Vector2(EnemyShip.AttackRange, EnemyShip.AttackRange);
    }

    private void OnDisable()
    {
        TacticManager.Instance.CurrentMissionChanged -= OnCurrentMissionChanged;
        if (infoPanel != null)
        {
            infoPanel.Hide();
        }
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void Highlight(bool on)
    {
        highlight.SetActive(on);
        allowClick = on;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var tacMan = TacticManager.Instance;
        if (allowClick && CheckRange())
        {
            tacMan.Map.OnPointerClick(eventData);

            tacMan.Map.HighlightMarkers(false, MarkerType);

            tacMan.ConfirmedTarget = TacticalObject;
            tacMan.ChosenEnemyShip = EnemyShip;
            if (showPanel)
            {
                tacMan.StrategySelectionPanel.ShowPanel(EnemyShip);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoPanel != null && EnemyShip != null && EnemyShip.Side != ETacticalObjectSide.Neutral)
        {
            infoPanel.ShowPanel(EnemyShip, outdatedTicks);
            infoPanel.transform.SetParent(TacticManager.Instance.EnemyInfoPanelParent);
            infoPanel.transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel != null && EnemyShip != null && EnemyShip.Side != ETacticalObjectSide.Neutral)
        {
            infoPanel.Hide();
            infoPanel.transform.SetParent(rectTransform);
        }
    }

    public void SetOutdatedReportText(int ticks)
    {
        outdatedTicks = ticks;
    }

    private void OnMarkerHighlightChanged(bool value, EMarkerType type, List<TacticalEnemyShip> objectsToHighlight)
    {
        if (objectsToHighlight == null)
        {
            if (MarkerType == type)
            {
                Highlight(value);
            }
        }
        else
        {
            if (objectsToHighlight.Contains(EnemyShip))
            {
                Highlight(value);
            }
        }
    }

    private void OnCurrentMissionChanged(TacticalMission mission)
    {
        var currentMission = mission;
        if (currentMission)
        {
            showPanel = TacticalMission.AirstrikeMissions.Contains(currentMission.OrderType);
        }
        else
        {
            showPanel = false;
        }
    }

    private bool CheckRange()
    {
        var tacMan = TacticManager.Instance;
        float maxDist = tacMan.DistanceOnFuel;
        if (tacMan.CurrentMission != null && TacticalMission.SwitchPlaneTypeMissions.Contains(tacMan.CurrentMission.OrderType) && !tacMan.CurrentMission.UseTorpedoes)
        {
            maxDist *= 1.25f;
        }
        maxDist *= maxDist;
        return Vector2.SqrMagnitude(tacMan.Map.MapShip.Position - rectTransform.anchoredPosition) < maxDist;
    }
}
