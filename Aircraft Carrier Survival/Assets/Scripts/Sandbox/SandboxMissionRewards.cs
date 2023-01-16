using System;

[Serializable]
public class SandboxMissionRewards
{
    public int AdmiralExp;
    public int CommandPoints;

    public SandboxMissionRewards (int exp, int commandPoints)
    {
        AdmiralExp = exp;
        CommandPoints = commandPoints;
    }

    public SandboxMissionRewards Duplicate()
    {
        var result = new SandboxMissionRewards(AdmiralExp, CommandPoints);
        return result;
    }
}
