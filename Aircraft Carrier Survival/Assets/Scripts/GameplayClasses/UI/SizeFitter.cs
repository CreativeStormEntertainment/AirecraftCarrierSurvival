using GambitUtils;
using UnityEngine;

public class SizeFitter : MonoBehaviour
{
    [SerializeField]
    private RectTransform panel = null;
    [SerializeField]
    private RectTransform sizeFitterObject = null;
    [SerializeField]
    private bool IsDamageControlFitter = false;

    private float baseHeight;

    private void Start()
    {
        baseHeight = sizeFitterObject.sizeDelta.y;
        if (IsDamageControlFitter)
        {
            DamageControlManager.Instance.OnDCChanged += FitSize;
        }
    }
    private void OnEnable()
    {
        FitSize();
    }

    public void FitSize()
    {
        if (gameObject.activeInHierarchy)
        {
            this.StartCoroutineActionAfterFrames(() => sizeFitterObject.sizeDelta = new Vector2(sizeFitterObject.sizeDelta.x, baseHeight + panel.sizeDelta.y), 1);
        }
    }
}
