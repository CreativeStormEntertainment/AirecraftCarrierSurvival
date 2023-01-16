using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotCrewButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public int CrewmanIndex { get; set; }
    public int SpecialityCost { get => crewPanel.SpecialityCost; }

    [SerializeField]
    bool isOfficer = false;
    [SerializeField]
    private CrewmanDescription descriptionCrew = null;
    [SerializeField]
    private OfficerDescription descriptionOfficer = null;

    [SerializeField]
    GameObject highlightTexture = null;
    [SerializeField]
    GameObject pressedTexture = null;
    [SerializeField]
    GameObject pressedTexture2 = null;
    [SerializeField]
    GameObject selectedTexture = null;
    [SerializeField]
    private Image rankImage = null;

    [SerializeField]
    private BuyCrewmanSpecialty buyCrewmanSpecialty = null;


    public Image Portrait;

    public static SlotCrewButton LastSelected;

    private Button button;
    private CrewSubpanel_Old crewPanel;

    public bool IsOfficer
    {
        get
        {
            return isOfficer;
        }
    }

    public CrewmanDescription DescriptionCrew
    {
        get
        {
            return descriptionCrew;
        }
    }

    public void Setup(CrewSubpanel_Old crewPanel, int crewmanIndex, Sprite portrait, Sprite rankSprite, OfficerSetup officer = null)
    {
        this.crewPanel = crewPanel;
        CrewmanIndex = crewmanIndex;
        button = GetComponent<Button>();
        button.onClick.AddListener(ShowPopup);

        Portrait.sprite = portrait;
        if (isOfficer)
        {
            descriptionOfficer.FillOfficerDescription(officer);
        }
        else
        {
            rankImage.sprite = rankSprite;
            descriptionCrew.FillCrewmanDescription(CrewmanIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightTexture.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightTexture.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsOfficer)
        {
            if (LastSelected != null)
            {
                LastSelected.Deselect();
            }

            highlightTexture.SetActive(false);
            pressedTexture.SetActive(true);
            pressedTexture2.SetActive(true);

            LastSelected = this;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsOfficer)
        {
            pressedTexture2.SetActive(false);
            selectedTexture.SetActive(true);
        }
    }

    public void Deselect()
    {
        pressedTexture.SetActive(false);
        selectedTexture.SetActive(false);
    }

    private void ShowPopup()
    {
        var data = crewPanel.CurrentCrew[CrewmanIndex];
        if (!isOfficer && data.Rank > 0 && data.Specialty == ECrewmanSpecialty.None && 
            SaveManager.Instance.Data.IntermissionMissionData.CommandsPoints >= crewPanel.SpecialityCost)
        {
            buyCrewmanSpecialty.gameObject.SetActive(true);
            IntermissionPanel.Instance.ActivateClickBlocker(true);
            buyCrewmanSpecialty.CurrentChosen = this;
            buyCrewmanSpecialty.RandomizeSpecialty();
        }
    }
}
