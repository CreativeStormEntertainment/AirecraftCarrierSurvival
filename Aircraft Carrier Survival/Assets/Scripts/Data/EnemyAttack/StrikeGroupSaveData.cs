using System;
using System.Collections.Generic;

[Serializable]
public struct StrikeGroupSaveData
{
    public List<EscortSaveData> Escort;
    public List<EscortActiveSkillSaveData> Skills;
    public bool Towship;

    public StrikeGroupSaveData Duplicate()
    {
        var result = this;

        result.Escort = new List<EscortSaveData>(Escort);
        result.Skills = new List<EscortActiveSkillSaveData>(Skills);

        return result;
    }
}
