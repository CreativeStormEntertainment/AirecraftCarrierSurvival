using UnityEngine;

public class DelayDeckOrders : ShutdownEffect
{
    [SerializeField]
    private int ticksDelay = 45;

    protected override void OnSectionWorkingChanged(bool __)
    {
        AircraftCarrierDeckManager.Instance.SetOrderDelay(room.IsWorking ? 1 : ticksDelay);
    }
}
