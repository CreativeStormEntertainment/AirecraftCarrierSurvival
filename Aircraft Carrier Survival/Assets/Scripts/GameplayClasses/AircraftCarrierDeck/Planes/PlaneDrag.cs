using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaneDrag : MonoBehaviour, IEnableable
{
    public int ID
    {
        get
        {
            if (id == -1)
            {
                id = int.Parse(gameObject.name.Split('_')[1]) - 1;
            }
            return id;
        }
    }


    public bool SquadronOnDeck
    {
        get => ID < deck.DeckSquadrons.Count;
    }

    public List<Transform> Hollows = null;

    public Transform ChildTransform
    {
        get;
        private set;
    }

    public bool IsDragging
    {
        get => isDragging;
    }

    public bool HollowsVisible
    {
        get;
        private set;
    }

    private Vector3 screenPoint;
    private Vector3 lineStartPoint;

    private Vector3 startPoint;
    private Vector3 endPoint;

    private int id = -1;
    private AircraftCarrierDeckManager deck = null;
    private ACOrder order = null;
    private Collider col = null;
    private DragPlanesManager dpManager = null;
    private NewSquadronBtn squadronBtn;
    private GameObject slotHighlight;
    private GameObject newGameObject;
    private SpriteRenderer icon;

    private bool isDragging = false;
    private bool isMouseOver = false;
    private bool forceDragEnd = false;
    private bool inOrderQueue
    {
        get
        {
            if (order == null)
            {
                return false;
            }
            else
            {
                if (deck.OrderQueue.Contains(order))
                {
                    return true;
                }
                else
                {
                    order = null;
                    return false;
                }
            }
        }
    }
    private List<Transform> hollowSpots = null;

    private bool correctCameraView
    {
        get => CameraManager.Instance.CurrentCameraView == ECameraView.Deck;
    }

    private List<OutlineChar> fighterHollows = new List<OutlineChar>();
    private List<OutlineChar> bomberHollows = new List<OutlineChar>();
    private List<OutlineChar> torpedoHollows = new List<OutlineChar>();

    private Dictionary<EPlaneType, List<OutlineChar>> hollowsDict = new Dictionary<EPlaneType, List<OutlineChar>>();

    private bool disabled;
    private bool selectedDisabled;

    private void Awake()
    {
        col = transform.GetComponent<Collider>();
        var trans = transform;

        startPoint = trans.position;
        endPoint = startPoint;

        ChildTransform = trans.GetChild(0);
        lineStartPoint = ChildTransform.position;
        squadronBtn = ChildTransform.GetComponent<NewSquadronBtn>();

        slotHighlight = trans.GetChild(1).gameObject;
        slotHighlight.SetActive(false);

        newGameObject = trans.GetChild(2).gameObject;
        icon = newGameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        dpManager = DragPlanesManager.Instance;
        deck = AircraftCarrierDeckManager.Instance;

        hollowSpots = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.Equals("Spot"))
            {
                hollowSpots.Add(child);
            }
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            var hudMan = HudManager.Instance;
            if (hudMan.IsSettingsOpened || !hudMan.AcceptInput)
            {
                return;
            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                endPoint = hit.point;
                dpManager.SetLine(lineStartPoint, hit.point);

                float distance = Vector3.SqrMagnitude(endPoint - dpManager.PlaneElevatorPoint.position);
                dpManager.SetLiftHighlight(distance < dpManager.SnapDistanceSqr * 2f);

                if (hit.transform.TryGetComponent(out PlaneDrag planeDrag))
                {
                    planeDrag.HighlightSlot(true);
                }
            }
            else
            {
                dpManager.HideLine();
            }

            if (!forceDragEnd && Input.GetKeyUp(KeyCode.Mouse1))
            {
                EndDrag(false);
                forceDragEnd = false;
            }
        }
        else
        {
            var hudMan = HudManager.Instance;
            bool show = !disabled && !selectedDisabled && isMouseOver && !hudMan.IsSettingsOpened && hudMan.AcceptInput && correctCameraView;
            newGameObject.SetActive(show);
            HighlightSlot(show);
            isMouseOver = false;
        }
    }

    private void OnMouseOver()
    {
        var hudManager = HudManager.Instance;
        if (!selectedDisabled && (!inOrderQueue || order.OrderType != EOrderType.SendToHangar) && 
            !isDragging && 
            Input.GetKeyDown(KeyCode.Mouse1) && 
            SquadronOnDeck && 
            !hudManager.IsSettingsOpened &&
            hudManager.AcceptInput &&
            correctCameraView && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() &&
            !GameStateManager.Instance.Tutorial)
        {
            isDragging = true;
            forceDragEnd = false;
            screenPoint = Camera.main.WorldToScreenPoint(endPoint);
            //offset = endPoint - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            HighlightSquadron(true);
        }
        isMouseOver = true;
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        squadronBtn.SetEnable(enable);
        SetSelectedEnable(!selectedDisabled);
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        if (disabled || selectedDisabled)
        {
            ForceEndDrag();
        }
    }

    public void CopyHollow(List<Transform> hollows)
    {
        if (hollows == null || hollows.Count == 0 || hollows.Count > 3)
        {
            return;
        }
        Hollows.Clear();
        for(int i = 0; i < hollows.Count; i++)
        {
            SetHollow(hollowSpots[i], hollows[i]);
            Hollows.Add(hollows[i]);
        }
        hollows.Clear();
    }

    public void DestroyHollow()
    {
        foreach (var plane in Hollows)
        {
            Destroy(plane.gameObject);
        }
        Hollows.Clear();
    }

    public void UpdateSpot(bool active)
    {
        col.enabled = active && SquadronOnDeck;
        if (active && !SquadronOnDeck)
        {
            int count = deck.SquadronsCount();
            if (ID == count)
            {
                if (deck.HasDamage || (ID < deck.MaxSlotsLimit && ID == deck.MaxSlots))
                {
                    squadronBtn.gameObject.SetActive(true);
                    squadronBtn.SetActive(false);
                    return;
                }
                else if (ID < deck.MaxSlots)
                {
                    squadronBtn.gameObject.SetActive(true);
                    squadronBtn.SetActive(true);
                    return;
                }
            }
        }
        squadronBtn.SetActive(true);
        squadronBtn.gameObject.SetActive(false);
    }

    public void HighlightSquadron(bool isEnable)
    {
        if (SquadronOnDeck)
        {
            foreach (var plane in deck.DeckSquadrons[ID].Planes)
            {
                plane.MaterialChanger.Highlight(isEnable);
            }        
        }
    }
    
    public void HighlightSlot(bool isEnable)
    {
        slotHighlight.SetActive(isEnable);
        if (SquadronOnDeck)
        {
            var hudMan = HudManager.Instance;
            switch (deck.DeckSquadrons[ID].PlaneType)
            {
                case EPlaneType.Bomber:
                    icon.sprite = hudMan.BomberIcon;
                    return;
                case EPlaneType.Fighter:
                    icon.sprite = hudMan.FighterIcon;
                    return;
                case EPlaneType.TorpedoBomber:
                    icon.sprite = hudMan.TorpedoIcon;
                    return;
            }
        }
    }

    public void ForceEndDrag()
    {
        if (isDragging)
        {
            forceDragEnd = true;
            EndDrag(true);
        }
    }

    public void CreateHollows()
    {
        var fighterSquadron = PlaneMovementManager.Instance.CreateHollow(EPlaneType.Fighter);
        for (int i = 0; i < fighterSquadron.Hollows.Count; i++)
        {
            SetHollow(hollowSpots[i], fighterSquadron.Hollows[i]);
            fighterHollows.Add(fighterSquadron.Hollows[i].GetComponent<OutlineChar>());
        }
        var bomberSquadron = PlaneMovementManager.Instance.CreateHollow(EPlaneType.Bomber);
        for (int i = 0; i < bomberSquadron.Hollows.Count; i++)
        {
            SetHollow(hollowSpots[i], bomberSquadron.Hollows[i]);
            bomberHollows.Add(bomberSquadron.Hollows[i].GetComponent<OutlineChar>());
        }
        var torpedoSquadron = PlaneMovementManager.Instance.CreateHollow(EPlaneType.TorpedoBomber);
        for (int i = 0; i < torpedoSquadron.Hollows.Count; i++)
        {
            SetHollow(hollowSpots[i], torpedoSquadron.Hollows[i]);
            torpedoHollows.Add(torpedoSquadron.Hollows[i].GetComponent<OutlineChar>());
        }
        hollowsDict.Add(EPlaneType.Fighter, fighterHollows);
        hollowsDict.Add(EPlaneType.Bomber, bomberHollows);
        hollowsDict.Add(EPlaneType.TorpedoBomber, torpedoHollows);
    }

    public void ShowHollows(EPlaneType type)
    {
        var hollows = hollowsDict[type];
        foreach (var hollow in hollows)
        {
            hollow.gameObject.SetActive(true);
        }
        HollowsVisible = true;

    }

    public void HideAllHollows()
    {
        HollowsVisible = false;
        foreach (var list in hollowsDict.Values)
        {
            foreach (var hollow in list)
            {
                hollow.gameObject.SetActive(false);
            }
        }
    }

    private void SetHollow(Transform parent, Transform plane)
    {
        var trans = plane.transform;
        trans.SetParent(parent);
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
    }

    private void SnapToObject(bool isForced)
    {
        isDragging = false;

        //Snap to Squadron Lift
        float distance = Vector3.SqrMagnitude(endPoint - dpManager.PlaneElevatorPoint.position);
        if (!isForced && distance < dpManager.SnapDistanceSqr * 2f)
        {
            if (ID >= deck.DeckSquadrons.Count)
            {
                EventManager.Instance.OrderCannotBeDonePopup();
                return;
            }
            order = new SendToHangarOrder(deck, deck.DeckSquadrons[ID]);
            deck.AddOrder(order);
        }
        if (!isForced)
        {
            //Snap to reorder squadrons on deck
            Assert.IsTrue(deck.DeckMode == EDeckMode.Starting || deck.DeckMode == EDeckMode.Landing);
            CheckSwapPlanes(deck.DeckMode == EDeckMode.Starting ? dpManager.PlanesLaunchingSpots : dpManager.PlanesRecoverySpots);
        }
        endPoint = startPoint;
    }

    private void CheckSwapPlanes(List<PlaneDrag> spots)
    {
        var plusSlot = false;

        foreach (var spot in spots)
        {
            if (spot.SquadronOnDeck)
            {
                var distance = Vector3.SqrMagnitude(endPoint - spot.ChildTransform.position);
                if (distance < dpManager.SnapDistanceSqr && distance != 0 && deck.DeckSquadrons[ID] != deck.DeckSquadrons[spot.ID])
                {
                    if (ID != spot.ID)
                    {
                        order = new SwapOrder(deck, deck.DeckSquadrons[ID], spot.ID);
                        deck.RemoveDuplicateSwapOrders(deck.DeckSquadrons[ID]);
                        deck.AddOrder(order);
                    }
                    return;
                }
            }
            else if (!plusSlot)
            {
                var distance = Vector3.SqrMagnitude(endPoint - spot.ChildTransform.position);
                if (distance < dpManager.SnapDistanceSqr && distance != 0 && ID < spot.ID - 1)
                {
                    order = new SwapToFrontOrder(deck, deck.DeckSquadrons[ID]);
                    deck.RemoveDuplicateSwapOrders(deck.DeckSquadrons[ID]);
                    deck.AddOrder(order);
                    plusSlot = true;
                }
            }
        }
    }

    private void EndDrag(bool isForced)
    {
        isDragging = false;
        SnapToObject(isForced);
        HighlightSquadron(false);
        dpManager.HideLine();
        dpManager.LiftIcon.Hide();
        dpManager.SetLiftHighlight(false);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
    }
}
