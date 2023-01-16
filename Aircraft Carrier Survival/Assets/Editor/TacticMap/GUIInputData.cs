using System;
using UnityEngine;

public class GUIInputData
{
    public GUIStyle Style;
    public string Text;
    public Action<GUIObjectData> Gui;
    public Action Effect;

    public GUIInputData(string text, Action effect)
    {
        Text = text;
        Effect = effect;
    }

    public GUIInputData(GUIStyle style, string text, Action<GUIObjectData> gui)
    {
        Style = style;
        Text = text;
        Gui = gui;
    }
}
