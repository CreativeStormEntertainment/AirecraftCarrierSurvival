
public class VoiceSoundData
{
    public FMODEvent SelectEvent;
    public FMODEvent PositiveEvent;
    public FMODEvent NegativeEvent;

    public VoiceSoundData(VoiceEventData data)
    {
        SelectEvent = new FMODEvent(data.SelectEvent);
        PositiveEvent = new FMODEvent(data.PositiveEvent);
        NegativeEvent = new FMODEvent(data.NegativeEvent);
    }

    public void Deinit()
    {
        SelectEvent.Release();
        PositiveEvent.Release();
        NegativeEvent.Release();
    }
}
