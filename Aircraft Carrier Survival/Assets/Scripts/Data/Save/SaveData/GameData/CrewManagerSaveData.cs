using System;
using System.Collections.Generic;

[Serializable]
public struct CrewManagerSaveData
{
    public List<int> AACooldown;
    //crew health and cd
    public List<CrewSaveData> Crew;

    public CrewManagerSaveData Duplicate()
    {
        var result = new CrewManagerSaveData();

        result.AACooldown = new List<int>(AACooldown);
        result.Crew = new List<CrewSaveData>(Crew);

        return result;
    }
}
