using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategySelectionPanel : ParameterEventBase<ECardAnimation>, IPopupPanel
{
    public event Action ForcedStrategySet = delegate { };
    public event Action<EManeuverType> CategorySwitched = delegate { }; 

    public EWindowType Type => EWindowType.StrategySelection;

    public bool Drag
    {
        get;
        set;
    }

    public RectTransform Container => container.GetComponent<RectTransform>();

    public Dictionary<PlayerManeuverData, int> ForcedStrategy
    {
        get;
        private set;
    }

    [SerializeField]
    private Canvas canvas = null;

    [SerializeField]
    private EnemyStrategyPanel enemyStrategyPanel = null;

    [SerializeField]
    private List<GameObject> cardPrefab = null;
    [SerializeField]
    private List<Transform> cardsPanels = null;
    [SerializeField]
    private List<CardPlace> cardPlaces = null;

    [SerializeField]
    private Text playerAttackText = null;
    [SerializeField]
    private Text playerDefenceText = null;
    [SerializeField]
    private Text enemyAttackText = null;
    [SerializeField]
    private Text enemyDefenceText = null;
    [SerializeField]
    private Text playerResultText = null;
    [SerializeField]
    private Text enemyResultText = null;
    [SerializeField]
    private Text fightersAmountTexts = null;
    [SerializeField]
    private Text bombersAmountTexts = null;
    [SerializeField]
    private Text torpedoAmountTexts = null;

    [SerializeField]
    private Button confirmButton = null;

    [SerializeField]
    private Button offensiveCardsButton = null;
    [SerializeField]
    private Button defensiveCardsButton = null;
    [SerializeField]
    private Button supplyCardsButton = null;

    [SerializeField]
    private Button closeButton = null;

    [SerializeField]
    private RectTransform cardsBackground = null;

    [SerializeField]
    private GameObject container = null;

    [SerializeField]
    private GameObject StrategyTooExpensive = null;

    [SerializeField]
    private int maxSquadrons = 6;

    private List<CardHandler> cards = null;

    private int bombers;
    private int fighters;
    private int torpedoes;

    private List<PlayerManeuverData> playerManeuverDatas = new List<PlayerManeuverData>();
    private Dictionary<int, int> blockedModifiersPlaces = new Dictionary<int, int>();
    private Dictionary<int, int> debuffModifiersPlaces = new Dictionary<int, int>();
    private CardHandler midwayCustomCard;
    private CardHandler magicCustomCard;

    private int lastTime;

    private TacticalEnemyShip enemyShip;
    private EMissionOrderType missionType;

    private Dictionary<PlayerManeuverData, CardHandler> cardsDict;

    private int blocks;

    public void Setup()
    {
        WorldMap.Instance.Toggled += OnToggled;

        cards = new List<CardHandler>();

        closeButton.onClick.AddListener(Hide);

        confirmButton.onClick.AddListener(OnConfirmButtonClick);

        offensiveCardsButton.onClick.AddListener(() => OnCardsButtonClick(offensiveCardsButton, EManeuverType.Aggressive));
        defensiveCardsButton.onClick.AddListener(() => OnCardsButtonClick(defensiveCardsButton, EManeuverType.Defensive));
        supplyCardsButton.onClick.AddListener(() => OnCardsButtonClick(supplyCardsButton, EManeuverType.Supplementary));

        cardsDict = new Dictionary<PlayerManeuverData, CardHandler>();

        var tacMan = TacticManager.Instance;
        for (int i = 0; i < 3; i++)
        {
            var maneuvers = tacMan.PlayerManevuersDict[(EManeuverType)i];
            for (int j = 0; j < maneuvers.Count; j++)
            {
                CardHandler card = Instantiate(cardPrefab[i], cardsPanels[i].GetChild(j)).GetComponent<CardHandler>();
                card.Setup(maneuvers[j], canvas, this);
                cards.Add(card);
                cardsDict.Add(maneuvers[j], card);
            }
            cardsPanels[0].gameObject.SetActive(false);
        }
        var maneuver = tacMan.MidwayCustomManeuver;
        midwayCustomCard = Instantiate(cardPrefab[(int)maneuver.ManeuverType], Container).GetComponent<CardHandler>();
        midwayCustomCard.Setup(maneuver, canvas, this);
        midwayCustomCard.Irreplaceable = true;

        maneuver = tacMan.MagicCustomManeuver;
        magicCustomCard = Instantiate(cardPrefab[(int)maneuver.ManeuverType], Container).GetComponent<CardHandler>();
        magicCustomCard.Setup(maneuver, canvas, this);
        magicCustomCard.Irreplaceable = true;

        foreach (var item in cardPlaces)
        {
            item.Setup(this);
        }

        SelectPanel(EManeuverType.Aggressive);
        container.SetActive(false);

        enemyStrategyPanel.Init();
    }

    public void OpenTutorial()
    {
        MovieManager.Instance.Play(ETutorialType.DesignAttack);
    }

    public void SelectPanel(EManeuverType type)
    {
        int intType = (int)type;
        for (int i = 0; i < 3; i++)
        {
            cardsPanels[i].gameObject.SetActive(i == intType);
        }
        CategorySwitched(type);
    }

    public void ShowPanel(TacticalEnemyShip ship)
    {
        var hudMan = HudManager.Instance;
        lastTime = hudMan.TimeIndex;
        hudMan.OnPausePressed();
        hudMan.UnpauseMapMusic();

        var mission = TacticManager.Instance.CurrentMission.OrderType;
        int blocks = 0;
        int count = ship.Blocks.Count;
        for (int i = 0; i < count; i++)
        {
            if (ship.Blocks[i].Dead)
            {
                blocks |= 1 << i;
            }
            if (ship.Blocks[i].Visible)
            {
                blocks |= 1 << (i + count);
            }
        }

        container.SetActive(true);
        OnCardsButtonClick(offensiveCardsButton, EManeuverType.Aggressive);

        if (enemyShip != ship || missionType != mission || this.blocks != blocks)
        {
            enemyShip = ship;
            missionType = mission;
            this.blocks = blocks;

            enemyStrategyPanel.Setup(enemyShip, this);

            ResetCards();
            SetConfirmActive(false);

            DisableCards(enemyShip, enemyStrategyPanel.EnemyShipDatas);

            RecalculateValues();

            midwayCustomCard.gameObject.SetActive(false);
            magicCustomCard.gameObject.SetActive(false);
            switch (missionType)
            {
                case EMissionOrderType.MidwayAirstrike:
                    midwayCustomCard.gameObject.SetActive(true);
                    midwayCustomCard.CardToPlace(cardPlaces[0]);
                    break;
                case EMissionOrderType.MagicAirstrike:
                    magicCustomCard.gameObject.SetActive(true);
                    magicCustomCard.CardToPlace(cardPlaces[0]);
                    break;
            }
        }

        hudMan.PopupShown(this);
    }

    public void Restart()
    {
        var maneuvers = playerManeuverDatas;

        ResetCards();
        SetConfirmActive(false);
        DisableCards(enemyShip, enemyStrategyPanel.EnemyShipDatas);
        for (int i = 0; i < maneuvers.Count; i++)
        {
            if (maneuvers[i] == null || cardPlaces[i].Blocker.activeSelf)
            {
                continue;
            }
            var card = cardsDict[maneuvers[i]];
            if (card.Blocker.activeSelf)
            {
                continue;
            }
            card.CardToPlace(cardPlaces[i]);
        }
        RecalculateValues();
    }

    public void RecalculateValues()
    {
        List<PlayerManeuverData> playerManeuverDatas = new List<PlayerManeuverData>();
        bool allSlotsEmpty = true;
        int playerManeuversCount = 0;
        foreach (var item in cardPlaces)
        {
            if (item.CurrentCard == null)
            {
                playerManeuverDatas.Add(null);
            }
            else
            {
                if (!item.CurrentCard.Blocker.activeSelf && !item.CurrentCard.ModifierBlocker.activeSelf &&
                    blockedModifiersPlaces.TryGetValue(playerManeuverDatas.Count, out int value))
                {
                    item.CurrentCard.SlotModifierBlocked = true;
                    item.CurrentCard.BlockerIndex = value;
                    item.CurrentCard.ModifierBlocker.SetActive(true);
                }

                //check if has secondary debuff
                if (debuffModifiersPlaces.TryGetValue(playerManeuverDatas.Count, out int val))
                {
                    item.CurrentCard.DebuffIndices.Add(val);
                }

                foreach (int index2 in item.CurrentCard.DebuffIndexSecondaries)
                {
                    item.CurrentCard.DebuffIndices.Add(index2);
                }

                playerManeuverDatas.Add(item.CurrentCard.Data);
                playerManeuversCount++;
                allSlotsEmpty = false;
            }

        }
        this.playerManeuverDatas = playerManeuverDatas;

        enemyStrategyPanel.GetSelectedShip(out int index, out var enemyManeuverDatas, out var durabilities);
        var durabilities2 = new List<int>(durabilities);

        var modifiers = Parameters.Instance.DifficultyParams;
        var tacMan = TacticManager.Instance;
        ManeuverCalculator.Calculate(playerManeuverDatas, enemyManeuverDatas, durabilities2, index, out FightSquadronData squadronData, out AttackParametersData parametersData,
            out List<AttackParametersData> playerList, out AttackParametersData enemyDataMin, out CasualtiesData casualtiesDataMin, ECalculateType.TestMin,
            tacMan.MinDivisor, tacMan.MaxDivisor, tacMan.Divisor, tacMan.GetBonusManeuversDefence(enemyShip), tacMan.StrategyBonusManeuversAttack, missionType == EMissionOrderType.MagicAirstrike,
            modifiers.EnemyBlocksAttackModifier, modifiers.EnemyBlocksDefenseModifier);

        bool damagedBlocks = false;
        for (int i = 0; i < durabilities.Count; i++)
        {
            if (durabilities[i] > durabilities2[i])
            {
                damagedBlocks = true;
                break;
            }
        }

        enemyStrategyPanel.GetSelectedShip(out index, out enemyManeuverDatas, out durabilities);
        durabilities2.Clear();
        durabilities2.AddRange(durabilities);
        ManeuverCalculator.Calculate(playerManeuverDatas, enemyManeuverDatas, durabilities2, index, out _, out _, out _, out AttackParametersData enemyDataMax, out CasualtiesData casualtiesDataMax,
            ECalculateType.TestMax, tacMan.MinDivisor, tacMan.MaxDivisor, tacMan.Divisor, tacMan.GetBonusManeuversDefence(enemyShip), tacMan.StrategyBonusManeuversAttack, missionType == EMissionOrderType.MagicAirstrike,
            modifiers.EnemyBlocksAttackModifier, modifiers.EnemyBlocksDefenseModifier);

        bool uncertainDamagedBlocks = false;
        for (int i = 0; i < durabilities.Count; i++)
        {
            if (durabilities[i] > durabilities2[i])
            {
                uncertainDamagedBlocks = !damagedBlocks;
                damagedBlocks = true;
                break;
            }
        }

        for (int i = 0; i < cardPlaces.Count; i++)
        {
            if (cardPlaces[i].CurrentCard != null)
            {
                cardPlaces[i].CurrentCard.NewValues(playerList[i].Attack, playerList[i].Defense);
            }
        }

        playerAttackText.text = ((int)parametersData.Attack).ToString();
        playerDefenceText.text = ((int)parametersData.Defense).ToString();
        enemyAttackText.text = enemyDataMin.Attack.ToString() + " - " + enemyDataMax.Attack.ToString();
        enemyDefenceText.text = enemyDataMin.Defense.ToString() + " - " + enemyDataMax.Defense.ToString();

        fighters = Mathf.Max(squadronData.Fighters, 0);
        bombers = Mathf.Max(squadronData.Bombers, 0);
        torpedoes = Mathf.Max(squadronData.Torpedoes, 0);
        torpedoAmountTexts.text = torpedoes.ToString();
        bombersAmountTexts.text = bombers.ToString();
        fightersAmountTexts.text = fighters.ToString();

        int squadrons = squadronData.Torpedoes + squadronData.Fighters + squadronData.Bombers;

        if (ForcedStrategy == null)
        {
            SetConfirmActive(!(allSlotsEmpty || squadrons <= 0 || squadrons > maxSquadrons));
        }
        else
        {
            bool ok = true;
            foreach (var pair in ForcedStrategy)
            {
                if (playerManeuverDatas.Count <= pair.Value || playerManeuverDatas[pair.Value] != pair.Key)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                ForcedStrategySet();
            }
            SetConfirmActive(ok);
        }
        StrategyTooExpensive.SetActive(squadrons > maxSquadrons);

        bool allEnemyBlocksVisible = enemyStrategyPanel.AllEnemyShipBlocksVisible;

        int damagedSquadronsCountMin = casualtiesDataMin.GetDamagedSquadronsCount();
        int damagedSquadronsCountMax = casualtiesDataMax.GetDamagedSquadronsCount();

        string playerResultTextID = GetPlayerResultTextID(allEnemyBlocksVisible, playerManeuversCount, casualtiesDataMin, casualtiesDataMax, squadronData);
        //playerResultText.text = playerResultTextID;
        playerResultText.text = LocalizationManager.Instance.GetText(playerResultTextID, damagedSquadronsCountMin.ToString(), damagedSquadronsCountMax.ToString());

        int destroyedEnemyCountMin = casualtiesDataMax.EnemyDestroyedIndices.Count;
        int destroyedEnemyCountMax = casualtiesDataMin.EnemyDestroyedIndices.Count;

        string enemyResultTextID = GetEnemyResultTextID(allEnemyBlocksVisible, damagedBlocks, uncertainDamagedBlocks, enemyManeuverDatas.Count, playerManeuversCount, casualtiesDataMin, casualtiesDataMax, squadronData);
        //enemyResultText.text = enemyResultTextID;
        enemyResultText.text = LocalizationManager.Instance.GetText(enemyResultTextID, destroyedEnemyCountMin.ToString(), destroyedEnemyCountMax.ToString());
    }

    public void SetShowHighlight(bool show, int enemyIndex)
    {
        enemyStrategyPanel.SetShowHighlight(show && !Drag, enemyIndex);
    }

    public EManeuverType GetCurrentCategory()
    {
        for (int i = 0; i < cardPlaces.Count; i++)
        {
            if (cardsPanels[i].gameObject.activeSelf)
            {
                return (EManeuverType)i;
            }
        }
        return (EManeuverType)(-1);
    }

    public void Hide()
    {
        if (container.activeSelf)
        {
            container.SetActive(false);
            HudManager.Instance.ChangeTimeSpeed(lastTime);

            var map = TacticManager.Instance.Map;
            map.ClearRaidWaypoints();
            map.ChangeAirRaidMode(true, TacticManager.Instance.CurrentMission);
        }
        HudManager.Instance.PopupHidden(this);
    }

    public void ForceStrategy(List<StrategyData> list)
    {
        if (list == null)
        {
            ForcedStrategy = null;
        }
        else
        {
            ForcedStrategy = new Dictionary<PlayerManeuverData, int>();
            foreach (var data in list)
            {
                ForcedStrategy.Add(data.Strategy, data.SlotIndex);
            }
        }
    }

    public int IndexOf(CardPlace place)
    {
        return cardPlaces.IndexOf(place);
    }

    public void SetShowSprite(bool show, List<int> targets, int objective)
    {
        enemyStrategyPanel.SetShowSprite(show, targets, objective);
    }

    public void SaveStrategySprites(ref StrategyVisualsSaveData data)
    {
        enemyStrategyPanel.SaveStrategySprites(ref data);
    }

    public void LoadStrategySprites(ref StrategyVisualsSaveData data)
    {
        enemyStrategyPanel.LoadStrategySprites(ref data);
    }

    public void DropHere(int slot)
    {
        cardPlaces[slot].DropHere.SetActive(true);
    }

    public void ResetDrops(int slot)
    {
        if (slot == -1)
        {
            foreach (var cardPlace in cardPlaces)
            {
                cardPlace.DropHere.SetActive(false);
            }
        }
        else
        {
            cardPlaces[slot].DropHere.SetActive(false);
        }
    }

    private string GetPlayerResultTextID(bool allEnemyBlocksVisible, int playerManeuversCount, CasualtiesData casualtiesDataMin, CasualtiesData casualtiesDataMax, FightSquadronData squadronData)
    {
        bool allPlayerSquadronsDestroyed = casualtiesDataMin.AllSquadronsDestroyed(squadronData);
        int destroyedSquadronsCountMin = casualtiesDataMin.GetDestroyedSquadronsCount();
        int destroyedSquadronsCountMax = casualtiesDataMax.GetDestroyedSquadronsCount();
        //bool playerLossesAreCertain = casualtiesDataMin.CompareSquadronsDestroyed(casualtiesDataMax);
        bool playerLossesAreCertain = destroyedSquadronsCountMin == destroyedSquadronsCountMax;

        string prefix = allEnemyBlocksVisible ? "KnownEnemy" : "UnknownEnemy";
        string playerResultTextID;
        if (playerManeuversCount <= 1)
        {
            playerResultTextID = "PlayerResultTooFewManeuvers";
        }
        else
        {
            if (allPlayerSquadronsDestroyed)
            {
                playerResultTextID = "PlayerResultCertainTotalLosses";
            }
            else
            {
                if (destroyedSquadronsCountMin == 0)
                {
                    if (destroyedSquadronsCountMax == 0)
                    {
                        playerResultTextID = "PlayerResultCertainNoLosses";
                    }
                    else if (destroyedSquadronsCountMax == 1)
                    {
                        playerResultTextID = "PlayerResultUncertainLostOneSquadron";
                    }
                    else if (allEnemyBlocksVisible)
                    {
                        playerResultTextID = "PlayerResultUncertainPartialLosses";
                    }
                    else
                    {
                        playerResultTextID = "PlayerResultUncertainLostOneSquadron";
                    }
                }
                else
                {
                    if (!allEnemyBlocksVisible || !playerLossesAreCertain)
                    {
                        playerResultTextID = "PlayerResultUncertainPartialLosses";
                    }
                    else
                    {
                        playerResultTextID = "PlayerResultCertainPartialLosses";
                    }
                }
            }
        }
        return prefix + playerResultTextID;
    }

    private string GetEnemyResultTextID(bool allEnemyBlocksVisible, bool damagedBlocks, bool uncertainDamagedBlocks, int enemyCount, int playerManeuversCount, CasualtiesData casualtiesDataMin, CasualtiesData casualtiesDataMax, FightSquadronData squadronData)
    {
        bool allEnemiesDestroyed = casualtiesDataMax.EnemyDestroyedIndices.Count == enemyCount;
        int destroyedEnemyCountMin = casualtiesDataMin.EnemyDestroyedIndices.Count;
        int destroyedEnemyCountMax = casualtiesDataMax.EnemyDestroyedIndices.Count;
        bool enemyLossesAreCertain = destroyedEnemyCountMin == destroyedEnemyCountMax;

        string prefix = allEnemyBlocksVisible ? "KnownEnemy" : "UnknownEnemy";
        string enemyResultTextID;
        if (playerManeuversCount <= 1)
        {
            enemyResultTextID = "EnemyResultTooFewManeuvers";
        }
        else
        {
            if (allEnemiesDestroyed)
            {
                enemyResultTextID = "EnemyResultCertainTotalLosses";
            }
            else
            {
                if (destroyedEnemyCountMin == 0 && destroyedEnemyCountMax == 0)
                {
                    if (damagedBlocks)
                    {
                        if (uncertainDamagedBlocks)
                        {
                            enemyResultTextID = "EnemyResultUncertainPartialLossesDurability";
                        }
                        else
                        {
                            enemyResultTextID = "EnemyResultCertainPartialLossesDurability";
                        }
                    }
                    else
                    {
                        enemyResultTextID = "EnemyResultCertainNoLosses";
                    }
                }
                else
                {
                    if (!allEnemyBlocksVisible && !enemyLossesAreCertain && destroyedEnemyCountMin == 0)
                    {
                        enemyResultTextID = "EnemyResultUncertainPartialLossesDurability";
                    }
                    else if (!allEnemyBlocksVisible || !enemyLossesAreCertain)
                    {
                        enemyResultTextID = "EnemyResultUncertainPartialLosses";
                    }
                    else
                    {
                        enemyResultTextID = "EnemyResultCertainPartialLosses";
                    }
                }
            }
        }
        return prefix + enemyResultTextID;
    }

    private void SetConfirmActive(bool active)
    {
        confirmButton.interactable = active;
    }

    private void OnCardsButtonClick(Button button, EManeuverType category)
    {
        cardsBackground.SetAsLastSibling();
        button.GetComponent<RectTransform>().SetAsLastSibling();
        SelectPanel(category);
    }

    private void OnConfirmButtonClick()
    {
        var tacMan = TacticManager.Instance;
        tacMan.Bombers = bombers;
        tacMan.Fighters = fighters;
        tacMan.Torpedoes = torpedoes;
        tacMan.Map.CanAddRetrievalWaypoint = true;
        tacMan.PlayerManevuers = playerManeuverDatas;

        enemyShip = null;
        container.SetActive(false);
        HudManager.Instance.ChangeTimeSpeed(lastTime);
    }

    private void DisableCards(TacticalEnemyShip enemyShip, List<EnemyManeuverData> enemyManeuvers)
    {
        int j = 0;
        for (int i = 0; i < enemyShip.Blocks.Count; i++)
        {
            if (enemyShip.Blocks[i].Dead)
            {
                continue;
            }
            foreach (var modifier in enemyManeuvers[j++].Modifiers)
            {
                bool disable = modifier.Effect == EBonusValue.DisableAll;
                bool disableModifier = modifier.Effect == EBonusValue.DisableModifier;
                bool disableParameters = modifier.Effect == EBonusValue.MultiplyAttackParameters;
                if (disable || disableModifier || disableParameters)
                {
                    switch (modifier.ModifierType)
                    {
                        case EBonusType.AllOfManeuverType:
                            foreach (var card in cards)
                            {
                                if (!card.Blocker.activeSelf && card.Data.ManeuverType == modifier.Data.ManeuverType)
                                {
                                    if (disableParameters)
                                    {
                                        card.DebuffIndexSecondaries.Add(i);
                                    }
                                    else
                                    {
                                        card.BlockerIndex = i;
                                        (disableModifier ? card.ModifierBlocker : card.Blocker).SetActive(true);
                                    }
                                }
                            }
                            break;
                        case EBonusType.AllOfSquadronType:
                            foreach (var card in cards)
                            {
                                if (!card.Blocker.activeSelf && card.Data.NeededSquadrons.Type == modifier.SquadronType)
                                {
                                    if (disableParameters)
                                    {
                                        card.DebuffIndexSecondaries.Add(i);
                                    }
                                    else
                                    {
                                        card.BlockerIndex = i;
                                        (disableModifier ? card.ModifierBlocker : card.Blocker).SetActive(true);
                                    }
                                }
                            }
                            break;
                        case EBonusType.SpecificPiece:
                            if (disableParameters)
                            {
                                debuffModifiersPlaces[modifier.Data.Slot] = i;
                                debuffModifiersPlaces[modifier.Data.Various] = i;
                            }
                            else if (disable)
                            {
                                cardPlaces[modifier.Data.Slot].SetBlocker(true, i);
                                if (modifier.Data.Various != -1)
                                {
                                    cardPlaces[modifier.Data.Various].SetBlocker(true, i);
                                }
                            }
                            else
                            {
                                blockedModifiersPlaces[modifier.Data.Slot] = i;
                                blockedModifiersPlaces[modifier.Data.Various] = i;
                            }
                            break;
                    }
                }
            }
        }
    }

    private void ResetCards()
    {
        foreach (var cardPlace in cardPlaces)
        {
            cardPlace.CardPlaced(null);
            cardPlace.SetBlocker(false);
        }

        foreach (var card in cards)
        {
            card.ResetPlacement();
            card.BlockerIndex = -1;
            card.DebuffIndices.Clear();
            card.DebuffIndexSecondaries.Clear();
            card.Blocker.SetActive(false);
            card.ModifierBlocker.SetActive(false);
        }
        blockedModifiersPlaces.Clear();
        debuffModifiersPlaces.Clear();
    }

    private void OnToggled(bool toggled)
    {
        enemyShip = null;
    }
}
