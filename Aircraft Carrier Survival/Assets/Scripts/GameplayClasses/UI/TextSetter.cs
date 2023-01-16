using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextSetter : MonoBehaviour
{
    [SerializeField]
    protected string id;

    [SerializeField]
    protected Text text;

    [SerializeField]
    protected TMP_Text textPro;

    protected LocalizationManager locMan;

    protected virtual void Start()
    {
        Set(id);
    }

    public virtual void Set(string id)
    {
        locMan = LocalizationManager.Instance;

        if (textPro == null)
        {
            text.text = locMan.GetText(string.IsNullOrEmpty(id) ? text.text : id);
        }
        else
        {
            textPro.text = locMan.GetText(string.IsNullOrEmpty(id) ? textPro.text : id);
            if (!textPro.font.HasCharacters(textPro.text, out var list))
            {
                var text = "Missing characters:";
                foreach (char ch in list)
                {
                    text += $" \"{ch}\";";
                }
                Debug.LogError(text);
            }
        }
    }
}
