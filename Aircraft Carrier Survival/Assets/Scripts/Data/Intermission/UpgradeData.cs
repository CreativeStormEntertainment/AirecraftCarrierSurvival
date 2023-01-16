using System;
using UnityEngine;

[Serializable]
public class UpgradeData
{
    [SerializeField] private Sprite ImageIcon = null;

    public Sprite GetImageIcon()
    {
        return ImageIcon;
    }
}
