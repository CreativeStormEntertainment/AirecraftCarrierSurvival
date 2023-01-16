using UnityEngine;
using UnityEngine.UI;

public class RewardObject : MonoBehaviour
{
    [SerializeField]
    private Text textField = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Sprite activeSprite = null;
    [SerializeField]
    private Sprite inactiveSprite = null;
    [SerializeField]
    private Color activeColor = default;
    [SerializeField]
    private Color inactiveColor = default;

    public void Setup(string text, bool active)
    {
        textField.text = text;
        if (active)
        {
            textField.color = activeColor;
            image.sprite = activeSprite;
        }
        else
        {
            textField.color = inactiveColor;
            image.sprite = inactiveSprite;
        }
    }
}
