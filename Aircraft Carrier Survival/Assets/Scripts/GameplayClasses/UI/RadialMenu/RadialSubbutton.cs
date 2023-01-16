using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialSubbutton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image Image;
    public Sprite Normal;
    public Sprite Highlighted;
    public Sprite Pressed;
    public Sprite Inactive;
    public bool IsHighlighted;
    public bool IsInteractable;
    private bool isPressed;
    public bool IsHovered;

    public void SetShow(bool show)
    {
        gameObject.SetActive(show);
        isPressed = false;
        Image.sprite = IsInteractable ? Normal : Inactive;
    }

    public bool Press()
    {
        if (IsHighlighted && IsInteractable)
        {
            isPressed = true;
            IsHighlighted = false;
            Image.sprite = Pressed;
        }
        else
        {
            isPressed = false;
        }
        return isPressed;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        IsHighlighted = true;
        if (IsInteractable && !isPressed)
        {
            Image.sprite = Highlighted;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        IsHighlighted = false;
        if (IsInteractable && !isPressed)
        {
            Image.sprite = Normal;
        }
    }

    public void SetInteractable(bool interactable)
    {
        IsInteractable = interactable;
        if (interactable)
        {
            Image.sprite = IsHighlighted ? Highlighted : Normal;
        }
        else
        {
            Image.sprite = Inactive;
        }
    }
}
