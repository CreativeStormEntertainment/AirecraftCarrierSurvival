using GambitUtils;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class AircraftPanel : Panel
{
    public int MaxHangarCount
    {
        get;
        set;
    }

    [SerializeField]
    private List<AircraftInitData> initDatas = null;

    [SerializeField]
    private AircraftShopInventory shopInventory = null;

    [SerializeField]
    private StudioEventEmitter hoverPlanesTypeEvent = null;

    [SerializeField]
    private Button buyTab = null;
    [SerializeField]
    private Text buyText = null;
    [SerializeField]
    private Button upgradeTab = null;
    [SerializeField]
    private Text upgradeText = null;

    [SerializeField]
    private Sprite activeTab = null;
    [SerializeField]
    private Sprite inactiveTab = null;

    [SerializeField]
    private Text allHangarSquadrons = null;

    [SerializeField]
    private Text bomberSquadrons = null;
    [SerializeField]
    private Text fighterSquadrons = null;
    [SerializeField]
    private Text torpedoSquadrons = null;

    [SerializeField]
    private List<AircraftClickableSlots> aircraftClickableSlots = null;

    [SerializeField]
    private AircraftPopup popup = null;

    [SerializeField]
    private List<HangarCapacityData> hangarDatas = null;

    [SerializeField]
    private int minSquadronsCount = 3;

    [SerializeField]
    private CarrierPanel carrierPanel = null;

    [SerializeField]
    private List<AircraftIntermission> fighters = null;
    [SerializeField]
    private List<AircraftIntermission> bombers = null;
    [SerializeField]
    private List<AircraftIntermission> torpedoes = null;

    [Header ("Sandbox")]
    [SerializeField]
    private GameObject sandboxPlanesPopup = null;

    private List<AircraftIntermissionData> datas;
    private Dictionary<EPlaneType, AircraftInitData> initDatasDict;
    private Dictionary<ECarrierType, HangarCapacityData> hangarDict;

    private int bomberUpgrades;
    private int fighterUpgrades;
    private int torpedoUpgrades;

    private List<Text> squadronsTexts;
    private bool nextFrame;
    private int value;

    private Color buyColor;
    private Color upgradeColor;
    private Image buyImage;
    private Image upgradeImage;

    public override void Setup(NewSaveData data)
    {
        squadronsTexts = new List<Text>();
        datas = new List<AircraftIntermissionData>();
        base.Setup(data);

        hangarDict = new Dictionary<ECarrierType, HangarCapacityData>();
        foreach (var hangarData in hangarDatas)
        {
            hangarDict.Add(hangarData.Type, hangarData);
        }

        MaxHangarCount = hangarDict[currentCarrier].Capacity[0];

        initDatasDict = new Dictionary<EPlaneType, AircraftInitData>();
        foreach (var initData in initDatas)
        {
            initDatasDict.Add(initData.Type, initData);
        }

        squadronsTexts = new List<Text>();
        AddData(EPlaneType.Bomber, ref data.IntermissionData, ref bomberUpgrades, bomberSquadrons, bombers);
        AddData(EPlaneType.Fighter, ref data.IntermissionData, ref fighterUpgrades, fighterSquadrons, fighters);
        AddData(EPlaneType.TorpedoBomber, ref data.IntermissionData, ref torpedoUpgrades, torpedoSquadrons, torpedoes);

        for (int i = 0; i < aircraftClickableSlots.Count; i++)
        {
            aircraftClickableSlots[i].Init(datas[i]);
            aircraftClickableSlots[i].Highlight += SetShowHighlights;
        }

        carrierPanel.HangarUpgraded += OnHangarUpgraded;

        shopInventory.Setup(datas, 3, OnUpgrade);
        shopInventory.SetupFinished();
        buyTab.onClick.AddListener(() => SetTab(true));
        upgradeTab.onClick.AddListener(() => SetTab(false));

        buyColor = buyText.color;
        upgradeColor = upgradeText.color;
        buyImage = buyTab.GetComponent<Image>();
        upgradeImage = upgradeTab.GetComponent<Image>();

        SetTab(true);
        //allPlanes.RightClicked += () =>
        //{
        //    OnRightClicked(EPlaneType.Fighter);
        //    OnRightClicked(EPlaneType.Bomber);
        //    OnRightClicked(EPlaneType.TorpedoBomber);
        //};

        shopInventory.Highlight += SetShowHighlights;

        popup.CurrentAircraftChanged += OnCurrentAircraftChanged;

        RefreshCounts();
    }

    public override void SetShow(bool show)
    {
        base.SetShow(show);
        if (!show)
        {
            popup.Hide();
        }
    }

    public override void Save(NewSaveData data)
    {
        var bomberData = datas[0];
        var fighterData = datas[1];
        var torpedoData = datas[2];

        ref var intermissionData = ref data.IntermissionData;
        intermissionData.SetCurrent(bomberData.CurrentCount, fighterData.CurrentCount, torpedoData.CurrentCount);
        intermissionData.SetSkinAndUpgrades(bomberUpgrades, fighterUpgrades, torpedoUpgrades, bomberData.ChosenSkin, fighterData.ChosenSkin, torpedoData.ChosenSkin);
    }

    public void SetShowHighlights(EPlaneType planeType, bool highlight)
    {
        aircraftClickableSlots[(int)planeType].SetHighlight(highlight);
        hoverPlanesTypeEvent.Play();
    }

    protected override void InnerRefresh(int prevCarrier)
    {
        base.InnerRefresh(prevCarrier);
        shopInventory.Refresh();
    }

    private void SetTab(bool buy)
    {
        buyTab.interactable = !buy;
        upgradeTab.interactable = buy;

        buyImage.sprite = buy ? activeTab : inactiveTab;
        upgradeImage.sprite = buy ? inactiveTab : activeTab;

        buyColor.a = buy ? 1f : 0.5f;
        buyText.color = buyColor;
        upgradeColor.a = buy ? 0.5f : 1f;
        upgradeText.color = upgradeColor;

        buyText.rectTransform.anchoredPosition = new Vector2(0f, buy ? 0f : -5f);
        upgradeText.rectTransform.anchoredPosition = new Vector2(0f, buy ? -5f : 0f);

        shopInventory.SetBuy(buy);
    }

    private void OnUpgrade(int index, bool bought)
    {
        if (bought)
        {
            OnCurrentAircraftChanged((EPlaneType)index, datas[index].Count);
        }
        else
        {
            switch (index)
            {
                case 0:
                    OnUpgrade(ref bomberUpgrades, EPlaneType.Bomber);
                    break;
                case 1:
                    OnUpgrade(ref fighterUpgrades, EPlaneType.Fighter);
                    break;
                case 2:
                    OnUpgrade(ref torpedoUpgrades, EPlaneType.TorpedoBomber);
                    break;
                default:
                    Assert.IsTrue(false);
                    break;
            }
        }
        //inventory.Refresh();
        shopInventory.SetBuy(upgradeTab.interactable);
    }

    private int RefreshCounts()
    {
        int all = 0;
        int index = 0;
        foreach (var data in datas)
        {
            squadronsTexts[index].text = data.CurrentCount.ToString();
            all += data.CurrentCount;
            index++;
        }
        allHangarSquadrons.text = $"{all}/{MaxHangarCount}";
        var intermissionMan = IntermissionManager.Instance;
        intermissionMan.SetActiveFightersWarning(datas[1].CurrentCount < minSquadronsCount);
        intermissionMan.SetActiveBombersWarning(datas[0].CurrentCount < minSquadronsCount);
        intermissionMan.SetActiveTorpedoWarning(datas[2].CurrentCount < minSquadronsCount);
        shopInventory.AllowBuy(all < MaxHangarCount);
        return all;
    }

    private void AddData(EPlaneType type, ref IntermissionData saveData, ref int upgradeCount, Text text, List<AircraftIntermission> squadrons)
    {
        var initData = initDatasDict[type];
        upgradeCount = saveData.GetUpgrade(type);
        var upgrade = initData.UpgradeDatas[upgradeCount];
        int currentCount = saveData.GetCurrent(type);
        if (currentCount < minSquadronsCount)
        {
            currentCount = minSquadronsCount;
            sandboxPlanesPopup.SetActive(true);
        }
        var data = new AircraftIntermissionData(type, currentCount, initData.BuyCosts, initData.BuyTextID, saveData.GetSkin(type), upgrade, upgradeCount, squadrons);
        datas.Add(data);
        foreach (var obj in upgrade.ShowCurrent)
        {
            obj.SetActive(true);
        }
        foreach (var squadron in data.Squadrons)
        {
            squadron.ShowTier(data.CurrentTier);
        }

        text.text = data.CurrentCount.ToString();
        squadronsTexts.Add(text);
    }

    private void OnUpgrade(ref int upgrade, EPlaneType type)
    {
        var upgrades = initDatasDict[type].UpgradeDatas;
        foreach (var obj in upgrades[upgrade].ShowCurrent)
        {
            obj.SetActive(false);
        }
        foreach (var obj in upgrades[upgrade].ShowOnHighlight)
        {
            obj.SetActive(false);
        }
        upgrade++;
        datas[(int)type].UpgradeData = upgrades[upgrade];
        var data = datas.Find(item => item.Type == type);
        data.CurrentTier = upgrade;
        foreach (var obj in upgrades[upgrade].ShowCurrent)
        {
            obj.SetActive(true);
        }
        foreach (var squadron in data.Squadrons)
        {
            squadron.ShowTier(upgrade);
        }
    }

    private void OnRightClicked(EPlaneType type)
    {
        datas[(int)type].ClearCurrent();
        squadronsTexts[(int)type].text = "0";
        //inventory.Refresh();
        RefreshCounts();
    }

    private void OnCurrentAircraftChanged(EPlaneType type, int value)
    {
        int index = (int)type;
        var data = datas[index];
        data.AddCurrent(value);
        squadronsTexts[index].text = data.CurrentCount.ToString();

        //inventory.Refresh();
        RefreshCounts();
    }

    private void OnHangarUpgraded(int count)
    {
        value = count;

        if (!nextFrame)
        {
            nextFrame = true;
            var intermMan = IntermissionManager.Instance;
            intermMan.StartCoroutineActionAfterFrames(() =>
                {
                    nextFrame = false;
                    MaxHangarCount = hangarDict[currentCarrier].Capacity[this.value];
                    int value = RefreshCounts();
                    if (value > MaxHangarCount)
                    {
                        int index = 0;
                        count = 0;
                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (count < datas[i].CurrentCount)
                            {
                                index = i;
                                count = datas[i].CurrentCount;
                            }
                        }

                        int c = 0;
                        int commandPoints = 0;
                        for (int i = MaxHangarCount; i < value;)
                        {
                            if (datas[index].CurrentCount > minSquadronsCount)
                            {
                                datas[index].AddCurrent(-1);
                                commandPoints++;
                                i++;
                                c = 0;
                            }
                            else
                            {
                                i--;
                                if (++c == 3)
                                {
                                    foreach (var data in datas)
                                    {
                                        commandPoints += data.CurrentCount;
                                        data.ClearCurrent();
                                    }
                                    break;
                                }
                            }
                            index = (index + 1) % datas.Count;
                        }
                        if (commandPoints > 0)
                        {
                            intermMan.StartCoroutineActionAfterFrames(() => intermMan.AddCommandPoints(commandPoints), 1);
                        }
                    }
                }, 1);
        }
    }

    private int PlaneToIndex(EPlaneType type)
    {
        return type == EPlaneType.TorpedoBomber ? (int)type : Mathf.Abs((int)type - 1);
    }
}
