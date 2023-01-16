using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlight : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectT = null;

    public RectTransform RectT
    {
        get => rectT;
    }

    [SerializeField]
    private Image image = null;
    private float ts = 0;
    private float timeCap = 0;

    private AnimationCurve highlightCurve = null;
    private Color color = Color.white;

    public void Setup(float timeCap, AnimationCurve highlightCurve, RectTransform target, Vector3 offset, Transform root)
    {
        this.timeCap = timeCap;
        this.highlightCurve = highlightCurve;
        rectT.SetParent(target);
        rectT.anchoredPosition = offset;
        ts = Random.Range(0, timeCap);
        RefreshColor();
        gameObject.SetActive(true);
        rectT.SetParent(root);
    }

    private void LateUpdate()
    {
        ts += Time.unscaledDeltaTime;
        if (ts >= timeCap)
        {
            ts = 0f;
        }
        RefreshColor();
    }

    void RefreshColor()
    {
        color.a = highlightCurve.Evaluate(ts / timeCap);
        image.color = color;
    }
}
