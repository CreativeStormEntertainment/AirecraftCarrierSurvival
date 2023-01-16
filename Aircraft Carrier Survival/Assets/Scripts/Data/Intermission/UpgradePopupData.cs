using System;

public struct UpgradePopupData
{
    public int Cost;
    public bool Command;
    public Action UpgradeAction;

    public UpgradePopupData(int cost, bool command, Action upgradeAction)
    {
        Cost = cost;
        Command = command;
        UpgradeAction = upgradeAction;
    }
}
