using UnityEngine;

public class PlaneSpeedSound : SpeedSounds<EPlaneSound>
{
    [SerializeField]
    private EPlaneSound plane = EPlaneSound.Buffalo;

    public void PlayEvent(ESpeed speed)
    {
        if (speed != ESpeed.Faster)
        {
            PlayEvent(plane, speed);
        }
        else
        {
            fmodEvent.Stop();
        }
    }
}
