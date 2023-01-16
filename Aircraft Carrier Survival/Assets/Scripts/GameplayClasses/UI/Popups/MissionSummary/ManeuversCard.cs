using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManeuversCard : CrewManeuversCard, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject highlightAtt = null;
    [SerializeField]
    private GameObject highlightDef = null;
    [SerializeField]
    private GameObject highlightSquadrons = null;

    [SerializeField]
    private GameObject pressed = null;

    [SerializeField]
    private Button button = null;

    private OfficerUpgradeWindow upgradeWindow;
    private ChoseBuffWindow choseBuffWindow;

    private PlayerManeuverData maneuverData;
    private PlayerManeuverData baseManeuverData;

    private void Start()
    {
        button.onClick.AddListener(SelectManeuver);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled)
        {
            base.OnPointerEnter(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.enabled)
        {
            pressed.SetActive(true);
        }
    }

    public void Setup(PlayerManeuverData data, int level, OfficerUpgradeWindow officerUpgradeWindow = null, ChoseBuffWindow choseWindow = null, PlayerManeuverData baseManeuver = null)
    {
        base.Setup(data, level);
        highlight.SetActive(false);
        pressed.SetActive(false);
        maneuverData = data;
        baseManeuverData = baseManeuver;
        choseBuffWindow = choseWindow;
        upgradeWindow = officerUpgradeWindow;
        button.enabled = officerUpgradeWindow != null || choseBuffWindow != null;
        ResetHighlights();
        gameObject.SetActive(true);
    }

    private void ResetHighlights()
    {
        highlightAtt.SetActive(false);
        highlightDef.SetActive(false);
        highlightSquadrons.SetActive(false);
    }

    public void HighlightUpgrades(PlayerManeuverData previous, PlayerManeuverData upgraded)
    {
        highlightAtt.SetActive(previous.BaseValues.Attack < upgraded.BaseValues.Attack);
        highlightDef.SetActive(previous.BaseValues.Defense < upgraded.BaseValues.Defense);
        highlightSquadrons.SetActive(previous.NeededSquadrons.Count > upgraded.NeededSquadrons.Count);
    }

    private void SelectManeuver()
    {
        if (upgradeWindow != null)
        {
            upgradeWindow.SelectManeuver();
        }
        else if (choseBuffWindow != null)
        {
            choseBuffWindow.AddToLeftPanel(baseManeuverData);
        }
    }
}
