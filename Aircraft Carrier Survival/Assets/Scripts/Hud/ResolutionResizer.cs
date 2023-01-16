using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionResizer : MonoBehaviour
{
    public float Scale
    {
        get;
        protected set;
    }

    [SerializeField]
    protected RectTransform canvasRect = null;

    [SerializeField]
    protected List<RectTransform> rects = null;

    protected float orginalCanvasWidth = 1920f;
}
