using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewUpgradeWindow : MonoBehaviour
{
    [SerializeField]
    private List<CrewSpecialityCard> specialityCards = null;

    [SerializeField]
    private Image crewImage = null;
    [SerializeField]
    private Image specAImage = null;
    [SerializeField]
    private Image specBImage = null;
    [SerializeField]
    private Image levelImage = null;
    [SerializeField]
    protected List<Sprite> levelFrames = null;

    private int crewIndex;
    private MedalsWindow medalsWindow;

    public void Init(MedalsWindow medalsWindow)
    {
        this.medalsWindow = medalsWindow;
    }

    public void Setup(int crewIndex)
    {
        var crewUpgrade = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades[crewIndex];
        this.crewIndex = crewIndex;

        var savedSpecialities = new List<ECrewmanSpecialty>(crewUpgrade.GetSpecialties());
        if (savedSpecialities.Count > 2)
        {
            SetShow(false);
            return;
        }
        SetupSpecImage(specAImage, null);
        SetupSpecImage(specBImage, null);
        if (savedSpecialities.Count > 0)
        {
            if (CrewManager.Instance.CrewSpecialtiesDict.TryGetValue(savedSpecialities[0], out var specData))
            {
                SetupSpecImage(specAImage, specData);
            }
        }
        if (savedSpecialities.Count > 1)
        {
            if (CrewManager.Instance.CrewSpecialtiesDict.TryGetValue(savedSpecialities[1], out var specDataB))
            {
                SetupSpecImage(specBImage, specDataB);
            }
        }
        levelImage.sprite = levelFrames[Mathf.Min(crewUpgrade.Medals, 2)];
        crewImage.sprite = CrewManager.Instance.CrewDataList.List[crewUpgrade.CrewDataIndex].Portrait;

        var specialities = new List<ECrewmanSpecialty>();
        for (int index = 1; index < (int)ECrewmanSpecialty.Count; index++)
        {
            var spec = (ECrewmanSpecialty)index;
            if (!savedSpecialities.Contains(spec) && spec != ECrewmanSpecialty.GeneralBoost && spec != ECrewmanSpecialty.Count)
            {
                specialities.Add(spec);
            }
        }
        while (specialities.Count > 2)
        {
            int rand = Random.Range(0, specialities.Count);
            specialities.RemoveAt(rand);
        }

        for (int i = 0; i < specialities.Count; i++)
        {
            specialityCards[i].Setup(specialities[i], this);
        }
        specialityCards[2].Setup(ECrewmanSpecialty.GeneralBoost, this);
        SetShow(true);
    }

    public void SelectSpeciality(ECrewmanSpecialty spec)
    {
        var crewUpgrades = GameStateManager.Instance.MissionSuccessPopup.MissionRewards.CrewsUpgrades;
        var upgrade = crewUpgrades[crewIndex];
        if (spec == ECrewmanSpecialty.GeneralBoost && (upgrade.Specialties & (1 << (int)spec)) != 0)
        {
            spec++;
        }
        upgrade.Specialties |= 1 << (int)spec;
        crewUpgrades[crewIndex] = upgrade;
        SetShow(false);
    }

    public void SetupSpecImage(Image image, CrewSpecialityData specData)
    {
        if (specData != null)
        {
            image.sprite = specData.Icon;
            image.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            image.transform.parent.gameObject.SetActive(false);
        }
    }

    private void SetShow(bool show)
    {
        gameObject.SetActive(show);
        if (!show)
        {
            medalsWindow.SetContinueButtonEnabled();
            medalsWindow.UpdateVisualization();
        }
    }
}
