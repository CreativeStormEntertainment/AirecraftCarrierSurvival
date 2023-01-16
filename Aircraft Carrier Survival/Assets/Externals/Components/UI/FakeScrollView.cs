using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GambitUtils.UI
{
    public class FakeScrollView : MonoBehaviour
    {
        public bool LayoutInitialized => contentWidth > 0;
        private int ButtonRowsCount => buttonRows.Count;
        private RectTransform FirstRow => buttonRows.Count > 0 ? buttonRows[0] : null;

        [SerializeField]
        private ScrollRect scrollRect = null;
        [SerializeField]
        private VerticalLayoutGroup verticalLayout = null;
        [SerializeField]
        private List<RectTransform> buttonRows = new List<RectTransform>();
        [SerializeField]
        private RectTransform topFillElement = null;
        [SerializeField]
        private RectTransform bottomFillElement = null;

        private RectTransform content = null;

        private readonly List<List<IFakeScrollButton>> buttonsInRows = new List<List<IFakeScrollButton>>();

        private readonly List<object> contentList = new List<object>();

        private bool awaked = false;

        private int colsCount = 0;
        private int totalRowsCount = 0;

        private float normalizedCellHeight = 1f;

        private float contentHeight = 0f;
        private float contentWidth = 0f;
        private float rowHeight = 0f;

        private Vector2 spacing = default;

        private int firstVisibleRow = 0;
        private int contentRowsCount = 0;

        private bool buttonIsRow = false;

        private void Awake()
        {
            content = scrollRect.content;
            awaked = true;
        }

        protected void OnEnable()
        {
            content = scrollRect.content;
            if (awaked && contentWidth <= 0f)
            {
                this.StartCoroutineActionAfterPredicate(SetContentSize, () => content.rect.width <= 0f);
            }
        }

        public bool Initialize<T>(out List<T> buttons) where T : MonoBehaviour, IFakeScrollButton
        {
            if (buttonsInRows.Count > 0)
            {
                buttons = null;
                return false;
            }
            buttons = new List<T>();
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

            buttonIsRow = FirstRow.GetComponent<IFakeScrollButton>() != null;
            if (buttonIsRow)
            {
                foreach (var buttonRow in buttonRows)
                {
                    var rowButtons = new List<IFakeScrollButton>();
                    var fakeButton = buttonRow.GetComponent<IFakeScrollButton>();
                    rowButtons.Add(fakeButton);
                    buttons.Add(fakeButton as T);
                    buttonsInRows.Add(rowButtons);
                }
            }
            else
            {
                foreach (var buttonRow in buttonRows)
                {
                    var rowButtons = new List<IFakeScrollButton>();
                    foreach (Transform child in buttonRow.transform)
                    {
                        var fakeButton = child.GetComponent<IFakeScrollButton>();
                        rowButtons.Add(fakeButton);
                        buttons.Add(fakeButton as T);
                    }
                    buttonsInRows.Add(rowButtons);
                }
            }
            ResetGridSize();
            return true;
        }

        public void SetContent<T>(IEnumerable<T> list, bool refresh = true)
        {
            contentList.Clear();
            foreach (var item in list)
            {
                contentList.Add(item);
            }
            if (refresh)
            {
                RefreshContent();
            }
        }

        public void AddContent(object content, bool refresh)
        {
            contentList.Add(content);
            if (refresh)
            {
                RefreshContent();
            }
        }

        public void ClearButtonRow(IFakeScrollButton fakeButton)
        {
            if (buttonIsRow)
            {
                fakeButton.ClearFakeScrollButtonContent();
                int rowIndex = buttonsInRows.FindIndex(row => row[0] == fakeButton);
                if (rowIndex >= 0)
                {
                    var row = buttonsInRows[rowIndex];
                    buttonsInRows.RemoveAt(rowIndex);
                    buttonsInRows.Add(row);
                }
                RebuildLayout();
            }
        }

        public void ResetButtonRows()
        {
            ResetGridSize();

            scrollRect.verticalNormalizedPosition = 1f;

            for (int i = 0; i < ButtonRowsCount; i++)
            {
                SetupButtonRow(i, 0);
            }

            int activeRowsCount = Mathf.Min(ButtonRowsCount, contentRowsCount);
            for (int i = 0; i < ButtonRowsCount; i++)
            {
                SetButtonRowActive(i, i < activeRowsCount);
            }
        }

        private void RefreshContent()
        {
            ResetButtonRows();
            SetContentSize();
        }

        private void SetContentSize()
        {
            if (!content || !verticalLayout || content.rect.width <= 0f)
            {
                Debug.LogError(GetType() + ".SetContentSize: " + (content ? content.rect.ToString() : "") + " verticalLayout: " + verticalLayout);
                return;
            }
            var viewport = scrollRect.viewport;
            var padding = verticalLayout.padding;
            spacing.y = verticalLayout.spacing;
            //spacing.x = buttonRows[0].spacing;

            rowHeight = FirstRow.rect.height;
            float totalRowsHeight = totalRowsCount * rowHeight + (totalRowsCount - 1) * spacing.y;
            contentHeight = padding.top + padding.bottom + totalRowsHeight + spacing.y;
            normalizedCellHeight = (rowHeight + spacing.y) / (contentHeight - viewport.rect.height);
            contentWidth = content.rect.width;
            int bottomEmptyRowsCount = Mathf.Max(0, totalRowsCount - ButtonRowsCount);
            bottomFillElement.sizeDelta = new Vector2(contentWidth, bottomEmptyRowsCount * (rowHeight + spacing.y));
            topFillElement.sizeDelta = new Vector2(contentWidth, 0f);

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private void OnScrollRectValueChanged(Vector2 position)
        {
            int begRow = (int)((1f - position.y) / normalizedCellHeight);
            begRow = Mathf.Clamp(begRow - 1, 0, Mathf.Max(0, totalRowsCount - ButtonRowsCount));
            if (begRow != firstVisibleRow)
            {
                int topEmptyRowsCount = begRow;
                float topFillHeight = topEmptyRowsCount * (rowHeight + spacing.y);
                topFillElement.sizeDelta = new Vector2(contentWidth, topFillHeight);

                int bottomEmptyRowsCount = Mathf.Max(totalRowsCount - Mathf.Min(begRow + ButtonRowsCount, totalRowsCount), 0);
                float bottomFillHeight = Mathf.Max(bottomEmptyRowsCount * (rowHeight + spacing.y), 0f);
                bottomFillElement.sizeDelta = new Vector2(contentWidth, bottomFillHeight);

                ShiftButtonRows(begRow, firstVisibleRow);
            }
            firstVisibleRow = begRow;
        }

        private void ResetGridSize()
        {
            colsCount = buttonIsRow ? 1 : FirstRow.childCount;
            contentRowsCount = (contentList.Count + colsCount - 1) / colsCount;
#if DEBUG_ROWS_COUNT
            totalRowsCount = 15;
#else
            totalRowsCount = contentRowsCount;
#endif
        }

        private void ShiftButtonRows(int begRow, int begRowPrev)
        {
            int deltaRows = begRow - begRowPrev;
            int absDeltaRows = Mathf.Abs(deltaRows);

            if (absDeltaRows >= ButtonRowsCount)
            {
                for (int i = 0; i < ButtonRowsCount; i++)
                {
                    SetupButtonRow(i, begRow);
                }
            }
            else if (absDeltaRows > 0)
            {
                if (deltaRows > 0)
                {
                    for (int i = 0; i < deltaRows; i++)
                    {
                        var buttonsInRow = buttonsInRows[0];
                        buttonsInRows.RemoveAt(0);
                        buttonsInRows.Add(buttonsInRow);
                    }
                    int begRowIndex = ButtonRowsCount - deltaRows;
                    for (int i = begRowIndex; i < ButtonRowsCount; i++)
                    {
                        SetupButtonRow(i, begRow);
                    }
                }
                else if (deltaRows < 0)
                {
                    int endRowIndex = ButtonRowsCount - 1;
                    for (int i = 0; i < absDeltaRows; i++)
                    {
                        var buttonsInRow = buttonsInRows[endRowIndex];
                        buttonsInRows.RemoveAt(endRowIndex);
                        buttonsInRows.Insert(0, buttonsInRow);
                    }
                    int begRowIndex = 0;
                    for (int i = begRowIndex; i < absDeltaRows; i++)
                    {
                        SetupButtonRow(i, begRow);
                    }
                }
            }
            RebuildLayout();
        }

        private void RebuildLayout()
        {
            topFillElement.transform.SetSiblingIndex(0);
            if (buttonIsRow)
            {
                for (int i = 0; i < ButtonRowsCount; i++)
                {
                    buttonsInRows[i][0].FakeScrollButtonTransform.SetSiblingIndex(i + 1);
                }
            }
            else
            {
                for (int i = 0; i < ButtonRowsCount; i++)
                {
                    buttonsInRows[i][0].FakeScrollButtonTransform.parent.SetSiblingIndex(i + 1);
                }
            }
            bottomFillElement.transform.SetSiblingIndex(content.transform.childCount - 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private Transform GetButtonRowTransform(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < buttonsInRows.Count && buttonsInRows[rowIndex].Count > 0)
            {
                return buttonIsRow ? buttonsInRows[rowIndex][0].FakeScrollButtonTransform : buttonsInRows[rowIndex][0].FakeScrollButtonTransform.parent;
            }
            return null;
        }

        private void SetButtonRowActive(int rowIndex, bool active)
        {
            GetButtonRowTransform(rowIndex).SetGameObjectActive(active);
        }

        private void SetupButtonRow(int rowIndex, int rowOffset)
        {
            if (rowIndex < 0 || rowIndex >= buttonsInRows.Count)
            {
                return;
            }
            var buttonsInRow = buttonsInRows[rowIndex];
            for (int i = 0; i < buttonsInRow.Count; i++)
            {
                var button = buttonsInRow[i];
                button.ClearFakeScrollButtonContent();
            }
            int contentRow = rowOffset + rowIndex;
            int contentOffset = contentRow * colsCount;
            if (contentRow < 0 || contentRow >= contentRowsCount)
            {
                return;
            }
            int buttonCount = contentRow < contentRowsCount - 1 ? buttonsInRow.Count : (contentList.Count - contentOffset);
            for (int i = 0; i < buttonCount; i++)
            {
                var buttonContent = contentList[contentOffset + i];
                //Debug.Log(GetType() + ".SetupButtonRow: " + buttonContent + " " + i);
                var button = buttonsInRow[i];
                button.SetupFakeScrollButtonContent(buttonContent);
            }
        }
    }
}