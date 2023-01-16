using UnityEngine;

public class MainSceneResolutionResizer : ResolutionResizer
{
    private void Awake()
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
