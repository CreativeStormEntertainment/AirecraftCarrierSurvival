using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentMissionDetailsPanel : MonoBehaviour
{
    public bool KeepPanelOpen = false;
    [SerializeField]
    private float sizeConst = 252f;

    [SerializeField]
    private Text detailsText = null;

    [SerializeField]
    private RectTransform missionsRect = null;

    private RectTransform detailsTextTransform = null;
    [SerializeField]
    private RectTransform mainTransform = null;
    private void Awake()
    {
        //mainTransform = GetComponent<RectTransform>();
        detailsTextTransform = detailsText.GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void UpdateMissionDetailsText(string text)
    {
        detailsText.text = text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(detailsTextTransform);
        mainTransform.sizeDelta = new Vector2(mainTransform.sizeDelta.x, detailsTextTransform.sizeDelta.y + sizeConst);
    }

    public void Show()
    {
        missionsRect.anchoredPosition = new Vector2(-(mainTransform.sizeDelta.x + 40f), missionsRect.anchoredPosition.y);
        gameObject.SetActive(true);
    }

    public void Hide(bool forceHide = false)
    {
        if (!KeepPanelOpen || (KeepPanelOpen && forceHide))
        {
            KeepPanelOpen = false;
            missionsRect.anchoredPosition = new Vector2(0, missionsRect.anchoredPosition.y);
            gameObject.SetActive(false);
        }
    }
}
