using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public StrikeGroupSelectionWindow StrikeGroupSelectionWindow => strikeGroupSelectionWindow;
    public Button ConfirmOrderButton => confirmOrderButton;
    public List<OrderPanelCall> SelectedBuffButtons => selectedBuffButtons;
    public RectTransform IslandBuffsParent => islandBuffsParent;
    public IslandBuffUIElement CurrentBuffUi => currentBuffUi;
    public GameObject BuffsPanel => buffsPanel;
    public RectTransform SurvivorsRange => survivorsRange;
    public RectTransform CannonRange => cannonRange;
    public RectTransform CannonRange2 => cannonRange2;
    public GameObject MagicSprite => magicSprite;
    public GameObject MagicSprite1 => magicSprite1;
    public GameObject MagicSprite2 => magicSprite2;
    public GameObject MagicSprite3 => magicSprite3;
    public RepairSpotPopup RepairSpotPopup => repairSpotPopup;
    public MissionInstancePopup MissionInstancePopup => missionInstancePopup;
    public SandboxQuestPopup SandboxQuestPopup => sandboxQuestPopup;
    public PearlHarbourPopup PearlHarbourPopup => pearlHarbourPopup;
    public EnemyInstancePopup EnemyInstancePopup => enemyInstancePopup;
    public AbortMissionPopup AbortMissionPopup => abortMissionPopup;
    public MainGoalPanel MainGoalPanel => mainGoalPanel;
    public RectTransform SandboxPoiParent => sandboxPoiParent;
    public BuffsPanel SandboxBuffsPanel => sandboxBuffsPanel;
    public SandboxEventPopup SandboxEventPopup => eventPopup;
    public SandboxEventResultPopup SandboxEventResultPopup => eventResultPopup;
    public BuffsWindow BuffsListTip => buffsListTip;
    public TimePanel WorldMapTimePanel => worldMapTimePanel;
    public PoiInfoPanel PoiInfoPanel => poiInfoPanel;
    public GraphicRaycaster GraphicRaycaster => graphicRaycaster;

    public EUICategory EnabledCategories
    {
        get;
        private set;
    } = (EUICategory)(-1);

    [SerializeField]
    private GraphicRaycaster graphicRaycaster = null;
    [SerializeField]
    private List<UICategoryData> datas = null;
    [SerializeField]
    private StrikeGroupSelectionWindow strikeGroupSelectionWindow = null;
    [SerializeField]
    private Button confirmOrderButton = null;

    [SerializeField]
    private List<OrderPanelCall> selectedBuffButtons = null;
    [SerializeField]
    private RectTransform islandBuffsParent = null;
    [SerializeField]
    private IslandBuffUIElement currentBuffUi = null;
    [SerializeField]
    private GameObject buffsPanel = null;
    [SerializeField]
    private RectTransform survivorsRange = null;
    [SerializeField]
    private RectTransform cannonRange = null;
    [SerializeField]
    private RectTransform cannonRange2 = null;
    [SerializeField]
    private GameObject magicSprite = null;
    [SerializeField]
    private GameObject magicSprite1 = null;
    [SerializeField]
    private GameObject magicSprite2 = null;
    [SerializeField]
    private GameObject magicSprite3 = null;
    [SerializeField]
    private List<GameObject> showableGameObjects = null;
    [SerializeField]
    private BuffsWindow buffsListTip = null;
    [SerializeField]
    private Image shipSpeedImage = null;
    [SerializeField]
    private List<Sprite> shipSpeedSprites = null;

    [Header("Sandbox")]
    [SerializeField]
    private PoiInfoPanel poiInfoPanel = null;
    [SerializeField]
    private RepairSpotPopup repairSpotPopup = null;
    [SerializeField]
    private MissionInstancePopup missionInstancePopup = null;
    [SerializeField]
    private SandboxQuestPopup sandboxQuestPopup = null;
    [SerializeField]
    private EnemyInstancePopup enemyInstancePopup = null;
    [SerializeField]
    private RectTransform sandboxPoiParent = null;
    [SerializeField]
    private PearlHarbourPopup pearlHarbourPopup = null;
    [SerializeField]
    private AbortMissionPopup abortMissionPopup = null;
    [SerializeField]
    private MainGoalPanel mainGoalPanel = null;
    [SerializeField]
    private BuffsPanel sandboxBuffsPanel = null;
    [SerializeField]
    private SandboxEventPopup eventPopup = null;
    [SerializeField]
    private SandboxEventResultPopup eventResultPopup = null;
    [SerializeField]
    private TimePanel worldMapTimePanel = null;
    [SerializeField]
    private List<AllyHudMarker> allies = null;
    [SerializeField]
    private List<AllyHudMarker> allies2 = null;

    [SerializeField]
    private List<GameObject> toHideOnWorldMap = null;

    private Dictionary<EUICategory, UICategoryData> dict;
    private List<int> list;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        dict = new Dictionary<EUICategory, UICategoryData>();
        foreach (var data in datas)
        {
#if UNITY_EDITOR
            int cat = (int)data.Category;
            Assert.IsTrue((cat != 0 && (cat & (cat - 1)) == 0), data.Category + " is not valid category");
#endif
            data.EnableablesInterface = new List<IEnableable>();
            foreach (var enableable in data.Enableables)
            {
                if (enableable is IEnableable inter)
                {
                    data.EnableablesInterface.Add(inter);
                }
                else
                {
                    Assert.IsTrue(false, enableable + " is not IEnableable");
                }
            }
            dict[data.Category] = data;
        }
        list = new List<int>();
    }

    private void Start()
    {
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            WorldMap.Instance.Toggled += OnWorldMapToggled;
        }
    }

    public void Load(List<int> list)
    {
        if (list == null)
        {
            list = new List<int>();
        }
        ShowAllies(list.Count > 0, list);
    }

    public List<int> SaveData()
    {
        return list;
    }

    public void LoadShowable(ref VisualsSaveData data)
    {
        for (int i = 0; i < showableGameObjects.Count; i++)
        {
            SetShow(i, false);
        }
        if (data.Showables == null)
        {
            return;
        }
        foreach (int index in data.Showables)
        {
            SetShow(index, true);
        }
    }

    public void SaveShowable(ref VisualsSaveData data)
    {
        data.Showables = new List<int>();
        for (int i = 0; i < showableGameObjects.Count; i++)
        {
            if (showableGameObjects[i].activeSelf)
            {
                data.Showables.Add(i);
            }
        }
    }

    public void SetEnabledCategory(EUICategory category)
    {
        EnabledCategories = category;
        for (int i = 0; ; i++)
        {
            var cat = (EUICategory)(1 << i);
            if (dict.TryGetValue(cat, out var data))
            {
                bool enable = (category & cat) != 0;
                foreach (var button in data.Buttons)
                {
                    button.enabled = enable;
                }
                foreach (var enableable in data.EnableablesInterface)
                {
                    enableable.SetEnable(enable);
                }
            }
            else
            {
                Assert.IsTrue(i == dict.Count);
                break;
            }
        }
    }

    public void SetShow(int index, bool show)
    {
        showableGameObjects[index].SetActive(show);
    }

    public void ShowAllies(bool show, List<int> allies)
    {
        int i;
        var tacMan = TacticManager.Instance;
        if (!show)
        {
            allies.Clear();
        }
        list.Clear();
        list.AddRange(allies);
        for (i = 0; i < allies.Count; i++)
        {
            this.allies[i].Setup(tacMan.GetShip(allies[i]));
            this.allies2[i].Setup(tacMan.GetShip(allies[i]));
        }
        for (; i < this.allies.Count; i++)
        {
            this.allies[i].Hide();
            this.allies2[i].Hide();
        }
    }

    public void SwapCannons()
    {
        (cannonRange, cannonRange2) = (cannonRange2, cannonRange);
    }

    public void SetShipSpeedImage(int speedIndex)
    {
        shipSpeedImage.sprite = shipSpeedSprites[speedIndex];
    }

    private void OnWorldMapToggled(bool show)
    {
        foreach (var obj in toHideOnWorldMap)
        {
            obj.SetActive(!show);
        }
    }
}
