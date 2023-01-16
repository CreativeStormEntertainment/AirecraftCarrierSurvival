using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseUIManeuverCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerManeuverData Data
    {
        get;
        private set;
    }

    [SerializeField]
    protected GameObject highlight = null;
    [SerializeField]
    private Text titleText = null;
    [SerializeField]
    private Text descriptionText = null;
    [SerializeField]
    private Image squadronImage = null;
    [SerializeField]
    private Text squadronAmount = null;

    [SerializeField]
    private Image lvBg = null;
    [SerializeField]
    private Sprite lv2 = null;
    [SerializeField]
    private Sprite lv3 = null;

    [SerializeField]
    private List<Sprite> squadronIconVariants = null;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        highlight.SetActive(true);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        highlight.SetActive(false);
    }

    public virtual void Setup(PlayerManeuverData data, int level)
    {
        Data = data;

        lvBg.gameObject.SetActive(level > 1);
        lvBg.sprite = level == 2 ? lv2 : lv3;

        var locMan = LocalizationManager.Instance;
        titleText.text = locMan.GetText(data.Name);
        descriptionText.text = locMan.GetText(data.Description);

        squadronImage.sprite = squadronIconVariants[(int)data.NeededSquadrons.Type];
        squadronAmount.text = data.NeededSquadrons.Count.ToString();
    }
}
