using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftItem : MonoBehaviour, IInventoryItem<AircraftIntermissionData>, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<EPlaneType> LeftClicked = delegate { };
    public event Action<EPlaneType> RightClicked = delegate { };

    public event Action<EPlaneType, bool> Highlight = delegate { };

    [SerializeField]
    private Text planeName = null;

    [SerializeField]
    private Image planeImage = null;

    [SerializeField]
    private Image planeTypeImage = null;

    [SerializeField]
    private Clickable clickable = null;

    [SerializeField]
    private List<Button> skinsButtons = null;

    [SerializeField]
    private PlanesTiersData planesTiers = null;

    [SerializeField]
    private GameObject highlight = null;

    private AircraftIntermissionData data;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(data.Type, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(data.Type, false);
    }

    public void Setup(AircraftIntermissionData data)
    {
        this.data = data;

        clickable.Setup();
        clickable.LeftClicked += () => FireLeftClicked(this.data.Type);
        clickable.RightClicked += () => FireRightClicked(this.data.Type);
        for (int i = 0; i < skinsButtons.Count; i++)
        {
            int index = i;
            skinsButtons[index].onClick.AddListener(() => OnClicked(index));
        }

        Refresh();
        OnClicked(data.ChosenSkin);
    }

    public void Refresh()
    {
        var tierData = planesTiers.Data.Find(item => item.PlaneType == data.Type);
        planeImage.sprite = tierData.PlaneTiers[data.CurrentTier].Sprite;
        planeName.text = tierData.PlaneTiers[data.CurrentTier].Name;
        planeTypeImage.sprite = tierData.PlaneTypeSprite;
    }

    public void SetShowHighlight(bool show)
    {
        highlight.SetActive(show);
    }

    private void OnClicked(int index)
    {
        skinsButtons[data.ChosenSkin].interactable = true;

        data.ChosenSkin = index;

        skinsButtons[data.ChosenSkin].interactable = false;
        foreach (var obj in data.Squadrons)
        {
            obj.SetSkin(data.ChosenSkin);
        }
    }

    private void FireLeftClicked(EPlaneType type)
    {
        LeftClicked(type);
    }

    private void FireRightClicked(EPlaneType type)
    {
        RightClicked(type);
    }
}
