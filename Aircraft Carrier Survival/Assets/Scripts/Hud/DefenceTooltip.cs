using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DefenceTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject SectionDebuff => sectionDebuff;
    public GameObject BuffBuff => buffBuff;

    [SerializeField]
    private GameObject tooltipObject = null;
    [SerializeField]
    private Text AAText = null;
    [SerializeField]
    private Text islandText = null;
    [SerializeField]
    private Text defenceText = null;
    [SerializeField]
    private Text escortText = null;
    [SerializeField]
    private Text capText = null;
    [SerializeField]
    private Text sectionDebuffText = null;
    [SerializeField]
    private Text strikeGroupDefencePointsText = null;
    [SerializeField]
    private Text buffBuffText = null;
    [SerializeField]
    private GameObject sectionDebuff = null;
    [SerializeField]
    private GameObject buffBuff = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DefenceHoverIn);
        SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DefenceHoverOut);
        SetActive(false);
    }

    private void SetActive(bool active)
    {
        tooltipObject.SetActive(active);
        HudManager.Instance.FireWindowStateChanged(EWindowType.Defense, active);
    }

    public void SetValues(int AA, int island, int position, int escort, int cap, int sectionDebuff, int strikeGroupDefencePoints, int buffBuffPoints)
    {
        AAText.text = "+" + AA.ToString();
        islandText.text = "+" + island.ToString();
        defenceText.text = "+" + position.ToString();
        escortText.text = "+" + escort.ToString();
        capText.text = "+" + cap.ToString();
        sectionDebuffText.text = "+" + sectionDebuff.ToString();
        strikeGroupDefencePointsText.text = "+" + strikeGroupDefencePoints.ToString();
        buffBuffText.text = "+" + buffBuffPoints.ToString();
    }
}
