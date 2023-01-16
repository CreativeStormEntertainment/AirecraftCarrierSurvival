using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarrierPanel : Panel
{
    public event Action<int> HangarUpgraded = delegate { };

    [SerializeField]
    private MyInputField carrierName = null;

    [SerializeField]
    private Button carrierTypeButton = null;

    [SerializeField]
    private Text carrierTypeText = null;

    [SerializeField]
    private GameObject carrierChooser = null;

    [SerializeField]
    private Button cheatCarrierButton = null;

    [SerializeField]
    private List<Text> carrierChooserTexts = null;

    [SerializeField]
    private List<string> carrierNameIDs = null;

    [SerializeField]
    private List<GameObject> carrierModels = null;

    [SerializeField]
    private CrewPanel crewPanel = null;

    [SerializeField]
    private List<CarrierUpgradeData> carrierRequirements = null;

    [SerializeField]
    private List<GameObject> carrierSelection = null;

    [SerializeField]
    private GameObject newCarrierAvailable = null;

    [SerializeField]
    private CarrierUnlock unlocker = null;

    [SerializeField]
    private List<Button> carriers = null;

    private int currentCarrierWindow;

    public override void Setup(NewSaveData data)
    {
        base.Setup(data);

        currentCarrierWindow = -1;

        carrierName.Setup(data.CarrierName);

        controls.Setup(data.IntermissionData.CarriersUpgrades, (int)currentCarrier);

        controls[2].Upgraded += () => HangarUpgraded(controls[2].GetCurrentUpgrade());
        HangarUpgraded(controls[2].GetCurrentUpgrade());
        carrierTypeButton.onClick.AddListener(() => SetShowChooser(!carrierChooser.activeSelf));

#if ALLOW_CHEATS
        cheatCarrierButton.onClick.AddListener(EnableCarriers);
#else
        cheatCarrierButton.gameObject.SetActive(false);
#endif
        unlocker.CarrierChanged += () => SelectCarrier(currentCarrierWindow, false);
        unlocker.Init(false);
        for (int i = 0; i < carriers.Count; i++)
        {
            int index = i;
            carriers[i].onClick.AddListener(() => OnCarrierSelected(index));
        }
        for (int i = 0; i < carrierChooserTexts.Count; i++)
        {
            carrierChooserTexts[i].text = LocalizationManager.Instance.GetText(carrierNameIDs[i]);
        }
        carrierTypeText.text = carrierChooserTexts[(int)currentCarrier].text;
        SelectCarrier((int)data.SelectedAircraftCarrier, true);

        InnerRefresh(-1);
    }

    public override void Save(NewSaveData data)
    {
        data.CarrierName = carrierName.Text;
        data.IntermissionData.CarriersUpgrades = controls.Save();
    }

    protected override void InnerRefresh(int prevCarrier)
    {
        base.InnerRefresh(prevCarrier);

        var saveData = SaveManager.Instance.Data;
        if (prevCarrier != -1)
        {
            int carrier = (int)currentCarrier;
            carrierTypeText.text = carrierChooserTexts[carrier].text;

            saveData.IntermissionData.AvailableCarriers |= (1 << carrier);
        }

        bool show = false;
        for (int i = 0; i < carriers.Count; i++)
        {
            if ((saveData.IntermissionData.AvailableCarriers & (1 << i)) != 0)
            {
                continue;
            }
            var requirementData = carrierRequirements[i];
            if (!requirementData.Active)
            {
                requirementData.Active = true;
                CheckControl(ref requirementData.Active, ref requirementData.Radar, 0, i - 1);
                CheckControl(ref requirementData.Active, ref requirementData.AA, 1, i - 1);
                CheckControl(ref requirementData.Active, ref requirementData.Hangar, 2, i - 1);

                var carrier = (ECarrierType)i - 1;
                if (requirementData.Officer == -1 || crewPanel.GetOfficerUpgrade(carrier) >= requirementData.Officer)
                {
                    requirementData.Officer = -1;
                }
                else
                {
                    requirementData.Active = false;
                }
                if (requirementData.Crew == -1 || crewPanel.GetCrewUpgrade(carrier) >= requirementData.Crew)
                {
                    requirementData.Crew = -1;
                }
                else
                {
                    requirementData.Active = false;
                }
            }

            if (requirementData.Active)
            {
                show = true;
            }
        }

        show = saveData.GameMode != EGameMode.Sandbox && show;
        newCarrierAvailable.SetActive(show);

        UpdateSelection(currentCarrierWindow);
        HangarUpgraded(controls[2].GetCurrentUpgrade());
    }

    private void SetShowChooser(bool show)
    {
        carrierChooser.SetActive(show);
        if (!show)
        {
            currentCarrierWindow = -1;
            unlocker.gameObject.SetActive(false);
        }
    }

    private void OnCarrierSelected(int index)
    {
        UpdateSelection(index);
    }

    private void SelectCarrier(int index, bool setup)
    {
        SetShowChooser(false);
        if (!setup)
        {
            var data = SaveManager.Instance.Data;
            data.OfficersLastRooms.Clear();
            data.LastSwitches.Clear();
        }
        for (int i = 0; i < carrierModels.Count; i++)
        {
            carrierModels[i].SetActive(i == index);
        }
        IntermissionManager.Instance.CurrentCarrier = (ECarrierType)index;
        carrierModels[index].SetActive(true);
    }

    private void EnableCarriers()
    {
        SaveManager.Instance.Data.IntermissionData.AvailableCarriers = (1 << 3) - 1;
        cheatCarrierButton.gameObject.SetActive(false);
        InnerRefresh(-1);
    }

    private void CheckControl(ref bool active, ref int controlValue, int control, int carrier)
    {
        if (controlValue == -1 || controls[control].GetUpgrade(carrier) >= controlValue)
        {
            controlValue = -1;
        }
        else
        {
            active = false;
        }
    }

    private void UpdateSelection(int current)
    {
        foreach (var selection in carrierSelection)
        {
            selection.SetActive(false);
        }

        if (current > -1)
        {
            currentCarrierWindow = current;
            carrierSelection[current].SetActive(true);
            unlocker.Setup(carrierRequirements[current], (SaveManager.Instance.Data.IntermissionData.AvailableCarriers & (1 << current)) != 0, current);
        }
    }
}
