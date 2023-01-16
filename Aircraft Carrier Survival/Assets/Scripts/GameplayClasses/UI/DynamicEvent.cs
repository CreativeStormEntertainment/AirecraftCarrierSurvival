using UnityEngine;
using UnityEngine.UI;

public class DynamicEvent : MonoBehaviour
{
    public Button Button;
    public RectTransform RectTransform;
    public Text Text;

    public string TextID;

    public Image Icon;
    public Sprite Sprite;

    public Animator Animator;

    public void SetBorderGlow(bool value)
    {
        Animator.SetBool("BorderGlowOn", value);
    }
}
