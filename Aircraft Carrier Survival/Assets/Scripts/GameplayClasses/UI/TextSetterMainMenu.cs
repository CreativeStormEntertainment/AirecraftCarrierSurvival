public class TextSetterMainMenu : TextSetter
{
    protected override void Start()
    {
        base.Start();
        locMan = LocalizationManager.Instance;
        locMan.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        if (text)
        {
            text.text = locMan == null ? LocalizationManager.Instance.GetText(id) : locMan.GetText(id);
        }
    }
}