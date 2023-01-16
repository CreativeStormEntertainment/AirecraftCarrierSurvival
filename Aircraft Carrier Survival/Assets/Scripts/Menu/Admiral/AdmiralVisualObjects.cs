using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AdmiralVisualObjects
{
    public EAdmiralCustomizationCategory Category;
    public GameObject CurrentOption;
    public List<GameObject> Options;
    public List<String> Names;
    public Text ButtonText;
}
