
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class LiftInteractionArea : MonoBehaviour
//{
//    [SerializeField]
//    private MouseCatcher MouseCatcher = null;
//    private bool isDisabled = false;

//    private bool outOfCollider = true;

//    private void LateUpdate()
//    {
//        var deck = AircraftCarrierDeckManager.Instance;
//        deck.SetButtonsActivity();
//        if (deck.squadronPopup.activeSelf && (!HudManager.Instance.AcceptInput || !CameraManager.Instance.IsDeckShown || (outOfCollider && !EventSystem.current.IsPointerOverGameObject())))
//        {
//            HideMenu();
//        }
//    }

//    private void Start()
//    {
//        MouseCatcher.MouseEntered += OnMouseEntered;
//        MouseCatcher.MouseExited += OnMouseExited;
//        MouseCatcher.MouseClicked += OnMouseClicked;

//        CameraManager.Instance.ViewChanged += OnViewChanged;
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
//            gameObject.SetActive(false);
//            isDisabled = true;
//            outOfCollider = true;
//            HideMenu();
//        }
//    }

//    public void OnMouseEntered()
//    {
//        if (AircraftCarrierDeckManager.Instance.IsLocked || AircraftCarrierDeckManager.Instance.IsLandingMode || isDisabled)
//            return;
//        else
//        {
//            ShowMenu();
//            AircraftCarrierDeckManager.Instance.MissionOrdersPopup.Hide(false);
//            AircraftCarrierDeckManager.Instance.DeckInteractionArea.ForceHide();
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

//    private void HideMenu()
//    {
//        AircraftCarrierDeckManager.Instance.squadronPopup.SetActive(false);
//        Tooltip.Instance.Hide();
//    }

//    private void ShowMenu()
//    {
//        if (AircraftCarrierDeckManager.Instance.GetMode() == EDeckMode.Blocked || AircraftCarrierDeckManager.Instance.MissionPopup.gameObject.activeSelf)
//            return;
//        AircraftCarrierDeckManager.Instance.squadronPopup.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position);
//        AircraftCarrierDeckManager.Instance.squadronPopup.SetActive(true);
//        AircraftCarrierDeckManager.Instance.SetButtonsActivity();
//        outOfCollider = false;
//    }

//    public void ForceHide()
//    {
//        outOfCollider = true;
//        HideMenu();
//    }
//}
