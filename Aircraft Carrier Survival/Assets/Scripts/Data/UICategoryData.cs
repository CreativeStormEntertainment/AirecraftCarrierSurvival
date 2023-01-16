using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UICategoryData
{
    public EUICategory Category;
    public List<Button> Buttons;
    public List<MonoBehaviour> Enableables;
    public List<IEnableable> EnableablesInterface;
}
