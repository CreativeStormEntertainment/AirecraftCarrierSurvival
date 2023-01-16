using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyCrewmanSpecialty : MonoBehaviour
{
    public SlotCrewButton CurrentChosen
    {
        get;
        set;
    }

    public int FirstRandomIcon => firstRandomIcon;
    public int SecondRandomIcon => secondRandomIcon;

    [SerializeField]
    private List<Sprite> specialtyIcon = null;
    [SerializeField]
    private Image specialtyIcon1 = null;
    [SerializeField]
    private Text specialityText1 = null;
    [SerializeField]
    private Text specialityText2 = null;
    [SerializeField]
    private Image specialtyIcon2 = null;
    [SerializeField]
    private Button closeButton = null;
    [SerializeField]
    private Image portrait = null;
    [SerializeField]
    private CrewmanDescription crewmanDescription = null;

    [SerializeField]
    private GameObject clickBlocker = null;

    private int firstRandomIcon;
    private int secondRandomIcon;
    private HashSet<int> icons;

    private void Awake()
    {
        closeButton.onClick.AddListener(ClosePopup);

        icons = new HashSet<int>();
        for (int i = 0; i < specialtyIcon.Count; i++)
        {
            icons.Add(i);
        }
    }

    public void RandomizeSpecialty()
    {
        portrait.sprite = CurrentChosen.Portrait.sprite;
        crewmanDescription.FillCrewmanDescription(CurrentChosen.CrewmanIndex);

        firstRandomIcon = RandomUtils.GetRandom(icons);
        icons.Remove(firstRandomIcon);
        secondRandomIcon = RandomUtils.GetRandom(icons);
        icons.Add(firstRandomIcon);

        specialtyIcon1.sprite = specialtyIcon[firstRandomIcon];
        specialtyIcon2.sprite = specialtyIcon[secondRandomIcon];

        var locMan = LocalizationManager.Instance;
        specialityText1.text = locMan.GetText(((ECrewmanSpecialty)(firstRandomIcon + 1)).ToString() + "Upgrade");
        specialityText2.text = locMan.GetText(((ECrewmanSpecialty)(secondRandomIcon + 1)).ToString() + "Upgrade");
    }

    private void ClosePopup()
    {
        CurrentChosen.Deselect();
        gameObject.SetActive(false);
        clickBlocker.SetActive(false);

    }

    private void SetNewSpecialtyButton1()
    {
        CurrentChosen.DescriptionCrew.UnlockSpecialty(specialtyIcon1, firstRandomIcon + 1);
    }

    private void SetNewSpecialtyButton2()
    {
        CurrentChosen.DescriptionCrew.UnlockSpecialty(specialtyIcon2, secondRandomIcon + 1);
    }

}
