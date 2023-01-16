using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewManagerResolutionResizer : MonoBehaviour
{
    [SerializeField]
    private RectTransform canvasRect = null;

    [SerializeField]
    private List<RectTransform> rects = null;

    private float orginalImageWidth = 1920f;


    private void Start()
    {
        float scale = orginalImageWidth / canvasRect.rect.width;
        if (canvasRect.rect.width < orginalImageWidth)
        {
            foreach (RectTransform rect in rects)
            {
                rect.localScale /= scale;
                rect.anchoredPosition /= scale;
            }
        }
    }
}
