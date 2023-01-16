using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpriteLocalizationData
{
    [SerializeField]
    private List<Sprite> sprites = null;

    private string lang;

    public Sprite GetSprite()
    {
        lang = SaveManager.Instance.PersistentData.Lang;
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i].name.EndsWith(lang))
            {
                return sprites[i];
            }
        }
        return sprites[0];
    }
}
