using FMODUnity;
using System;
using UnityEngine;

public class ParameterEventBase<T> : MonoBehaviour where T : Enum
{
#if UNITY_EDITOR
    public bool Lock
    {
        get;
        set;
    }
#endif

    [SerializeField, EventRef]
    protected string eventName = null;

    protected FMODEvent fmodEvent = null;

    [SerializeField]
    private string paramName = null;
    [SerializeField]
    private bool sound3D = false;

    private bool init;

    protected virtual void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        fmodEvent?.Release();
    }

    public virtual void PlayEvent(T paramValue)
    {
#if UNITY_EDITOR
        //UnityEngine.Assertions.Assert.IsFalse(Lock);
#endif
        //Debug.Log(paramValue.ToString());
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            fmodEvent.Play(paramValue);
        }
#if LOG_SOUNDS && UNITY_EDITOR
        else
        {
            Debug.LogError("Not played");
        }
#endif
    }

    public void SetPause(bool pause)
    {
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            fmodEvent.SetPause(pause);
        }
    }

    public void Stop(bool allowFadeout)
    {
        Init();
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            fmodEvent.Stop(allowFadeout);
        }
    }

    protected virtual bool Init()
    {
        if (init)
        {
            return false;
        }
        init = true;
        if (string.IsNullOrWhiteSpace(eventName))
        {
            Debug.LogError("Specify event name for " + typeof(T).Name + ", in script " + GetType().Name);
        }
        else
        {
            fmodEvent = FMODEvent.CreateWithParameter<T>(eventName, paramName, sound3D ? transform : null);
        }
        return true;
    }
}
