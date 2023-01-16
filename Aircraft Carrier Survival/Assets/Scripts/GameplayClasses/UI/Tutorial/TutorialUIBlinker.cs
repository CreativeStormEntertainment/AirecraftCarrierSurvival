using UnityEngine;
using UnityEngine.UI;

public class TutorialUIBlinker : TutorialBlinker
{
    private Image image;
    private Color color;

    private void Awake()
    {
        image = GetComponent<Image>();
        color = image.color;
    }

    protected override void SetColor(float power)
    {
        color.a = power;
        image.color = color;
    }
}
