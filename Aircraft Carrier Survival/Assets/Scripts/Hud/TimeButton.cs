using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEnableable
{
    public Button Button => button;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private GameObject pressed = null;
    [SerializeField]
    private GameObject selected = null;
    [SerializeField]
    private GameObject blocked = null;

    [SerializeField]
    private Image icon = null;

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private int timeIndex = 0;

    private bool isSelected;
    private bool isBlocked;

    private HudManager hudMan;
    private Color color = new Color(1f, 1f, 1f, 0.5f);
    private bool disabled;

    private void Start()
    {
        hudMan = HudManager.Instance;

        if (!isSelected)
        {
            SetSelected(false);
        }

        button.onClick.AddListener(() =>
        {
            OnClick();
            switch (timeIndex)
            {
                case 0:
                    hudMan.OnPausePressed(false);
                    break;
                case 1:
                    hudMan.OnPlayPressed();
                    break;
                case 2:
                    hudMan.OnFastPressed();
                    break;
                case 3:
                    hudMan.OnFastestPressed();
                    break;
            }
        });
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            OnPointerUp(null);
            OnPointerExit(null);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!disabled && !isSelected && !isBlocked)
        {
            color.a = .5f;
            icon.color = color;
            pressed.SetActive(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        color.a = .85f;
        icon.color = color;
        pressed.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled && !isSelected && !isBlocked)
        {
            highlight.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.SetActive(false);
    }

    public void OnClick()
    {
        if (HudManager.Instance.TimeIndex == 0)
        {
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.PauseTime);
        }
        else
        {
            if (isSelected || isBlocked)
            {
                BackgroundAudio.Instance.PlayEvent(EMainSceneUI.InactiveClick);
            }
            else
            {
                BackgroundAudio.Instance.PlayEvent(EMainSceneUI.PlayTime);
            }
        }
        //if (!isSelected)
        //{
        //    panel.SelectButton(this);
        //}
    }

    public void SetSelected(bool value)
    {
        color.a = value ? 1f : .85f;
        icon.color = color;

        selected.SetActive(value);
        isSelected = value;
        highlight.SetActive(false);
    }

    public void SetBlock(bool value)
    {
        color.a = value ? .3f : .85f;
        icon.color = color;

        isBlocked = value;
        blocked.SetActive(value);
        highlight.SetActive(false);
    }
}
