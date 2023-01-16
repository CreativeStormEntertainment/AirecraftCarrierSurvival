using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoseBuffWindow : MonoBehaviour
{
    [SerializeField]
    private MissionSuccessLeftPanel leftPanel = null;
    [SerializeField]
    private List<BuffCard> buffCards = null;
    [SerializeField]
    private List<ManeuversCard> maneuversCards = null;
    [SerializeField]
    private BuffBaskets buffBaskets = null;
    [SerializeField]
    private Button airButton = null;
    [SerializeField]
    private Button navyButton = null;
    [SerializeField]
    private ManeuversList maneuversList = null;

    private List<int> randomlySelectedManeuvers = new List<int>();
    private List<IslandBuff> randomlySelectedBuffs = new List<IslandBuff>();
    private List<IslandBuff> greenBasket = new List<IslandBuff>();
    private List<IslandBuff> orangeBasket = new List<IslandBuff>();
    private List<IslandBuff> redBasket = new List<IslandBuff>();

    private int upgradeLevel;

    private void Start()
    {
        airButton.onClick.AddListener(() => UpgradePoints(true));
        navyButton.onClick.AddListener(() => UpgradePoints(false));
    }

    public void Setup(SandboxAdmiralLevel levelReward)
    {
        if (levelReward.Maneuver != null)
        {
            var maneuver = maneuversList.Maneuvers.Find(item => item.ManeuverType == levelReward.Maneuver.ManeuverType);
            int index = maneuversList.Maneuvers.IndexOf(maneuver);
            var levels = SaveManager.Instance.Data.ManeuversLevels;
            upgradeLevel = levels[index] + 1;
            AddToLeftPanel(maneuver);
        }
        else if (levelReward.Order)
        {
            SetupBuffs();
        }
        else if (levelReward.AirNavy)
        {
            SetupPoints();
        }
        else if (levelReward.UpgradePoints > 0)
        {
            leftPanel.GetNextFreeTab().SetupUpgradePoints(levelReward.UpgradePoints);
            leftPanel.NextUpgrade();
        }
    }

    public void SetupManeuverChoice()
    {
        HideAll();
        var tacMan = TacticManager.Instance;
        var maneuversSave = SaveManager.Instance.Data.ManeuversLevels;
        upgradeLevel = 2;
        randomlySelectedManeuvers.Clear();
        for (int i = 0; i < tacMan.PlayerManeuversList.AdmiralManeuversCount; i++)
        {
            if (maneuversSave[i] == 1)
            {
                randomlySelectedManeuvers.Add(i);
            }
        }
        if (randomlySelectedManeuvers.Count == 0)
        {
            upgradeLevel = 3;
            for (int i = 0; i < tacMan.PlayerManeuversList.AdmiralManeuversCount; i++)
            {
                if (maneuversSave[i] == 2)
                {
                    randomlySelectedManeuvers.Add(i);
                }
            }
        }
        while (randomlySelectedManeuvers.Count > 3)
        {
            randomlySelectedManeuvers.RemoveAt(Random.Range(0, randomlySelectedManeuvers.Count));
        }
        for (int i = 0; i < maneuversCards.Count; i++)
        {
            PlayerManeuverData maneuver = null;
            PlayerManeuverData baseManeuver = null;
            if (randomlySelectedManeuvers.Count > i)
            {
                baseManeuver = tacMan.AllPlayerManeuvers[randomlySelectedManeuvers[i]];
                if (upgradeLevel == 2)
                {
                    maneuver = baseManeuver.Level2;
                }
                else if (upgradeLevel == 3)
                {
                    maneuver = baseManeuver.Level3;
                }
            }
            maneuversCards[i].Setup(maneuver, upgradeLevel - 1, null, this, baseManeuver);
        }
        gameObject.SetActive(true);
        if (randomlySelectedManeuvers.Count == 0)
        {
            leftPanel.NextUpgrade();
            gameObject.SetActive(false);
        }
    }

    public void SetupBuffs()
    {
        HideAll();
        randomlySelectedBuffs.Clear();

        GetLockedBuffs(buffBaskets.GreenBasket, greenBasket);
        GetLockedBuffs(buffBaskets.OrangeBasket, orangeBasket);
        GetLockedBuffs(buffBaskets.RedBasket, redBasket);

        randomlySelectedBuffs.Add(GetBuff(greenBasket, orangeBasket, redBasket));
        randomlySelectedBuffs.Add(GetBuff(orangeBasket, greenBasket, redBasket));
        randomlySelectedBuffs.Add(GetBuff(redBasket, greenBasket, orangeBasket));

        for (int i = 0; i < Mathf.Min(randomlySelectedBuffs.Count, buffCards.Count); i++)
        {
            buffCards[i].Setup(randomlySelectedBuffs[i], true, this);
        }
        gameObject.SetActive(true);
    }

    public void SetupPoints()
    {
        HideAll();
        airButton.gameObject.SetActive(true);
        navyButton.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddToLeftPanel(IslandBuff buff)
    {
        leftPanel.ChooseOrder(buff);
        Hide();
    }

    public void AddToLeftPanel(PlayerManeuverData maneuver)
    {
        leftPanel.ChooseManeuver(maneuver, upgradeLevel);
        Hide();
    }

    private void HideAll()
    {
        foreach (var buffCard in buffCards)
        {
            buffCard.Hide();
        }
        airButton.gameObject.SetActive(false);
        navyButton.gameObject.SetActive(false);
        foreach (var card in maneuversCards)
        {
            card.gameObject.SetActive(false);
        }
    }

    private IslandBuff GetBuff(List<IslandBuff> basketA, List<IslandBuff> basketB, List<IslandBuff> basketC)
    {
        return GetRandomBuff(basketA) ?? (basketB.Count <= basketC.Count ? GetRandomBuff(basketB) : GetRandomBuff(basketC));
    }

    private IslandBuff GetRandomBuff(List<IslandBuff> basket)
    {
        if (basket.Count > 0)
        {
            int rand = Random.Range(0, basket.Count);
            IslandBuff buff = basket[rand];
            basket.RemoveAt(rand);
            return buff;
        }
        return null;
    }

    private void GetLockedBuffs(List<EIslandBuff> basket, List<IslandBuff> buffs)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        buffs.Clear();
        foreach (var buff in basket)
        {
            if (!islMan.IslandBuffs[buff].Unlocked)
            {
                if (GameStateManager.Instance != null)
                {
                    if ((GameStateManager.Instance.MissionSuccessPopup.MissionRewards.AdmiralUnlockedOrders & (1 << (int)buff)) == 0)
                    {
                        buffs.Add(islMan.IslandBuffs[buff]);
                    }
                }
                else
                {
                    if ((IntermissionManager.Instance.SandboxMainGoalSummary.MissionRewards.AdmiralUnlockedOrders & (1 << (int)buff)) == 0)
                    {
                        buffs.Add(islMan.IslandBuffs[buff]);
                    }
                }
            }
        }
    }

    private void UpgradePoints(bool air)
    {
        var saveData = SaveManager.Instance.Data;
        if (air)
        {
            saveData.AdmiralAirLevel++;
        }
        else
        {
            saveData.AdmiralNavyLevel++;
        }
        Hide();
        leftPanel.ChoosePoints(air);
    }
}
