using FMODUnity;
using PathCreation;
using System.Collections.Generic;
using UnityEngine;

public class DragPlanesManager : MonoBehaviour, IEnableable
{
    public static DragPlanesManager Instance = null;

    public EManeuverSquadronType SquadronTypeEnabled
    {
        get;
        set;
    }

    public List<PlaneDrag> PlanesLaunchingSpots
    {
        get;
        private set;
    }

    public List<PlaneDrag> PlanesRecoverySpots
    {
        get;
        private set;
    }

    public LiftIcon LiftIcon => liftIcon;

    public float SnapDistanceSqr => snapDistanceSqr;

    public Transform PlaneElevatorPoint => planeElevatorPoint;

    public bool LiftPanelIsOpen => liftPanel.IsOpen;

    public string InactiveSlotTooltipTitle => inactiveSlotTitle;
    public string InactiveSlotTooltipDesc => AircraftCarrierDeckManager.Instance.HasDamage ? inactiveSlotWreckDesc : inactiveSlotLessSlots;

    [SerializeField]
    private RectTransform canvasRect = null;

    [SerializeField]
    private Transform planeElevatorPoint = null;

    [SerializeField]
    private LiftPanel liftPanel = null;
    [SerializeField]
    private LineRenderer lineRenderer = null;
    [SerializeField]
    private float snapDistanceSqr = 500f;

    [SerializeField]
    private Transform planesLaunchingSpots = null;
    [SerializeField]
    private Transform planesRecoverySpots = null;

    [SerializeField]
    private GameObject liftHighlight = null;

    [SerializeField]
    private PathCreator creator = null;
    [SerializeField]
    private float minHeight = 2f;
    [SerializeField]
    private float maxHeight = 20f;

    [SerializeField]
    private LiftIcon liftIcon = null;

    [SerializeField]
    private string inactiveSlotTitle = "DeckSlotIsLocked";
    [SerializeField]
    private string inactiveSlotWreckDesc = "DeckLockedPlaneCrashed";
    [SerializeField]
    private string inactiveSlotLessSlots = "DeckLockedNoCrew";

    private Vector3[] list = new Vector3[3];

    private bool canShow;

    private bool originalDisabled;
    private bool selectedDisabled;

    private void Awake()
    {
        Instance = this;
        SquadronTypeEnabled = EManeuverSquadronType.Any;

        PlanesLaunchingSpots = new List<PlaneDrag>();
        foreach (Transform plane in planesLaunchingSpots)
        {
            if (plane.TryGetComponent(out PlaneDrag planeDrag))
            {
                PlanesLaunchingSpots.Add(planeDrag);
            }
            else
            {
                Debug.LogError($"{plane.name} has no PlaneDrag", plane);
            }
        }

        PlanesRecoverySpots = new List<PlaneDrag>();
        foreach (Transform plane in planesRecoverySpots)
        {
            if (plane.TryGetComponent(out PlaneDrag planeDrag))
            {
                PlanesRecoverySpots.Add(planeDrag);
            }
            else
            {
                Debug.LogError($"{plane.name} has no PlaneDrag", plane);
            }
        }

        var locMan = LocalizationManager.Instance;
        inactiveSlotTitle = locMan.GetText(inactiveSlotTitle);
        inactiveSlotWreckDesc = locMan.GetText(inactiveSlotWreckDesc);
        inactiveSlotLessSlots = locMan.GetText(inactiveSlotLessSlots);
    }

    private void Start()
    {
        CameraManager.Instance.ViewChanged += OnViewChanged;

        var deck = AircraftCarrierDeckManager.Instance;
        deck.DeckSquadronsCountChanged += UpdateMe;
        deck.WreckChanged += UpdateMe;
    }

    public void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        for (int i = 0; i < PlanesRecoverySpots.Count; ++i)
        {
            PlanesRecoverySpots[i].SetEnable(enable);
            PlanesRecoverySpots[i].SetSelectedEnable(enable && !selectedDisabled);
            PlanesLaunchingSpots[i].SetEnable(enable);
            PlanesLaunchingSpots[i].SetSelectedEnable(enable && !selectedDisabled);
        }
        if (!enable)
        {
            ShowLiftPanel(false);
        }
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }

    public void SetupHollows()
    {
        foreach (var spot in PlanesRecoverySpots)
        {
            spot.CreateHollows();
        }
        foreach (var spot in PlanesLaunchingSpots)
        {
            spot.CreateHollows();
        }
    }

    public void SetLine(Vector3 pos1, Vector3 pos2)
    {
        Vector3 pos = pos1 + pos2;
        pos /= 2f;
        pos.y += Mathf.Clamp(Vector3.Distance(pos1, pos2) / 2f, minHeight, maxHeight);

        list[0] = pos1;
        list[1] = pos;
        list[2] = pos2;
        creator.bezierPath = new BezierPath(list);

        lineRenderer.enabled = true;
        lineRenderer.positionCount = creator.path.NumPoints;
        for (int i = 0; i < creator.path.NumPoints; i++)
        {
            lineRenderer.SetPosition(i, creator.path.GetPoint(i));
        }
        LiftIcon.Show();
    }

    public void HideLine()
    {
        lineRenderer.enabled = false;
        LiftIcon.Hide();
    }

    public void SetLiftHighlight(bool show)
    {
        liftHighlight.SetActive(show);
    }

    public void ShowLiftPanel(bool show)
    {
        if (show)
        {
            liftPanel.SetPosition(Input.mousePosition / canvasRect.localScale.x);
        }
        liftPanel.SetShowPanel(show);
    }

    public void ForceEndDrag()
    {
        for (int i = 0; i < PlanesRecoverySpots.Count; ++i)
        {
            PlanesRecoverySpots[i].ForceEndDrag();
            PlanesLaunchingSpots[i].ForceEndDrag();
        }
    }

    public bool IsDraggingSquadron()
    {
        bool result = false;

        foreach (var launchingSpot in PlanesLaunchingSpots)
        {
            if (launchingSpot.IsDragging)
            {
                result = true;
                break;
            }
        }

        if (!result)
        {
            foreach (var recoverySpot in PlanesRecoverySpots)
            {
                if (recoverySpot.IsDragging)
                {
                    result = true;
                    break;
                }
            }
        }


        return result;
    }

    public void UpdateMe()
    {
        UpdateSpots();
        ForceEndDrag();
    }

    private void OnViewChanged(ECameraView view)
    {
        canShow = CameraManager.Instance.CurrentCameraView == ECameraView.Deck;
        UpdateMe();
        liftPanel.SetShowPanel(false);
    }

    private void UpdateSpots()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        var toUpdate = PlanesLaunchingSpots;
        var toDisable = PlanesRecoverySpots;

        if (deck.DeckMode == EDeckMode.Landing)
        {
            (toUpdate, toDisable) = (toDisable, toUpdate);
        }

        foreach (var spot in toUpdate)
        {
            spot.UpdateSpot(canShow);
            //spot.DestroyHollow();
            spot.HideAllHollows();
        }

        foreach (var spot in toDisable)
        {
            spot.UpdateSpot(false);
            //spot.DestroyHollow();
            spot.HideAllHollows();
        }

        if (canShow)
        {
            var orderCount = 0;
            int index = -1;

            foreach (var order in deck.OrderQueue)
            {
                if (order.OrderType == EOrderType.SquadronCreation)
                {
                    for (int i = 0; i < toUpdate.Count; i++)
                    {
                        if (toUpdate[i].HollowsVisible)
                        {
                            continue;
                        }

                        if (toUpdate[i].SquadronOnDeck)
                        {
                            if (deck.DeckSquadrons[i].AnimationPlay)
                            {
                                index = i;
                                break;
                            }
                            continue;
                        }

                        index = i;
                        break;
                    }
                    if (index == -1)
                    {
                        index = toUpdate.Count - 1;
                    }
                    //var squadron = PlaneMovementManager.Instance.CreateHollow(((SquadronCreationOrder)order).PlaneType);
                    //toUpdate[index].CreateHollow(squadron);
                    toUpdate[index].ShowHollows(((SquadronCreationOrder)order).PlaneType);
                }
                orderCount++;
            }
        }
    }
}
