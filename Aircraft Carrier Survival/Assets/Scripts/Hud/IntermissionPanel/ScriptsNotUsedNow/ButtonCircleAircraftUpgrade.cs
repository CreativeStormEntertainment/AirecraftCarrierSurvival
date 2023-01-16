using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonCircleAircraftUpgrade : MonoBehaviour
{
    /*
    [SerializeField] private GameObject buttonsToShowHide = null;
    [SerializeField] private List<ButtonAircraftUpgrade> buttons = null;

    public int ItemType = 0;
    public int ItemIndex = 0;

    public ButtonAircraftUpgrade Selected = null;

    public Toggle Toggle;

    private void Awake()
    {
        Toggle = GetComponent<Toggle>();

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetUpgradeLvl(i);
        }
    }

    private void Start()
    {
        ShowSelected();
        SetIcons();
    }

    public void ShowUpgrades(bool isSelected)
    {
        if (isSelected)
        {
            buttonsToShowHide.SetActive(true);
        }
        else
        {
            buttonsToShowHide.SetActive(false);
        }
    }

    public void SetIcons()
    {
        var saveData = SaveManager.Instance.Data;
        var current = IntermissionManager.Instance.GetUpgradesList()[ItemType][ItemIndex];
        foreach (ButtonAircraftUpgrade button in buttons)
        {
            button.Icon.sprite = current.GetImageIcon();
            if ((saveData.LockedUpgrades & button.UpgradeLvID) == button.UpgradeLvID)
            {
                button.LockedIcon.SetActive(true);
                button.LockedIcon.GetComponent<Image>().sprite = current.GetLockedIcon();

                button.PriceText.SetActive(true);
                button.Price.SetActive(true);
                button.Price.GetComponent<Text>().text = current.GetUnlockCost().ToString();
                button.UnlockCost = current.GetUnlockCost();
            }
            else
            {
                button.LockedIcon.SetActive(false);
                button.PriceText.SetActive(false);
                button.Price.SetActive(false);
            }
        }

    }

    public void ShowSelected()
    {
        var saveData = SaveManager.Instance.Data;

        Assert.AreEqual(saveData.AllUpgradesLv.Count, transform.parent.GetComponentsInChildren<ButtonAircraftUpgrade>(true).Length);

        var button = buttons[saveData.AllUpgradesLv[ItemType * 3 + ItemIndex]];
        button.GetComponent<Image>().color = new Color32(255, 255, 0, 255);
        Selected = button;
    }
    */
}
