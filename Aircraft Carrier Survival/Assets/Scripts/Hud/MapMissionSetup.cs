using UnityEngine;
using UnityEngine.UI;

public class MapMissionSetup : MonoBehaviour
{
    public RectTransform Rect;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private TacticalMap map = null;
    [SerializeField]
    private PulseAnim pulseAnim = null;

    private TacticalMission tacticalMission;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (tacticalMission != null)
        {
            Rect.position = tacticalMission.ButtonMission.ConfirmButtonParent.position;
        }
    }

    public void Setup(TacticalMission tacticalMission)
    {
        if (this.tacticalMission != null)
        {
            this.tacticalMission.ConfirmButtonShown = false;
        }
        this.tacticalMission = tacticalMission;
    }

    public void SetInteractable(bool interactable)
    {
        confirmButton.interactable = interactable;
        pulseAnim.SetShow(interactable);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (tacticalMission != null)
        {
            tacticalMission.ConfirmButtonShown = true;
        }
    }

    public void Hide()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
            if (tacticalMission != null)
            {
                tacticalMission.ConfirmButtonShown = false;
            }
        }
    }

    private void OnClick()
    {
        tacticalMission.Confirm();
        map.CancelMissionSetup(true);
        tacticalMission.MissionWaypoints.gameObject.SetActive(false);
        TacticManager.Instance.MissionPanel.MissionDetails.HideTooltip();
        //mapButton.onClick.Invoke();
        Hide();
    }
}
