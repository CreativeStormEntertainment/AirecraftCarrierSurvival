using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

public class MissionHelperCaller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool Enabled => ((int)TacticManager.Instance.EnabledMissions & (1 << (int)missionType)) != 0;

    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private MissionPanel missionPanel = null;
    [SerializeField]
    private Text missionName = null;
    [SerializeField]
    private RectTransform tooltipParent = null;
    [SerializeField]
    private EMissionOrderType missionType = EMissionOrderType.Airstrike;
    [SerializeField]
    private StudioEventEmitter disabledClickSound = null;

    private void Start()
    {
        missionName.text = TacticManager.Instance.MissionInfo[missionType].MissionName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Enabled)
        {
            highlight.SetActive(true);
            missionPanel.MissionDetails.ShowTooltip(tooltipParent, true);
            missionPanel.MissionDetails.SetupHelper(missionType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Enabled)
        {
            highlight.SetActive(false);
            missionPanel.MissionDetails.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Enabled)
        {
            disabledClickSound.Play();
        }
    }
}
