using System;
using System.Collections.Generic;

[Serializable]
public class OfficerSetup
{
    public string Name;
    public string Title;
    public string Description;
    public List<OfficerSkill> OfficerSkills;
    //public GameObject CustomModel;
    public int PortraitNumber;
    public int ModelNumber;
    public EVoiceType Voice;

    [NonSerialized]
    public bool HasAir;
    [NonSerialized]
    public bool HasNavy;

    public int ManeuverIndex = -1;
    public int ManeuverLevel = 1;

    public int Cost;

    public int BaseAirLvl => airSkill != null ? airSkill.BaseLevel : 0;
    public int AirLvl => airSkill != null ? airSkill.CurrentLevel : 0;

    public int BaseNavyLvl => navySkill != null ? navySkill.BaseLevel : 0;
    public int NavyLvl => navySkill != null ? navySkill.CurrentLevel : 0;

    private OfficerSkill airSkill = null;
    private OfficerSkill navySkill = null;

    public void Init()
    {
        foreach (OfficerSkill skill in OfficerSkills)
        {
            switch (skill.SkillEnum)
            {
                case EOfficerSkills.CommandingAirForce:
                    airSkill = skill;
                    HasAir = true;
                    break;
                case EOfficerSkills.CommandingNavy:
                    navySkill = skill;
                    HasNavy = true;
                    break;
            }
        }
    }
}
