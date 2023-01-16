using UnityEngine;
using UnityEngine.UI;

public class CrewPanel : Panel
{
    [SerializeField]
    private CrewSubpanel crewSubpanel = null;
    [SerializeField]
    private OfficersSubPanel officersSubpanel = null;
    [SerializeField]
    private Button crewTabButton = null;
    [SerializeField]
    private Button officersTabButton = null;
    [SerializeField]
    private Text crewText = null;
    [SerializeField]
    private Text officersText = null;

    [SerializeField]
    private Sprite activeTab = null;
    [SerializeField]
    private Sprite inactiveTab = null;

    private Color crewColor;
    private Color officersColor;
    private Image crewImage;
    private Image officersImage;

    private void Start()
    {
        crewTabButton.onClick.AddListener(() => SetShowSubpanels(true));
        officersTabButton.onClick.AddListener(() => SetShowSubpanels(false));
    }

    public int GetCrewUpgrade(ECarrierType type)
    {
        return controls[0].GetUpgrade((int)type);
    }

    public int GetOfficerUpgrade(ECarrierType type)
    {
        return controls[1].GetUpgrade((int)type);
    }

    public override void Setup(NewSaveData data)
    {
        crewColor = crewText.color;
        officersColor = officersText.color;
        crewImage = crewTabButton.GetComponent<Image>();
        officersImage = officersTabButton.GetComponent<Image>();

        base.Setup(data);
        crewSubpanel.Setup(controls[0], ref data.IntermissionData.CrewData, (int)currentCarrier);
        officersSubpanel.Setup(controls[1], ref data.IntermissionData.OfficerData, (int)currentCarrier);
        SetShowSubpanels(true);
    }

    public override void Save(NewSaveData data)
    {
        crewSubpanel.Save(ref data.IntermissionData.CrewData);
        officersSubpanel.Save(ref data.IntermissionData.OfficerData);
    }

    protected override void InnerRefresh(int prevCarrier)
    {
        if (prevCarrier == -1)
        {
            controls[0].Refresh();
            controls[1].Refresh();
        }
        else
        {
            controls[0].SetData((int)currentCarrier);
            controls[1].SetData((int)currentCarrier);
            crewSubpanel.SetCarrier(currentCarrier, controls[0].GetCurrentUpgrade());
            officersSubpanel.SetCarrier(currentCarrier, controls[1].GetCurrentUpgrade());
        }
        crewSubpanel.Refresh();
        officersSubpanel.Refresh();
    }

    private void SetShowSubpanels(bool crewTab)
    {
        crewSubpanel.SetShow(crewTab);
        officersSubpanel.SetShow(!crewTab);
        crewTabButton.interactable = !crewTab;
        officersTabButton.interactable = crewTab;

        crewImage.sprite = crewTab ? activeTab : inactiveTab;
        officersImage.sprite = crewTab ? inactiveTab : activeTab;

        crewColor.a = crewTab ? 1f : 0.5f;
        crewText.color = crewColor;
        officersColor.a = crewTab ? 0.5f : 1f;
        officersText.color = officersColor;

        crewText.rectTransform.anchoredPosition = new Vector2(0f, crewTab ? 0f : -5f);
        officersText.rectTransform.anchoredPosition = new Vector2(0f, crewTab ? -5f : 0f);
    }
}
