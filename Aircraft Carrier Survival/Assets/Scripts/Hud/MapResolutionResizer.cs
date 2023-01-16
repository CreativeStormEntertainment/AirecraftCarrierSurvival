using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapResolutionResizer : MonoBehaviour
{
    public float Scale
    {
        get;
        private set;
    }
    [SerializeField]
    private RectTransform canvasRect = null;

    [SerializeField]
    private List<RectTransform> rects = null;

    private float orginalCanvasWidth = 1920f;

    private void Start()
    {
        Resize();
    }

    private void Resize()
    {
        Scale = orginalCanvasWidth / canvasRect.rect.width;
        if (canvasRect.rect.width < orginalCanvasWidth)
        {
            foreach (RectTransform rect in rects)
            {
                rect.localScale /= Scale;
                rect.anchoredPosition /= Scale;
            }
        }
    }
}
