using GambitUtils;
using UnityEngine;

#if !FMOD_DISABLED
public class FMODRandomizedEvent : FMODEvent
{
    private const string IntSelectorEventParameter = "IntSelector";

    private readonly ListRandomizer<int> listRandomizer = null;

    public static FMODRandomizedEvent Create(string eventName, int valuesCount, Transform transform = null)
    {
        FMODRandomizedEvent eventInstance = new FMODRandomizedEvent(eventName, valuesCount, transform);
        eventInstance.AddParameter(IntSelectorEventParameter);
        return eventInstance;
    }

    public FMODRandomizedEvent(string eventName, int valuesCount, Transform transform = null) : base(eventName, transform)
    {
        listRandomizer = ListRandomizer<int>.CreateIntRandomizer(valuesCount);
    }

    public void PlayRandom()
    {
        Play(IntSelectorEventParameter, listRandomizer.PickRandomElement());
    }
}
#endif