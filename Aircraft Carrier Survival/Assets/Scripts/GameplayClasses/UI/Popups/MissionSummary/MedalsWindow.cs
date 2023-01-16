using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MedalsWindow : MonoBehaviour
{
    public StudioEventEmitter LevelUpSound => levelUpSound;
    public StudioEventEmitter MedalSound => medalSound;
    public CrewInspector CrewInspector => crewInspector;

    [SerializeField]
    private List<DraggableMedal> medals = null;
    [SerializeField]
    private List<OfficerMedalSlot> officerSlots = null;
    [SerializeField]
    private List<CrewMedalSlot> crewSlots = null;
    [SerializeField]
    private OfficerUpgradeWindow officerUpgradeWindow = null;
    [SerializeField]
    private CrewUpgradeWindow crewUpgradeWindow = null;
    [SerializeField]
    private Button continueButton = null;
    [SerializeField]
    private Button closeWindowButton = null;
    [SerializeField]
    private StudioEventEmitter levelUpSound = null;
    [SerializeField]
    private StudioEventEmitter medalSound = null;
    [SerializeField]
    private CrewInspector crewInspector = null;

    private int medalsCount;
    private bool allOptional;

    private void Start()
    {
        continueButton.onClick.AddListener(Continue);
        closeWindowButton.onClick.AddListener(Hide);
        foreach (var medal in medals)
        {
            medal.MedalAssigned += OnMedalAssigned;
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            foreach (var medal in medals)
            {
                if (medal.Drag)
                {
                    medal.OnEndDrag(null);
                }
            }
        }
    }

    public void Setup(int medalsCount, bool allOptional)
    {
        this.medalsCount = medalsCount;
        this.allOptional = allOptional;

        closeWindowButton.interactable = false;
        officerUpgradeWindow.Init(this);
        crewUpgradeWindow.Init(this);
        foreach (var medal in medals)
        {
            medal.SetShow(false);
        }
        for (int i = 0; i < medalsCount; i++)
        {
            medals[i].SetShow(true);
        }
        var crews = GetCrewsIndices();
        for (int i = 0; i < crewSlots.Count; i++)
        {
            crewSlots[i].Setup(crews[i], crewUpgradeWindow, this);
        }
        var officers = GetLevelableOfficersIndices();
        for (int i = 0; i < officerSlots.Count; i++)
        {
            officerSlots[i].SetupOfficer(officers[i], officerUpgradeWindow, this);
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        continueButton.interactable = true;
        gameObject.SetActive(false);
        GameStateManager.Instance.MissionSuccessPopup.MainPanel.SetActive(true);
    }

    public void SetContinueButtonEnabled()
    {
        if (officerUpgradeWindow.gameObject.activeInHierarchy || crewUpgradeWindow.gameObject.activeInHierarchy)
        {
            return;
        }
        foreach (var medal in medals)
        {
            if (medal.gameObject.activeInHierarchy)
            {
                return;
            }
        }
        continueButton.enabled = true;
        closeWindowButton.interactable = true;
    }

    public void Continue()
    {
        continueButton.interactable = false;
        var stateMan = GameStateManager.Instance;
        stateMan.MissionSuccessPopup.SaveRewards();
        stateMan.MissionSuccessPopup.Back();
    }

    public void UpdateVisualization()
    {
        foreach (var officer in officerSlots)
        {
            officer.UpdateLevelVisualization();
        }
        foreach (var crew in crewSlots)
        {
            crew.UpdateLevelVisualization();
        }
    }

    private List<int> GetLevelableOfficersIndices()
    {
        var saveMan = SaveManager.Instance;
        var selectedOfficersIndices = saveMan.Data.IntermissionData.OfficerData.Selected;
        List<int> levelableOfficers = new List<int>();
        List<int> selectedOfficersLeft = new List<int>();
        var upgradesList = saveMan.Data.MissionRewards.OfficersUpgrades;
        for (int i = 0; i < selectedOfficersIndices.Count; i++)
        {
            if (selectedOfficersIndices[i] >= 0)
            {

                var list = upgradesList[selectedOfficersIndices[i]].Medals < 6 ? levelableOfficers : selectedOfficersLeft;
                list.Add(selectedOfficersIndices[i]);
            }
        }
        while (levelableOfficers.Count > 2)
        {
            levelableOfficers.RemoveAt(Random.Range(0, levelableOfficers.Count));
        }
        while (levelableOfficers.Count < 2)
        {
            int index = Random.Range(0, selectedOfficersLeft.Count);
            levelableOfficers.Add(selectedOfficersLeft[index]);
            selectedOfficersLeft.RemoveAt(index);
        }
        return levelableOfficers;
    }

    private List<int> GetCrewsIndices()
    {
        var saveMan = SaveManager.Instance;
        var selectedCrewsIndices = saveMan.Data.IntermissionData.CrewData.Selected;
        List<int> levelableCrews = new List<int>();
        List<int> otherCrews = new List<int>();
        otherCrews.RemoveAll(item => item == -1);
        foreach (var crew in selectedCrewsIndices)
        {
            if (crew != -1)
            {
                if (saveMan.Data.MissionRewards.CrewsUpgrades[crew].Medals < 2)
                {
                    levelableCrews.Add(crew);
                }
                otherCrews.Add(crew);
            }
        }
        while (levelableCrews.Count > 2)
        {
            levelableCrews.RemoveAt(Random.Range(0, levelableCrews.Count));
        }
        foreach (var crew in levelableCrews)
        {
            otherCrews.Remove(crew);
        }
        while (levelableCrews.Count < 3)
        {
            int index = Random.Range(0, otherCrews.Count);
            levelableCrews.Add(otherCrews[index]);
            otherCrews.RemoveAt(index);
        }
        return levelableCrews;
    }

    private void OnMedalAssigned()
    {
        SetContinueButtonEnabled();
        UpdateVisualization();
    }
}
