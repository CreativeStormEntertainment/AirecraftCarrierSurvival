using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action Clicked = delegate { };

    public virtual bool State
    {
        get => state;
        set
        {
            state = value;
            onState.SetActive(state);
            offState.SetActive(!state);
        }
    }

    public bool Hover
    {
        get => hover;
        set
        {
            hover = value;
            onHover.SetActive(hover && enabled);
            offHover.SetActive(hover && enabled);

            if (enabled)
            {
                frame.sprite = hover ? frameHovered : frameNormal;
            }
            else
            {
                frame.sprite = frameDisabled;
            }
        }
    }

    public bool Pressed
    {
        get => pressed;
        set
        {
            pressed = value;
            onPressed.SetActive(pressed || !enabled);
            offPressed.SetActive(pressed || !enabled);
        }
    }

    [SerializeField]
    private Image frame = null;

    [SerializeField]
    private Sprite frameNormal = null;

    [SerializeField]
    private Sprite frameHovered = null;

    [SerializeField]
    private Sprite frameDisabled = null;

    [SerializeField]
    private GameObject onState = null;
    [SerializeField]
    private GameObject offState = null;

    [SerializeField]
    private GameObject onHover = null;
    [SerializeField]
    private GameObject offHover = null;

    [SerializeField]
    private GameObject onPressed = null;
    [SerializeField]
    private GameObject offPressed = null;

    private bool state;
    private bool hover;
    private bool pressed;

    private void OnEnable()
    {
        Hover = Hover;
        Pressed = Pressed;
    }

    private void OnDisable()
    {
        Hover = Hover;
        Pressed = Pressed;
    }

    public void Setup(bool state, bool active)
    {
        State = state;

        enabled = active;
        Hover = false;
        Pressed = false;
        gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (enabled)
        {
            State = !State;
            Clicked();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hover = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
    }
}
