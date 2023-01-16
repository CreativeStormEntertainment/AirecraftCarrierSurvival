using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewInspector : MonoBehaviour
{
    [SerializeField]
    private GameObject officerContent = null;
    [SerializeField]
    private GameObject crewContent = null;

    [SerializeField]
    private OfficerList officerList = null;
    [SerializeField]
    private ManeuversList maneuversList = null;

    [SerializeField]
    private IntermissionOfficerPortrait officerPortrait = null;
    [SerializeField]
    private CrewManeuversCard maneuver = null;
    [SerializeField]
    private Image shadow = null;

    [SerializeField]
    private Text officerName = null;
    [SerializeField]
    private Text officerDesc = null;
    [SerializeField]
    private Text maneuverName = null;
    [SerializeField]
    private Text completedMissions = null;

    [SerializeField]
    private IntermissionCrewPortrait crewPortrait = null;
    [SerializeField]
    private List<Text> specializationTexts = null;

    [SerializeField]
    private GameObject noviceText = null;
    [SerializeField]
    private GameObject noviceDescText = null;

    [SerializeField]
    private List<string> specializationIDs = null;
    [SerializeField]
    private List<string> specializationDescIDs = null;
    [SerializeField]
    private List<int> specializationParams = null;

    [SerializeField]
    private List<Image> images = null;
    [SerializeField]
    private List<Text> texts = null;

    [SerializeField]
    private float showTime = .33f;
    [SerializeField]
    private float hideTime = .75f;
    [SerializeField]
    private float delay = 5f;

    [SerializeField]
    private bool playOnPause = false;

    private float delayTimer;
    private float current;
    private float time;
    private float from;
    private float to;

    private void Awake()
    {
        Hide();
        Animate(1f);
    }

    private void Update()
    {
        if (Time.timeScale < 0.1f && !playOnPause)
        {
            return;
        }
        if (delayTimer > 0f)
        {
            delayTimer -= Time.unscaledDeltaTime;
            return;
        }
        Animate(Time.unscaledDeltaTime / time);
    }

    public void Setup(OfficerUpgrades data, int index)
    {
        Setup(true);
        officerPortrait.SetupOfficer(data, index);

        var locMan = LocalizationManager.Instance;
        var officerData = officerList.Officers[index];
        var maneuverData = maneuversList.Maneuvers[officerData.ManeuverIndex];
        officerName.text = locMan.GetText(officerData.Name);
        officerDesc.text = locMan.GetText(officerData.Description);
        maneuverName.text = locMan.GetText(maneuverData.Name);
        completedMissions.text = data.MissionsPlayed.ToString();

        switch (data.ManeuverLevel)
        {
            case 2:
                maneuver.Setup(maneuverData.Level2, data.ManeuverLevel);
                break;
            case 3:
                maneuver.Setup(maneuverData.Level3, data.ManeuverLevel);
                break;
            default:
                maneuver.Setup(maneuverData, data.ManeuverLevel);
                break;
        }

        shadow.gameObject.SetActive(data.Medals == 0);
    }

    public void Setup(CrewUpgradeSaveData data)
    {
        Setup(false);
        var locMan = LocalizationManager.Instance;
        int i = 0;
        foreach (var specialization in data.GetSpecialties())
        {
            int index = (int)specialization;
            specializationTexts[i++].text = locMan.GetText(specializationIDs[index]);
            specializationTexts[i++].text = locMan.GetText(specializationDescIDs[index], specializationParams[index].ToString());
        }

        crewPortrait.Setup(data);
        noviceText.SetActive(i == 0);
        noviceDescText.SetActive(i == 0);
        for (i = i; i < specializationTexts.Count; i++)
        {
            specializationTexts[i].gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        delayTimer = delay;
        enabled = true;
        from = 1f;
        to = 0f;
        current = 0f;
        time = hideTime;
        Animate(0f);
    }

    private void Setup(bool officer)
    {
        officerContent.SetActive(officer);
        crewContent.SetActive(!officer);

        from = 0f;
        to = 1f;
        time = showTime;
        current = 0f;
        if (enabled)
        {
            Animate(1f);
        }
        else
        {
            enabled = true;
        }
        foreach (var img in images)
        {
            img.gameObject.SetActive(true);
        }
        foreach (var text in texts)
        {
            text.gameObject.SetActive(true);
        }
    }

    private void Animate(float delta)
    {
        current += delta;
        if (current >= 1f)
        {
            current = 1f;

            enabled = false;
            delayTimer = 0f;
            if (to < 1f)
            {
                foreach (var img in images)
                {
                    img.gameObject.SetActive(false);
                }
                foreach (var text in texts)
                {
                    text.gameObject.SetActive(false);
                }
            }
        }
        float value = Mathf.Lerp(from, to, current * current);
        foreach (var img in images)
        {
            var color = img.color;
            color.a = value;
            img.color = color;
        }
        foreach (var text in texts)
        {
            var color = text.color;
            color.a = value;
            text.color = color;
        }
    }
}
