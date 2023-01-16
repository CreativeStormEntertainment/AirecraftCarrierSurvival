using TMPro;
using UnityEngine;

public class SwitchFontTMP : MonoBehaviour
{
    private const string ChineseLang = "zh";
    private const string RussianLang = "ru";

    [SerializeField]
    private TMP_FontAsset chineseFontAsset = null;

    private void Awake()
    {
        if (SaveManager.Instance.PersistentData.Lang == ChineseLang || SaveManager.Instance.PersistentData.Lang == RussianLang)
        {
            GetComponent<TMP_Text>().font = chineseFontAsset;
        }
    }
}
