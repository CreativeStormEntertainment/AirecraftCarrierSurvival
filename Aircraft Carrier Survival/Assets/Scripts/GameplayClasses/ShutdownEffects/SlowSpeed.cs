using UnityEngine;

public class SlowSpeed : ShutdownEffect
{
    [SerializeField]
    private float modifier = .6f;

    protected override void OnSectionWorkingChanged(bool __)
    {
        HudManager.Instance.SetSpeedModifier(room.IsWorking ? 1f : modifier);
    }
}
