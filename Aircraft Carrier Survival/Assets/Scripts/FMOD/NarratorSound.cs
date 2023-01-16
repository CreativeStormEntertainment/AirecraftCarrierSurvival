
public class NarratorSound : ParameterEventBase<EVoiceOver>
{
    protected override void Awake()
    {
        LocalizeEvent(SaveManager.Instance.PersistentData.Lang);
        if (!FMODEvent.IsValid(eventName))
        {
            LocalizeEvent(LocalizationManager.FallbackLang);
        }

        base.Awake();
    }

    private void LocalizeEvent(string lang)
    {
        eventName = eventName.Substring(0, eventName.Length - 2) + lang;
    }
}
