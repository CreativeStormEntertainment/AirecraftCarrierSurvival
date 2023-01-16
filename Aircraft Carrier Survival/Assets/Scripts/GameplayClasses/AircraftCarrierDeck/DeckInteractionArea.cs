//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class DeckInteractionArea : MonoBehaviour
//{
//    [SerializeField]
//    private GameObject deckControlPopup = null;
//    [SerializeField]
//    private RectTransform deckControlPopupRectT = null;

//    [SerializeField]
//    private MouseCatcher mouseCatcher = null;

//    private AircraftCarrierDeckManager aircraftCarrierDeck = null;
//    private bool isDisabled = false;

//    private bool outOfCollider = true;

//    private void Start()
//    {
//        //aircraftCarrierDeck = AircraftCarrierDeckManager.Instance;
//        //mouseCatcher.MouseEntered += OnMouseEntered;
//        //mouseCatcher.MouseExited += OnMouseExited;
//        //mouseCatcher.MouseClicked += OnMouseClicked;

//        //CameraManager.Instance.ViewChanged += OnViewChanged;
//    }

//    private void LateUpdate()
//    {
//        if (deckControlPopup.activeSelf)
//        {
//            var camMan = CameraManager.Instance;
//            if (HudManager.Instance.AcceptInput && camMan.IsDeckShown && (!outOfCollider || EventSystem.current.IsPointerOverGameObject()))
//            {
//                deckControlPopupRectT.position = camMan.MainCamera.WorldToScreenPoint(transform.position);
//            }
//            else
//            {
//                HideMenu();
//            }
//        }
//    }

//    private void OnViewChanged(ECameraView view)
//    {
//        if (view == ECameraView.Deck)
//        {
//            isDisabled = false;
//            gameObject.SetActive(true);
//        }
//        else
//        {
//            isDisabled = true;
//            outOfCollider = true;
//            HideMenu();
//            gameObject.SetActive(false);
//        }
//    }

//    public void OnMouseEntered()
//    {
//        if (AircraftCarrierDeckManager.Instance.IsLocked || isDisabled)
//            return;
//        else
//        {
//            ShowMenu();
//            AircraftCarrierDeckManager.Instance.MissionOrdersPopup.Hide(false);
//            AircraftCarrierDeckManager.Instance.LiftInteractionArea.ForceHide();
//        }
//    }

//    public void OnMouseClicked()
//    {
//        if (AircraftCarrierDeckManager.Instance.IsLocked)
//            return;
//    }

//    public void OnMouseExited()
//    {
//        outOfCollider = true;
//    }

//    public void OpenMissionPopup()
//    {
//        AircraftCarrierDeckManager.Instance.MissionPopup.Show(deckControlPopupRectT);
//        ForceHide();
//    }

//    private void HideMenu()
//    {
//        deckControlPopup.SetActive(false);
//        Tooltip.Instance.Hide();
//    }

//    private void ShowMenu()
//    {
//        if (AircraftCarrierDeckManager.Instance.GetMode() == EDeckMode.Blocked || AircraftCarrierDeckManager.Instance.MissionPopup.gameObject.activeSelf)
//            return;

//        deckControlPopup.SetActive(true);
//        outOfCollider = false;
//    }

//    public void AddMissionOrder()
//    {
//        //aircraftCarrierDeck.AddOrder(new MissionOrder(aircraftCarrierDeck, slot.Squadron, slot));
//        HideMenu();
//    }

//    public void ForceHide()
//    {
//        outOfCollider = true;
//        HideMenu();
//    }

//}
