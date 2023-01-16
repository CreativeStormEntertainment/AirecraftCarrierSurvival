using System;
using UnityEngine;

[Serializable]
public class CrewmanDataTemporary
{
    [SerializeField]
    string Name = "";
    [SerializeField]
    [Range(1, 3)]
    private int rank = 0;
    [SerializeField]
    string Description = "";
    [SerializeField]
    private Sprite portraitImage = null;
    [SerializeField]
    private ECrewmanSpecialty skill1 = ECrewmanSpecialty.None;
    [SerializeField]
    private ECrewmanSpecialty skill2 = ECrewmanSpecialty.None;
    [SerializeField]
    [Range(0, 2)]
    private int maxSkills = 0;


    public ECrewmanSpecialty SkillType1
    {
        get
        {
            return skill1;
        }
        set
        {
            skill1 = value;
        }
    }
    public ECrewmanSpecialty SkillType2
    {
        get
        {
            return skill2;
        }
        set
        {
            skill2 = value;
        }
    }

    public Sprite GetPortrait()
    {
        return portraitImage;
    }

    public string GetName()
    {
        return Name;
    }

    public int GetRank()
    {
        return rank;
    }

    public string GetDescription()
    {
        return Description;
    }

    public int GetMaxSkills()
    {
        return maxSkills;
    }
}
