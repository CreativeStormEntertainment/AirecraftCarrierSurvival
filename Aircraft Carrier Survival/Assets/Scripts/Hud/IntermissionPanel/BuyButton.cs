using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{
    public event Action Clicked = delegate { };

    public int MaxUpgrades
    {
        get;
        set;
    }

    public bool CommandPoints
    {
        get;
        set;
    }

    [SerializeField]
    protected Text costText = null;

    [SerializeField]
    protected Button button = null;

    [SerializeField]
    protected Image icon = null;

    [SerializeField]
    protected List<Sprite> icons = null;

    protected int cost;

    [SerializeField]
    private bool showPopup = true;

    private bool active;
    private bool isUnlock;

    private bool setuped;

    public void Setup(bool commandPoints, int maxUpgrades, Action upgrade)
    {
        CommandPoints = commandPoints;
        MaxUpgrades = maxUpgrades;
        if (!setuped)
        {
            button.onClick.AddListener(() =>
            {
                AskForUpgrades(upgrade);
                Clicked();
            });
            setuped = true;
        }
    }

    public virtual void Refresh()
    {
        var interMan = IntermissionManager.Instance;
        button.interactable = active && ((CommandPoints ? interMan.CurrentCommandPoints : interMan.CurrentUpgradePoints) >= cost);
    }

    public virtual void SetUpgrade(int upgrade, int cost, bool isUnlock)
    {
        this.isUnlock = isUnlock;
        active = upgrade < MaxUpgrades;
        this.cost = cost;
        costText.text = Mathf.Abs(cost).ToString();
        //icon.sprite = icons[upgrade];
        Refresh();
    }

    private void AskForUpgrades(Action upgrade)
    {
        IntermissionManager.Instance.ShowUpgradePopup(isUnlock, new UpgradePopupData(cost, CommandPoints, upgrade), showPopup);
    }
}
