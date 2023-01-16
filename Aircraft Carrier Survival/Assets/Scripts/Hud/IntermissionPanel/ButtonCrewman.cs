using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonCrewman : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image Portrait;
    public GameObject LockedImage;
    public GameObject PriceText;
    public GameObject Price;
    [SerializeField] private GameObject descriptionPanel = null;
    [SerializeField] private ChooseCrew panel = null;
    //[SerializeField] private Text numberOfSelected = null;
    //[SerializeField] private ButtonIntermissionPanel crewButton = null;
    public int CrewmanIndex = 0;

    [SerializeField] GameObject highlightTexture = null;
    [SerializeField] GameObject pressedTexture = null;
    [SerializeField] GameObject pressedTexture2 = null;

    public bool IsChosen = false;



    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightTexture.SetActive(true);

        descriptionPanel.SetActive(true);

        var panelRectT = panel.GetComponent<RectTransform>();
        var panelSize = panelRectT.sizeDelta / 2;
        panelSize.x *= -1;
        var pos = GetComponent<RectTransform>().anchoredPosition + panelRectT.anchoredPosition + panelSize;

        CrewmanDescription.Instance.FillCrewmanDescription(CrewmanIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightTexture.SetActive(false);

        descriptionPanel.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        highlightTexture.SetActive(false);
        pressedTexture.SetActive(true);
        pressedTexture2.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressedTexture.SetActive(false);
        pressedTexture2.SetActive(false);
    }

    private void OnClicked()
    {
        panel.Hide();

        var slotPortrait = SlotCrewButton.LastSelected.Portrait.sprite;
        var chosenCrewmanPortrait = Portrait.sprite;
        Portrait.sprite = slotPortrait;
        SlotCrewButton.LastSelected.Portrait.sprite = chosenCrewmanPortrait;

        var slotCrewmanIndex = SlotCrewButton.LastSelected.CrewmanIndex;
        var chosenCrewmanIndex = CrewmanIndex;
        CrewmanIndex = slotCrewmanIndex;
        SlotCrewButton.LastSelected.CrewmanIndex = chosenCrewmanIndex;

        /*
        var saveData = SaveManager.Instance.Data;

        var crewmanList = saveData.AvailableCrewmen;
        var costCurrentCrewman = crewmanList[CrewmanIndex].UnlockCost;

        if ((saveData.LockedCrewman & (1 << (CrewmanIndex))) == (1 << (CrewmanIndex)))
        {
            if (costCurrentCrewman <= saveData.CommandPoints)
            {
                LockedImage.SetActive(false);
                PriceText.SetActive(false);
                Price.SetActive(false);

                IntermissionPanel.Instance.ReduceCommandPoints(costCurrentCrewman);

                saveData.LockedCrewman = saveData.LockedCrewman & ~(1 << (CrewmanIndex)); // unlock crewman
            }
        }
        else
        {
            if (!IsChosen)
            {
                if (crewButton.choicesNumber < crewButton.GetMaxChoicesNumber())
                {
                    crewButton.choicesNumber++;

                    numberOfSelected.text = crewButton.choicesNumber.ToString();

                    saveData.ChosenCrew = saveData.ChosenCrew | (1 << (CrewmanIndex)); // add chosen crewman

#warning to change: what should happened with button, when particulary planeType is already been chosen ?
                    //gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255);

                    IsChosen = !IsChosen;
                }
            }
            else
            {
                crewButton.choicesNumber--;
                numberOfSelected.text = crewButton.choicesNumber.ToString();

                saveData.ChosenCrew = saveData.ChosenCrew & ~(1 << (CrewmanIndex)); //// remove crewman
#warning to change
                //gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);

                IsChosen = !IsChosen;
            }
        }
        */
    }
}
