using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewSpecialityCard : MonoBehaviour
{
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private CrewManSpecialityTooltip tooltip = null;

    private ECrewmanSpecialty specialty;
    private CrewUpgradeWindow upgradeWindow;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Setup(ECrewmanSpecialty spec, CrewUpgradeWindow window)
    {
        tooltip.Speciality = spec;
        specialty = spec;
        upgradeWindow = window;
        if (CrewManager.Instance.CrewSpecialtiesDict.TryGetValue(specialty, out var specData))
        {
            icon.sprite = specData.Icon;
            title.text = LocalizationManager.Instance.GetText(specData.TitleID);
        }
    }

    private void OnClick()
    {
        upgradeWindow.SelectSpeciality(specialty);
    }
}
