using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button Button => button;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private GameObject hover = null;
    [SerializeField]
    private GameObject selectImage = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hover.SetActive(false);
    }

    public void SetSelected(bool select)
    {
        selectImage.SetActive(select);
    }

    public void SetHighlighted(bool highlighted)
    {
        highlight.SetActive(highlighted);
    }

    public void SetShow(bool show)
    {
        gameObject.SetActive(show);
    }
}
