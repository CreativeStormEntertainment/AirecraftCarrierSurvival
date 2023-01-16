using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GambitUtils.UI
{
    [ExecuteInEditMode]
    public class ContentGridLayoutGroupFitter : UIBehaviour
    {
        [SerializeField]
        private RectTransform _content = default;
        [SerializeField]
        private GridLayoutGroup _gridLayout = default;
        [SerializeField, Range(0f, 1f)]
        private float _spacingNormalizedX = default;
        [SerializeField, Range(0f, 1f)]
        private float _spacingNormalizedY = default;
        [SerializeField, Range(0f, 1f)]
        private float _paddingNormalizedLeft = default;
        [SerializeField, Range(0f, 1f)]
        private float _paddingNormalizedRight = default;
        [SerializeField, Range(0f, 1f)]
        private float _paddingNormalizedTop = default;
        [SerializeField, Range(0f, 1f)]
        private float _paddingNormalizedBottom = default;
        [SerializeField, Range(0f, 5f), Tooltip("height / width")]
        private float _cellSizeAspectRatio = 1f;

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
            if (!EditorApplicationUtils.IsCompilingOrUpdating && enabled && _gridLayout)
            {
                RectTransform gridRectTransform = _gridLayout.transform as RectTransform;
                switch (_gridLayout.constraint)
                {
                    case GridLayoutGroup.Constraint.Flexible:
                        throw new NotImplementedException();
                    case GridLayoutGroup.Constraint.FixedColumnCount:
                        int colCount = _gridLayout.constraintCount;
                        int rowCount = _gridLayout.transform.childCount / colCount;
                        float cellWidth = _content.rect.width / (colCount + _paddingNormalizedLeft + _paddingNormalizedRight + _spacingNormalizedX * (colCount - 1));
                        float cellHeight = cellWidth * _cellSizeAspectRatio;
                        float gridHeightTrimmed = cellHeight * rowCount;
                        _gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
                        _gridLayout.spacing = new Vector2(_spacingNormalizedX * cellWidth, _spacingNormalizedY * cellHeight);
                        _gridLayout.padding.left = (int)(_paddingNormalizedLeft * cellWidth);
                        _gridLayout.padding.right = (int)(_paddingNormalizedRight * cellWidth);
                        _gridLayout.padding.top = (int)(_paddingNormalizedTop * cellHeight);
                        _gridLayout.padding.bottom = (int)(_paddingNormalizedBottom * cellHeight);
                        float gridHeight = rowCount * cellHeight + _gridLayout.padding.top + _gridLayout.padding.bottom + _gridLayout.spacing.y * (rowCount - 1);
                        gridRectTransform.sizeDelta = new Vector2(gridRectTransform.sizeDelta.x, gridHeight);
                        break;
                    case GridLayoutGroup.Constraint.FixedRowCount:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}
