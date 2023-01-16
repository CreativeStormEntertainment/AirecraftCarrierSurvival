using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckOrderPanelManager : MonoBehaviour, IEnableable
{
    public event Action EnableChanged = delegate { };

    public static DeckOrderPanelManager Instance;

    private static readonly int ShowBool = Animator.StringToHash("Show");

    public Transform Content = null;

    public bool IsDragging = false;

    public bool Disable
    {
        get;
        private set;
    }

    [HideInInspector]
    public DeckOrder currentDrag = null;

    [SerializeField]
    private Canvas canvas = null;

    [SerializeField]
    private GameObject orderPrefab = null;

    [SerializeField]
    private GameObject recoverySelected = null;

    [SerializeField]
    private GameObject launchingSelected = null;

    [SerializeField]
    private Animator animator = null;

    [SerializeField]
    private GameObject currentOrderHighlight = null;

    [SerializeField]
    private Text currentStateText = null;

    [SerializeField]
    private Text currentOrderNameText = null;

    [SerializeField]
    private List<OrderData> orderData = null;

    [SerializeField]
    private GameObject container = null;

    [SerializeField]
    private Button recoveryBtn = null;

    [SerializeField]
    private Button launchingBtn = null;

    [SerializeField]
    private PulseAnim pulseAnim = null;

    private Dictionary<EOrderType, OrderData> orderDict;
    private string noOrder = "noOrder";

    private string launchingText;
    private string recoveryText;
    private string blockText;

    private HashSet<ItemSlot> slots;

    private List<StateTooltip> stateTooltips;

    private void Awake()
    {
        Instance = this;
        container.SetActive(false);
        var locMan = LocalizationManager.Instance;
        orderDict = new Dictionary<EOrderType, OrderData>();
        foreach (var data in orderData)
        {
            data.Title = locMan.GetText(data.Title);
            orderDict[data.OrderType] = data;
        }
        //{
        //    { EOrderType.Mission, locMan.GetText("OrderMission") },
        //    { EOrderType.Landing, locMan.GetText("OrderLanding") },
        //    { EOrderType.ModeChange, locMan.GetText("OrderModeChange") },
        //    { EOrderType.SquadronCreation, locMan.GetText("OrderSquadronCreation") },
        //    { EOrderType.SendToHangar, locMan.GetText("OrderSendToHangar") },
        //    { EOrderType.Swap, locMan.GetText("OrderSwap") },
        //};
        noOrder = locMan.GetText(noOrder);
    }

    private void Start()
    {
        var locMan = LocalizationManager.Instance;
        launchingText = locMan.GetText("Launching");
        recoveryText = locMan.GetText("Recovery");
        blockText = locMan.GetText("DeckBlockedState");

        var deck = AircraftCarrierDeckManager.Instance;
        deck.DeckModeChanged += OnDeckModeChanged;
        deck.BlockOrdersChanged += OnDeckModeChanged;
        OnDeckModeChanged();

        slots = new HashSet<ItemSlot>();
        stateTooltips = new List<StateTooltip>();
        foreach (Transform slot in Content)
        {
            slots.Add(slot.GetComponent<ItemSlot>());
            stateTooltips.Add(slot.GetComponent<StateTooltip>());
        }
        currentOrderNameText.text = noOrder;
    }

    public void SetEnable(bool enable)
    {
        Disable = !enable;
        EnableChanged();
    }

    public Sprite GetIcon(EOrderType type)
    {
        return orderDict[type].OrderSprite;
    }

    public void ToggleShow()
    {
        animator.SetBool(ShowBool, !animator.GetBool(ShowBool));
    }

    public void UpdateOrders()
    {
        if (!IsDragging)
        {
            var deck = AircraftCarrierDeckManager.Instance;
            ClearOrders();

            launchingBtn.interactable = deck.DeckMode == EDeckMode.Landing;
            recoveryBtn.interactable = deck.DeckMode == EDeckMode.Starting;

            for (int i = 0; i < deck.OrderQueue.Count; i++)
            {
                var parentSlot = Content.GetChild(i);
                var orderObj = Instantiate(orderPrefab, parentSlot, false);
                orderObj.GetComponent<DeckOrder>().Setup(i, deck.OrderQueue[i], parentSlot.GetComponent<ItemSlot>(), canvas);
                if (deck.OrderQueue[i].OrderType == EOrderType.ToLaunching || deck.OrderQueue[i].OrderType == EOrderType.ToRecovering)
                {
                    launchingBtn.interactable = false;
                    recoveryBtn.interactable = false;
                }
            }
            bool isNotEmpty = deck.OrderQueue.Count != 0;
            currentOrderHighlight.SetActive(isNotEmpty);
            if (isNotEmpty)
            {
                currentOrderNameText.text = orderDict[deck.OrderQueue[0].OrderType].Title;
            }
            else
            {
                currentOrderNameText.text = noOrder;
            }
        }
        DragPlanesManager.Instance.UpdateMe();
        TooltipUpdate();
    }

    public void OnRecoveryBtn()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        deck.AddOrder(new ChangeToRecoveringOrder(deck, false));
    }

    public void OnLaunchingBtn()
    {
        var deck = AircraftCarrierDeckManager.Instance;
        deck.AddOrder(new ChangeToLaunchingOrder(deck, false));
    }

    public void SwitchHighlights(bool value)
    {
        foreach (ItemSlot slot in slots)
        {
            slot.Highlight(value);
        }
    }

    private void ClearOrders()
    {
        foreach (Transform orderSlot in Content)
        {
            if (orderSlot.childCount >= 2)
            {
                foreach (Transform order in orderSlot)
                {
                    if (!order.name.Equals("Highlight"))
                    {
                        Destroy(order.gameObject);
                    }
                }
            }
        }
    }

    private void OnDeckModeChanged()
    {
        var deck = AircraftCarrierDeckManager.Instance;

        pulseAnim.SetShow(deck.BlockOrders);
        if (deck.BlockOrders)
        {
            currentStateText.text = blockText;
        }
        else
        {
            switch (deck.DeckMode)
            {
                case EDeckMode.Landing:
                    currentStateText.text = recoveryText;
                    break;
                case EDeckMode.Starting:
                    currentStateText.text = launchingText;
                    break;
                default:
                    Debug.LogError("Deck Mode ERROR!");
                    break;
            }
        }
        bool launching = deck.DeckMode == EDeckMode.Starting;
        recoverySelected.SetActive(!launching);
        launchingSelected.SetActive(launching);
    }

    private void TooltipUpdate()
    {
        for (int i = 0; i < stateTooltips.Count; i++)
        {
            GetQueuedOrders(i, out EOrdersTooptip tooltip, out string param);
            stateTooltips[i].ChangeStateTitleParams(tooltip, param);
        }
    }

    private void GetQueuedOrders(int orderIndex, out EOrdersTooptip tooltip, out string param)
    {
        var deck = AircraftCarrierDeckManager.Instance;
        param = "";
        if (orderIndex >= deck.OrderQueue.Count)
        {
            tooltip = EOrdersTooptip.NoOperations;
            return;
        }

        var order = deck.OrderQueue[orderIndex];

        if (order.OrderType == EOrderType.ToLaunching || order.OrderType == EOrderType.ToRecovering)
        {
            tooltip = EOrdersTooptip.DeckChange;
        }
        else if (order.OrderType == EOrderType.SquadronCreation && ((SquadronCreationOrder)order).PlaneType == EPlaneType.Fighter)
        {
            tooltip = EOrdersTooptip.FightersOnDeck;
        }
        else if (order.OrderType == EOrderType.SquadronCreation && ((SquadronCreationOrder)order).PlaneType == EPlaneType.Bomber)
        {
            tooltip = EOrdersTooptip.BombersOnDeck;
        }
        else if (order.OrderType == EOrderType.SquadronCreation && ((SquadronCreationOrder)order).PlaneType == EPlaneType.TorpedoBomber)
        {
            tooltip = EOrdersTooptip.TorpedoOnDeck;
        }
        else if (order.OrderType == EOrderType.SendToHangar)
        {
            tooltip = EOrdersTooptip.MoveToHangar;
        }
        else if (order.OrderType == EOrderType.Swap)
        {
            tooltip = EOrdersTooptip.SquadronSwap;
        }
        else if (order.OrderType == EOrderType.Mission)
        {
            tooltip = EOrdersTooptip.MissionLaunching;
            param = TacticManager.Instance.MissionInfo[order.Mission.OrderType].MissionName;
        }
        else if (order.OrderType == EOrderType.Landing)
        {
            tooltip = EOrdersTooptip.MissionRecovering;
            param = TacticManager.Instance.MissionInfo[order.Mission.OrderType].MissionName;
        }
        else if(order.OrderType == EOrderType.ForcedDecreaseSquadrons)
        {
            tooltip = EOrdersTooptip.ForcedDecreaseSquadrons;
        }
        else
        {
            tooltip = EOrdersTooptip.NoOperations;
        }
    }
}
