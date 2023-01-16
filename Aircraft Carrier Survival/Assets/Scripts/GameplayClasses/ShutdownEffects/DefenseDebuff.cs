using UnityEngine;

public class DefenseDebuff : ShutdownEffect
{
    [SerializeField]
    private int defenseDebuff = -1;

    protected override void OnSectionWorkingChanged(bool __)
    {
        EnemyAttacksManager.Instance.SetSectionDefencePoints(room.IsWorking ? 0 : defenseDebuff);
    }
}
