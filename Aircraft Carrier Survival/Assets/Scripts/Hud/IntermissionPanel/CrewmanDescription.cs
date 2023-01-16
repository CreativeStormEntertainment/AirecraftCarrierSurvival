using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewmanDescription : MonoBehaviour
{
    public static CrewmanDescription Instance;

    public List<Sprite> SkillIcons => skillsIcons;

    public int CrewmanIndex
    {
        get;
        private set;
    }

    [SerializeField]
    protected GameObject skill1Frame = null;

    [SerializeField]
    protected Image skill1Icon = null;

    [SerializeField]
    protected GameObject skill2Frame = null;

    [SerializeField]
    protected Image skill2Icon = null;

    [SerializeField]
    protected List<Sprite> skillsIcons = new List<Sprite>();

    private void Awake()
    {
        Instance = this;
    }

    public void FillCrewmanDescription(int crewmanIndex)
    {
        //CrewmanIndex = crewmanIndex;
        //var specialty = IntermissionPanel.Instance.Crew.CurrentCrew[CrewmanIndex].Specialty;
        //if (specialty == ECrewmanSpecialty.None)
        //{
        //    skill1Frame.gameObject.SetActive(false);
        //}
        //else
        //{
        //    skill1Frame.gameObject.SetActive(true);
        //    skill1Icon.sprite = skillsIcons[(int)specialty - 1];
        //}
    }

    public void UnlockSpecialty(Image specialtyIcon, int specialtyIndex)
    {
        //skill1Frame.SetActive(true);
        //skill1Icon.sprite = specialtyIcon.sprite;

        //var intermission = IntermissionPanel.Instance;
        ////intermission.Crew.CurrentCrew[CrewmanIndex].Specialty = (ECrewmanSpecialty)specialtyIndex;
        //intermission.ReduceCommandPoints(2);
    }

}
