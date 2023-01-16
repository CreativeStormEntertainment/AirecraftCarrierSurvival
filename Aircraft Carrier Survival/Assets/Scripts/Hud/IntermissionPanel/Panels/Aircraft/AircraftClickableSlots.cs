using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftClickableSlots : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<EPlaneType, bool> Highlight = delegate { };

    [SerializeField]
    private EPlaneType planeType = EPlaneType.Bomber;
    [SerializeField]
    private List<Button> skinsButtons = null;
    [SerializeField]
    private GameObject highlight = null;

    private AircraftIntermissionData data;

    public void Init(AircraftIntermissionData data)
    {
        this.data = data;
        for (int i = 0; i < skinsButtons.Count; i++)
        {
            int index = i;
            skinsButtons[index].onClick.AddListener(() => OnClicked(index));
        }
        OnClicked(data.ChosenSkin);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(planeType, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(planeType, false);
    }

    public void SetHighlight(bool show)
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
}
