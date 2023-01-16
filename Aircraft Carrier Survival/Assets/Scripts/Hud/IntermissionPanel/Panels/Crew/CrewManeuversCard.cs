using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewManeuversCard : BaseUIManeuverCard
{
    [SerializeField]
    private Image image1 = null;
    [SerializeField]
    private Image image2 = null;
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Text attack = null;
    [SerializeField]
    private Text def = null;

    [SerializeField]
    private List<Sprite> attackSprites = null;
    [SerializeField]
    private List<Sprite> defenseSprites = null;
    [SerializeField]
    private List<Sprite> supportSprites = null;

    public override void Setup(PlayerManeuverData data, int level)
    {
        base.Setup(data, level);

        List<Sprite> sprites = attackSprites;
        switch (data.ManeuverType)
        {
            case EManeuverType.Defensive:
                sprites = defenseSprites;
                break;
            case EManeuverType.Supplementary:
                sprites = supportSprites;
                break;
        }
        image1.sprite = sprites[0];
        image2.sprite = sprites[1];

        icon.sprite = data.Icon;
        attack.text = data.Values.Attack.ToString("N0");
        def.text = data.Values.Defense.ToString("N0");
    }
}
