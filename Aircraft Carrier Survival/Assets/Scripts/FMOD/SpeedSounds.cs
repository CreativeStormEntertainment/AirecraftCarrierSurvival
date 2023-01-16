using System;

public class SpeedSounds<T1> : ParameterEventBase2<T1, ESpeed> where T1 : Enum
{
    private void Start()
    {
        TimeManager.Instance.TimeScaleChanged += OnTimeScaleChanged;
    }

    public void SetParameter(T1 parameter)
    {
        fmodEvent.SetParameter(0, parameter);
    }

    private void OnTimeScaleChanged()
    {
        if (fmodEvent != null)
        {
            var currentSpeed = TimeManager.Instance.CurrentSpeed;
            //if (currentSpeed == ESpeed.Faster)
            //{
            //    //fmodEvent.Stop();
            //}
            //else
            {
                fmodEvent.SetParameter(1, currentSpeed);
            }
        }
    }
}
