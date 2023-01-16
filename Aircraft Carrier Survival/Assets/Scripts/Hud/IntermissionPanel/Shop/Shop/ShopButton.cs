using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton<T> : BuyButton where T : IShopButtonData
{
    public virtual bool Unlocked
    {
        get => CommandPoints;
        set
        {
            CommandPoints = value;
            Data.Unlocked = value;
            if (value)
            {
                SetUpgrade(0, Data.BuyCost, false);
                buttonText.text = buy;
            }
            else
            {
                SetUpgrade(1, Data.UnlockCost, true);
                buttonText.text = unlock;
            }
        }
    }

    public T Data
    {
        get;
        protected set;
    }

    public int Index
    {
        get;
        private set;
    }

    [SerializeField]
    protected Text buttonText = null;

    protected string buy;
    protected string unlock;

    [SerializeField]
    private string buyID = "BuyID";
    [SerializeField]
    private string unlockID = "UnlockID";

    public virtual void Setup(T data, int index, Action click)
    {
        Index = index;

        var locMan = LocalizationManager.Instance;
        if (buy == null)
        {
            buy = locMan.GetText(buyID);
        }
        if (unlock == null)
        {
            unlock = locMan.GetText(unlockID);
        }

        Data = data;
        Setup(false, 2, click);
        Unlocked = data.Unlocked;
    }
}
