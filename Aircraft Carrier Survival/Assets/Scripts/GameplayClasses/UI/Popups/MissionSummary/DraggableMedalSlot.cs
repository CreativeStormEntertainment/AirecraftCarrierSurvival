using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableMedalSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool Blocked
    {
        get;
        protected set;
    }
    public RectTransform RectTransform => rectTransform;

    [SerializeField]
    protected Image levelImage = null;
    [SerializeField]
    protected GameObject medalImage = null;
    [SerializeField]
    protected Text medalsCount = null;
    [SerializeField]
    protected List<Sprite> levelFrames = null;
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private GameObject highlight = null;

    private CrewUnit crew;


    public virtual void AssignMedal(DraggableMedal medal)
    {
        medalImage.SetActive(true);
        highlight.SetActive(false);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!Blocked)
        {
            highlight.SetActive(true);
            BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!Blocked)
        {
            highlight.SetActive(false);
        }
    }
}
