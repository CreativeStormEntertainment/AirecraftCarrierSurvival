using UnityEngine;
using UnityEngine.UI;

public class SwitchFont : MonoBehaviour
{
    protected const string ChineseLang = "zh";

    [SerializeField]
    protected Font chineseFont = null;

    protected Text text;

    protected virtual void Awake()
    {
        text = GetComponent<Text>();
        if (SaveManager.Instance.PersistentData.Lang == ChineseLang)
        {
            text.font = chineseFont;
        }
    }
}
