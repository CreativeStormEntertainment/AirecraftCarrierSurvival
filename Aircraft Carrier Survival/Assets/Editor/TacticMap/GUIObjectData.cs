using UnityEngine;

public class GUIObjectData
{
    public GUIContent Content;
    public GUIStyle Style;
    public Rect Rect;

    public void Set(GUIContent content, GUIStyle style)
    {
        Content = content;
        Style = style;
    }
}
