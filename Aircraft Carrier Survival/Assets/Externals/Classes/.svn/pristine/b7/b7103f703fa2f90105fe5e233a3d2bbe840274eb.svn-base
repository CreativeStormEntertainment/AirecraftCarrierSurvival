using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GambitUtils.UI
{
    [ExecuteInEditMode]
    public class LayoutElementFitter : UIBehaviour
    {
        [SerializeField]
        private LayoutElement _layoutElement = default;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMin = default;
        [SerializeField, Tooltip("height / width")]
        private float _aspectRatioMax = default;
        [SerializeField]
        private float _prefferedWidthMin = default;
        [SerializeField]
        private float _prefferedWidthMax = default;

        protected override void OnEnable()
        {
            OnRectTransformDimensionsChange();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            OnRectTransformDimensionsChange();
        }
#endif

        protected override void OnRectTransformDimensionsChange()
        {
            if (!EditorApplicationUtils.IsCompilingOrUpdating && enabled && _layoutElement)
            {
                float aspectRatio = 1f * Screen.height / Screen.width;
                float aspectTransition = Mathf.InverseLerp(_aspectRatioMin, _aspectRatioMax, aspectRatio);
                //Debug.Log(GetType() + "." + aspectTransition + " " + Maths.LerpInverse(_aspectRatioMin, _aspectRatioMax, aspectRatio));
                float prefferedWidth = Mathf.Lerp(_prefferedWidthMin, _prefferedWidthMax, aspectTransition);
                _layoutElement.preferredWidth = prefferedWidth;
            }
        }
    }


}
