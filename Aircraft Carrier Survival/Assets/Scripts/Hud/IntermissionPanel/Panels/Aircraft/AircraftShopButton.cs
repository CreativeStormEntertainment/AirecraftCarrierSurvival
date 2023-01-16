using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftShopButton : ShopButton<AircraftIntermissionData>, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<EPlaneType, bool> Highlight = delegate { };

    public override bool Unlocked
    {
        get => base.Unlocked;
        set
        {
            CommandPoints = value;
            Data.Unlocked = value;
            if (value)
            {
                SetUpgrade(0, Data.BuyCosts[Data.CurrentTier] * Data.Count, false);
                buttonText.text = buy;
            }
            else
            {
                buttonText.text = unlock;
                SetUpgrade(Data.CurrentTier + 1, Data.UnlockCost, true);
                hideOnMaxUpgrade.SetActive(Data.CurrentTier < 2);
            }
        }
    }

    [SerializeField]
    private Image planeImage = null;
    [SerializeField]
    private Image planeTypeImage = null;
    [SerializeField]
    private Text planeName = null;
    [SerializeField]
    private PlanesTiersData planesTiers = null;
    [SerializeField]
    private Color commandPointsColor = Color.green;
    [SerializeField]
    private Color upgradePointsColor = Color.yellow;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private int minSquadronsCount = 4;
    [SerializeField]
    private Button plusButton = null;
    [SerializeField]
    private Button minusButton = null;
    [SerializeField]
    private GameObject hideOnMaxUpgrade = null;

    private Action click;
    private bool highlight;
    private bool allowBuy = true;

    private void Start()
    {
        plusButton.onClick.AddListener(() => Click(true));
        minusButton.onClick.AddListener(() => Click(false));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight = true;
        Highlight(Data.Type, true);
        if (Data.Unlocked)
        {
            Data.HighlightAction(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(Data.Type, false);
        highlight = false;
        Data.HighlightAction(false);
    }

    public override void Setup(AircraftIntermissionData data, int index, Action click)
    {
        this.click = click;

        base.Setup(data, index, OnClick);
        button.gameObject.SetActive(false);
        MaxUpgrades = 3;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        var interMan = IntermissionManager.Instance;
        minusButton.gameObject.SetActive(CommandPoints);
        if (CommandPoints)
        {
            plusButton.interactable = interMan.CurrentCommandPoints >= cost && allowBuy;
            minusButton.interactable = Data.CurrentCount >= minSquadronsCount;
        }
        else
        {
            plusButton.interactable = interMan.CurrentUpgradePoints >= cost;
        }
        var locMan = LocalizationManager.Instance;
        var data = planesTiers.Data.Find(item => item.PlaneType == Data.Type);
        planeImage.sprite = data.PlaneTiers[Data.CurrentTier].Sprite;
        planeName.text = data.PlaneTiers[Data.CurrentTier].Name;
        planeTypeImage.sprite = data.PlaneTypeSprite;
        description.text = locMan.GetText(data.PlaneType.ToString());
        if (Data.CurrentTier > 0)
        {
            description.text += "\n" + locMan.GetText(data.PlaneType.ToString() + "Skill1");
        }
        if (Data.CurrentTier == 2)
        {
            description.text += "\n" + locMan.GetText(data.PlaneType.ToString() + "Skill2");
        }
        if (highlight && Data.Unlocked)
        {
            Data.HighlightAction(true);
        }
    }

    public void SetBuy(bool buy)
    {
        Unlocked = buy;
        hideOnMaxUpgrade.SetActive(buy || Data.CurrentTier < 2);
        cost = buy ? Data.BuyCosts[Data.CurrentTier] : Data.UnlockCost;
    }

    public override void SetUpgrade(int upgrade, int cost, bool isUnlock)
    {
        base.SetUpgrade(upgrade, cost, isUnlock);
        icon.sprite = icons[upgrade];
        costText.color = Unlocked ? commandPointsColor : upgradePointsColor;
    }

    public void AllowBuy(bool allow)
    {
        allowBuy = allow;
        Refresh();
    }

    private void OnClick()
    {
        click();
    }

    private void Click(bool add)
    {
        Data.Count = add ? 1 : -1;
        Unlocked = Unlocked;
        button.onClick.Invoke();
    }
}
