using System;

[Serializable]
public struct IntermissionMissionData
{
    public int UpgradePoints;
    public int CommandsPoints;
    [EscortType]
    public int EscortType;
    public int EnemyBlocksDestroyed;
    public int SquadronsToLose;
    public int HoursToFinishMission;
    public ETemporaryBuff Buff;

    public string GetDateToFinish()
    {
        int days = HoursToFinishMission / 24;
        int hours = HoursToFinishMission - days * 24;
        return days.ToString() + "d:" + hours.ToString("00") + "h:" + "00";
    }
}
