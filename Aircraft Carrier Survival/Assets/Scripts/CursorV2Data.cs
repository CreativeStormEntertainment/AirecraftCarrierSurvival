using UnityEngine;

[System.Serializable]
public struct CursorV2Data
{
    public string Name;
    public ECursor Type;
    public Texture2D Normal;
    public Texture2D Glow;
    public Vector2 Hotspot;
}
