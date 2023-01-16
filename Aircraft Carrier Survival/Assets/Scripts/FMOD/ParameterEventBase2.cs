using System;
using UnityEngine;

public class ParameterEventBase2<T1, T2> : ParameterEventBase<T1> 
    where T1 : Enum
    where T2 : Enum
{
    [SerializeField]
    private string param2Name = null;

    public void PlayEvent(T1 param1, T2 param2)
    {
#if UNITY_EDITOR
        UnityEngine.Assertions.Assert.IsFalse(Lock);
#endif
        //Debug.Log(paramValue.ToString());
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            fmodEvent.Play(param1, param2);
        }
#if LOG_SOUNDS && UNITY_EDITOR
        else
        {
            Debug.LogError("Not played");
        }
#endif
    }

    protected override bool Init()
    {
        if (base.Init())
        {
            if (fmodEvent != null)
            {
                fmodEvent.AddParameter(param2Name);
            }
            return true;
        }
        return false;
    }
}
