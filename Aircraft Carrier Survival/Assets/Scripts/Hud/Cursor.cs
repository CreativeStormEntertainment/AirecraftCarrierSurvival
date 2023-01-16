using System;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public static Cursor Instance;

    public event Action CursorMoved = delegate { };

    [SerializeField]
    private RectTransform canvasRect = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private float cursorPossibleOffset = 10f;

    private Vector2 savedPos;
    private float scale;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        scale = canvasRect.localScale.x;
    }

    private void Update()
    {
        rectTransform.anchoredPosition = Input.mousePosition / canvasRect.localScale.x;
        if ((rectTransform.anchoredPosition - savedPos).sqrMagnitude > cursorPossibleOffset * cursorPossibleOffset)
        {
            CursorMoved();
        }
    }

    public void SaveCursorPos()
    {
        savedPos = rectTransform.anchoredPosition;
    }
}
