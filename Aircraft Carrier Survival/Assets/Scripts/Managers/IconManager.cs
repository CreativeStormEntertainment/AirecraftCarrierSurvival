using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.U2D; // need to use SpriteAtlas

public class IconManager : MonoBehaviour
{
    public static IconManager Instance;

    public SpriteAtlas PortraitsAtlas;

    public List<Sprite> BadgeIcons;
    public List<Sprite> SectionCategoryIcons;

    public Dictionary<ETraits, Sprite> BadgeIconsDict;
    public Dictionary<ESectionCategory, Sprite> SectionCategoryIconsDict;
    public Sprite NoSectionCategoryIcon;
    public Sprite DCJobIcon;

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        BadgeIconsDict = new Dictionary<ETraits, Sprite>();
        SectionCategoryIconsDict = new Dictionary<ESectionCategory, Sprite>();
        int start = (int)ETraits.None + 1;
        Assert.IsTrue(BadgeIcons.Count == ((int)ETraits.SpecialistEnd - start));
        Assert.IsTrue(BadgeIcons.Count == SectionCategoryIcons.Count);
        for (int i = 0; i < BadgeIcons.Count; i++)
        {
            BadgeIconsDict[(ETraits)(start + i)] = BadgeIcons[i];
            SectionCategoryIconsDict[(ESectionCategory)(start + i)] = SectionCategoryIcons[i];
        }
    }
}
