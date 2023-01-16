using UnityEngine;

public class IntermissionMapResolutionResizer : ResolutionResizer
{
    private void Awake()
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
