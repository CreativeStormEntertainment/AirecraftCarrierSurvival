using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetLanguage : MonoBehaviour
{
    public List<ChooseLanguageButtons> LanguageButtons => languageButtons;

    [SerializeField]
    private List<ChooseLanguageButtons> languageButtons = null;
    public void ChooseLanguage(string value)
    {
        LocalizationManager.Instance.SetLanguage(value);
    }

}
