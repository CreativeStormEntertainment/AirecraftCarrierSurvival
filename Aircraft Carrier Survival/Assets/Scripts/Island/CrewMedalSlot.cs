using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrewMedalSlot : DraggableMedalSlot
{
    [SerializeField]
    private Image crewImage = null;
    [SerializeField]
    private Image specAImage = null;
    [SerializeField]
    private Image specBImage = null;

    private int crewIndex;
    private CrewUpgradeWindow crewUpgradeWindow;
    private MedalsWindow medalsWindow;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        medalsWindow.CrewInspector.Setup(GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades[crewIndex]);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        medalsWindow.CrewInspector.Hide();
    }

    public void Setup(int index, CrewUpgradeWindow upgradeWindow, MedalsWindow medalsWindow)
    {
        medalImage.SetActive(false);
        crewIndex = index;
        var crewUpgrade = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades[crewIndex];
        crewUpgradeWindow = upgradeWindow;
        this.medalsWindow = medalsWindow;
        crewImage.sprite = CrewManager.Instance.CrewDataList.List[crewUpgrade.CrewDataIndex].Portrait;
        UpdateLevelVisualization();
        Blocked = false;
    }

    public override void AssignMedal(DraggableMedal medal)
    {
        if (!Blocked)
        {
            base.AssignMedal(medal);
            Blocked = true;
            var crewUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades;
            var upgrade = crewUpgrades[crewIndex];
            if (upgrade.Medals < 2)
            {
                crewUpgradeWindow.Setup(crewIndex);
                medalsWindow.LevelUpSound.Play();
            }
            else
            {
                medalsWindow.MedalSound.Play();
            }
            upgrade.Medals++;
            crewUpgrades[crewIndex] = upgrade;

            UpdateLevelVisualization();
        }
    }

    public void UpdateLevelVisualization()
    {
        var officersUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades;
        var upgrade = officersUpgrades[crewIndex];
        levelImage.sprite = levelFrames[Mathf.Min(upgrade.Medals, 2)];
        medalsCount.text = upgrade.Medals.ToString();

        var savedSpecialities = new List<ECrewmanSpecialty>(upgrade.GetSpecialties());
        crewUpgradeWindow.SetupSpecImage(specAImage, null);
        crewUpgradeWindow.SetupSpecImage(specBImage, null);
        if (savedSpecialities.Count > 0)
        {
            if (CrewManager.Instance.CrewSpecialtiesDict.TryGetValue(savedSpecialities[0], out var specData))
            {
                crewUpgradeWindow.SetupSpecImage(specAImage, specData);
            }
        }
        if (savedSpecialities.Count > 1)
        {
            if (CrewManager.Instance.CrewSpecialtiesDict.TryGetValue(savedSpecialities[1], out var specDataB))
            {
                crewUpgradeWindow.SetupSpecImage(specBImage, specDataB);
            }
        }
    }
}
