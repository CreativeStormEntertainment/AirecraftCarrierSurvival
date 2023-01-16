using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIData : MonoBehaviour
{
    public Image Icon;
    public Sprite BaseSprite;
    public Sprite UpgradedSprite;
    public Text Text;
    public Color BaseColor;
    public Color UpgradedColor;

    public void Set(bool upgraded)
    {
        Icon.sprite = upgraded ? UpgradedSprite : BaseSprite;
        Text.color = upgraded ? UpgradedColor : BaseColor;
    }
}
