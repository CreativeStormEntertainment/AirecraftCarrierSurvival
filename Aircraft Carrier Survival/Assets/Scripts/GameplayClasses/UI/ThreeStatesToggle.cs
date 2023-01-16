using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThreeStatesToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action Clicked = delegate { };

    public virtual int State
    {
        get => state;
        set
        {
            state = value;
            aState.SetActive(value == 0);
            bState.SetActive(value == 1);
            cState.SetActive(value == 2);
        }
    }

    public bool Hover
    {
        get => hover;
        set
        {
            hover = value;
            aHover.SetActive(hover && enabled);
            bHover.SetActive(hover && enabled);
            cHover.SetActive(hover && enabled);

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
            aPressed.SetActive(pressed || !enabled);
            bPressed.SetActive(pressed || !enabled);
            cPressed.SetActive(pressed || !enabled);
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
    private GameObject aState = null;
    [SerializeField]
    private GameObject bState = null;
    [SerializeField]
    private GameObject cState = null;

    [SerializeField]
    private GameObject aHover = null;
    [SerializeField]
    private GameObject bHover = null;
    [SerializeField]
    private GameObject cHover = null;

    [SerializeField]
    private GameObject aPressed = null;
    [SerializeField]
    private GameObject bPressed = null;
    [SerializeField]
    private GameObject cPressed = null;

    private int state;
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

    public void Setup(int state, bool active)
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
            State = State == 2 ? 0 : State + 1;
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
