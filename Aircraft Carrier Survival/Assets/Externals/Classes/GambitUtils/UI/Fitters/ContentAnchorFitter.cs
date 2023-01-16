using UnityEngine;
using UnityEngine.EventSystems;

namespace GambitUtils.UI
{
    [ExecuteInEditMode]
    public class ContentAnchorFitter : UIBehaviour
    {
        [SerializeField]
        private RectTransform _content = default;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMin = 1f;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMax = 2f;
        [SerializeField, Tooltip("MIN anchor for MIN aspect ratio height / width")]
        private Vector2 _aspectRatioMinAnchorMin = Vector2.zero;
        [SerializeField, Tooltip("MAX anchor for MIN aspect ratio height / width")]
        private Vector2 _aspectRatioMinAnchorMax = Vector2.one;
        [SerializeField, Tooltip("MIN anchor for MAX aspect ratio height / width")]
        private Vector2 _aspectRatioMaxAnchorMin = Vector2.zero;
        [SerializeField, Tooltip("MAX anchor for MAX aspect ratio height / width")]
        private Vector2 _aspectRatioMaxAnchorMax = Vector2.one;

        protected override void OnEnable()
        {
            OnRectTransformDimensionsChange();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            //OnRectTransformDimensionsChange();
        }
#endif

        protected override void OnRectTransformDimensionsChange()
        {
            if (!EditorApplicationUtils.IsCompilingOrUpdating && enabled && _content && Screen.width > 0)
            {
                float aspectRatio = 1f * Screen.height / Screen.width;
                float anchorTransition = Mathf.InverseLerp(_aspectRatioMin, _aspectRatioMax, aspectRatio);
                Vector2 minAnchor = Vector2.Lerp(_aspectRatioMinAnchorMin, _aspectRatioMaxAnchorMin, anchorTransition);
                Vector2 maxAnchor = Vector2.Lerp(_aspectRatioMinAnchorMax, _aspectRatioMaxAnchorMax, anchorTransition);
                _content.anchorMin = minAnchor;
                _content.anchorMax = maxAnchor;
                _content.anchoredPosition = Vector2.zero;
            }
        }
    }
}
