using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectorButton : MonoBehaviour
{
    public event Action<SectorButton> Clicked = delegate { };

    public RectTransform RectTransform => rectTransform;
    public ESectorType SectorType => sectorType;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private ESectorType sectorType = default;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private GameObject select = null;

    private MainGoalData mainGoalData;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Setup(MainGoalData data)
    {
        mainGoalData = data;
        gameObject.SetActive(true);
    }

    public void SetSelected(bool selected)
    {
        select.SetActive(selected);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        Clicked(this);
    }
}
