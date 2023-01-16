using UnityEngine;
using UnityEngine.UI;

public class ResourceIndicator : MonoBehaviour
{
    private float maxX;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        maxX = (transform.parent as RectTransform).rect.width;
    }

    public void SetIndicator(float progress)
    {
        //Debug.LogFormat(this, "progress {0}", progress);
        rectTransform.anchoredPosition = new Vector2(progress * maxX, 0f);
    }
}
