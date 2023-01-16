using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GambitUtils.UI
{
    /// <summary>
    /// Works only for CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
    /// </summary>
    [ExecuteInEditMode]
    public class CanvasScalerMatchFitter : UIBehaviour
    {
        [SerializeField]
        private CanvasScaler _canvasScaler = default;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMin = default;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMax = default;
        [SerializeField, Range(0f, 1f), Tooltip("Match for MIN aspect ratio height / width")]
        private float _aspectRatioMinMatch = 0f;
        [SerializeField, Range(0f, 1f), Tooltip("Match for MAX aspect ratio height / width")]
        private float _aspectRatioMaxMatch = 1f;

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
            if (!EditorApplicationUtils.IsCompilingOrUpdating && enabled
                && _canvasScaler && _canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                float aspectRatio = 1f * Screen.height / Screen.width;
                float anchorTransition = Mathf.InverseLerp(_aspectRatioMin, _aspectRatioMax, aspectRatio);
                float match = Mathf.Lerp(_aspectRatioMinMatch, _aspectRatioMaxMatch, anchorTransition);
                _canvasScaler.matchWidthOrHeight = match;
            }
        }
    }
}
