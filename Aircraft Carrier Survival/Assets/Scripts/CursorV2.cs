using System.Collections.Generic;
using UnityEngine;

public class CursorV2 : MonoBehaviour
{
    public static CursorV2 Instance = null;

    [SerializeField]
    private List<CursorV2Data> setupData = new List<CursorV2Data>();

    private ECursor current = ECursor.Default;
    private bool isGlowing = false;

    private Dictionary<ECursor, CursorV2Data> cursors;
    private bool isInitialized = false;

    private void Awake()
    {
        Instance = this;
        SetCursor(ECursor.Default, false);
    }

    private void LateUpdate()
    {
        if (!UnityEngine.Cursor.visible)
        {
            UnityEngine.Cursor.visible = true;
        }
    }

    public void SetCursor(ECursor mode, bool isGlowing)
    {
        if (!isInitialized)
        {
            isInitialized = true;
            cursors = new Dictionary<ECursor, CursorV2Data>();
            for (int i = 0; i < setupData.Count; ++i)
            {
                cursors.Add(setupData[i].Type, setupData[i]);
            }
        }
        current = mode;
        if (cursors.TryGetValue(mode, out CursorV2Data data))
        {
            this.isGlowing = isGlowing;
            UnityEngine.Cursor.SetCursor(isGlowing ? data.Glow : data.Normal, data.Hotspot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("Cursor: " + mode.ToString() + " not assigned!");
        }
    }

    public void SetCursor(bool isGlowing)
    {
        this.isGlowing = isGlowing;
        UnityEngine.Cursor.SetCursor(isGlowing ? cursors[current].Glow : cursors[current].Normal, cursors[current].Hotspot, CursorMode.Auto);
    }

    public void SetCursor(ECursor mode)
    {
        SetCursor(mode, isGlowing);
    }
}
