using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAircraftUpgrade : MonoBehaviour
{
    /*
    public Image Icon;
    public GameObject LockedIcon;
    public GameObject PriceText;
    public GameObject Price;
    [SerializeField] private GameObject panelToShowHide = null;
    [SerializeField] private ButtonCircleAircraftUpgrade buttonCircle = null;
    public int upgradeLvl = 0;

    [NonSerialized]
    public int UpgradeLvID;
    [NonSerialized]
    public int UnlockCost = -1;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        var saveData = SaveManager.Instance.Data;
        if (UnlockCost != -1)
        {
            if (UnlockCost <= saveData.CommandPoints)
            {
                LockedIcon.SetActive(false);
                PriceText.SetActive(false);
                Price.SetActive(false);

                IntermissionPanel.Instance.ReduceCommandPoints(UnlockCost);
                saveData.LockedUpgrades &= ~UpgradeLvID;
                UnlockCost = -1;
            }
        }
        else
        {
            if (buttonCircle.Selected != null && buttonCircle.Selected != this)
            {
                buttonCircle.Selected.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            buttonCircle.Selected = this;


#warning to change
            gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255);

            panelToShowHide.SetActive(false);
            buttonCircle.Toggle.isOn = false;

            saveData.AllUpgradesLv[buttonCircle.ItemType * 3 + buttonCircle.ItemIndex] = upgradeLvl;
        }
    }

    public void SetUpgradeLvl(int upgradeLvl)
    {
        this.upgradeLvl = upgradeLvl;
        UpgradeLvID = 1 << (upgradeLvl + buttonCircle.ItemType * 3);
    }
    */
}