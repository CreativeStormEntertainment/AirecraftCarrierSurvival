using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPlane : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{
    /*
    public Image PlaneIcon;
    public GameObject LockedIcon;
    public GameObject PriceText;
    public GameObject Price;
    [SerializeField] private GameObject descriptionPanel = null;
    //[SerializeField] private Text numberOfSelected = null;
    //[SerializeField] private ButtonIntermissionPanel planeButton = null;
    [SerializeField] private EPlaneType planeType = 0;

    public bool IsChosen = false;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        descriptionPanel.SetActive(true);

       //Description.Instance.FillPlaneDescription(planeType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionPanel.SetActive(false);
    }

    private void OnClicked()
    {
        var saveData = SaveManager.Instance.Data;

        var planeIndex = (int)planeType;
        var planesList = IntermissionManager.Instance.GetPlaneList();
        var costCurrentPlane = planesList[planeIndex].GetUnlockCost();

        if ((saveData.LockedPlaneTypes & (1 << (planeIndex))) == (1 << (planeIndex)))
        {
            if (costCurrentPlane <= saveData.CommandPoints)
            {
                LockedIcon.SetActive(false);
                PriceText.SetActive(false);
                Price.SetActive(false);

                IntermissionPanel.Instance.ReduceCommandPoints(costCurrentPlane);

                saveData.LockedPlaneTypes = saveData.LockedPlaneTypes & ~(1 << (planeIndex)); // unlock plane
            }
        }
        else
        {
//            if (!IsChosen)
//            {
//                if (planeButton.choicesNumber < planeButton.GetMaxChoicesNumber())
//                {
//                    planeButton.choicesNumber++;

//                    numberOfSelected.text = planeButton.choicesNumber.ToString();

//                    saveData.Planes.Add(planeType);

//#warning to change: what should happened with button, when particulary planeType is already been chosen ?
//                    gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255);

//                    IsChosen = !IsChosen;
//                }
//            }
//            else
//            {
//                planeButton.choicesNumber--;
//                numberOfSelected.text = planeButton.choicesNumber.ToString();

//                saveData.Planes.Remove(planeType);
//#warning to change
//                gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);

//                IsChosen = !IsChosen;
//            }
        }
    }

    public EPlaneType GetPlaneType()
    {
        return planeType;
    }
    */
}
