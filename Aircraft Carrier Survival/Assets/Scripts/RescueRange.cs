using UnityEngine;

public class RescueRange : MonoBehaviour
{
    [SerializeField]
    private RectTransform followTarget = null;
    [SerializeField]
    private RectTransform trans = null;
    [SerializeField]
    private RectTransform icon = null;

    private bool up;

    private void Update()
    {
        trans.anchoredPosition = followTarget.anchoredPosition;
        if ((trans.anchoredPosition.y < 0f) != up)
        {
            up = !up;
            var vec = icon.anchorMin;
            vec.y = up ? 1f : 0f;
            icon.anchorMin = icon.anchorMax = vec;
            icon.anchoredPosition = Vector2.zero;
        }
    }
}
