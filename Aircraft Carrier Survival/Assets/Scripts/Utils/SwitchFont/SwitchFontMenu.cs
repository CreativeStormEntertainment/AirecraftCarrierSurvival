using UnityEngine;
using UnityEngine.UI;

public class SwitchFontMenu : SwitchFont
{
    private Font baseFont;

    protected override void Awake()
    {
        baseFont = GetComponent<Text>().font;
        base.Awake();

        LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        text.font = SaveManager.Instance.PersistentData.Lang == ChineseLang ? chineseFont : baseFont;
    }
}
