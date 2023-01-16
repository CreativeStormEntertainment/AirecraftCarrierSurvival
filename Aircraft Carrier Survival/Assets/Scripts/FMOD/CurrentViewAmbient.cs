
public class CurrentViewAmbient : ParameterEventBase<ECurrentView>
{
    private FMODParameter distanceToSeaParam;

    public void SetDistanceToSeaParameter(float value)
    {
        distanceToSeaParam.SetValue(fmodEvent, value);
    }

    public override void PlayEvent(ECurrentView paramValue)
    {
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            fmodEvent.PlayWithOneEnum(paramValue);
        }
#if LOG_SOUNDS && UNITY_EDITOR
        else
        {
            UnityEngine.Debug.LogError("Not played");
        }
#endif
    }

    protected override bool Init()
    {
        if (base.Init())
        {
            if (fmodEvent != null)
            {
                distanceToSeaParam = fmodEvent.AddParameter("DistanceToSea_Param");
            }
            return true;
        }
        return false;
    }
}
