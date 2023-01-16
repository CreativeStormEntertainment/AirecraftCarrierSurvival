using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LiftPanel : MonoBehaviour, IPointerExitHandler
{
    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private Button bomberButton = null;
    [SerializeField]
    private Button fighterButton = null;
    [SerializeField]
    private Button torpedoButton = null;
    [SerializeField]
    private Text bomberText = null;
    [SerializeField]
    private Text fighterText = null;
    [SerializeField]
    private Text torpedoText = null;
    [SerializeField]
    private RectTransform rectTransform = null;

    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color disabledColor = Color.white;

    public bool IsOpen
    {
        get => panel.activeSelf;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetShowPanel(false);
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }

    public void SetShowPanel(bool show)
    {
        panel.SetActive(show);
        if (show)
        {
            CheckButtons();
        }
    }

    private void CheckButtons()
    {
        var deckMan = AircraftCarrierDeckManager.Instance;
        var planesType = DragPlanesManager.Instance.SquadronTypeEnabled;

        bool bombersInteractable = deckMan.GetFreeSquadronCount(EPlaneType.Bomber) > 0 && (planesType == EManeuverSquadronType.Any || planesType == EManeuverSquadronType.Bomber) &&
            deckMan.CanAdd(EPlaneType.Bomber);
        bool fightersInteractable = deckMan.GetFreeSquadronCount(EPlaneType.Fighter) > 0 && (planesType == EManeuverSquadronType.Any || planesType == EManeuverSquadronType.Fighter) &&
            deckMan.CanAdd(EPlaneType.Fighter);
        bool torpedoesInteractable = deckMan.GetFreeSquadronCount(EPlaneType.TorpedoBomber) > 0 && (planesType == EManeuverSquadronType.Any || planesType == EManeuverSquadronType.Torpedo) &&
            deckMan.CanAdd(EPlaneType.TorpedoBomber);

        bomberButton.interactable = bombersInteractable;
        fighterButton.interactable = fightersInteractable;
        torpedoButton.interactable = torpedoesInteractable;

        bomberText.color = bombersInteractable ? normalColor : disabledColor;
        fighterText.color = fightersInteractable ? normalColor : disabledColor;
        torpedoText.color = torpedoesInteractable ? normalColor : disabledColor;
    }
}
