using UnityEngine;

public class PlaneSounds : ParameterEventBase<EPlaneSound>
{
    [SerializeField]
    private EPlaneSound plane = EPlaneSound.Buffalo;

    public void PlayEvent()
    {
        PlayEvent(plane);
    }
}
