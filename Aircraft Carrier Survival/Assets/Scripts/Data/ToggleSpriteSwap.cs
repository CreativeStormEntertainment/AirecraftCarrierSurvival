using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSpriteSwap : MonoBehaviour
{
    public event Action ToggleChanged = delegate {};

    public bool Show
    {
        get;
        private set;
    }

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private Color inactiveTextColor = default;

    private Color normal;
    private Color highlighted;
    private Color pressed;

    private Color normalPressed;
    private Color highlightedPressed;
    private Color pressedPressed;

    private Color activeTextColor;

    private void Awake()
    {
        normal = button.colors.normalColor;
        highlighted = button.colors.highlightedColor;
        pressed = button.colors.pressedColor;

        normalPressed = pressed;
        highlightedPressed = pressed / .8f;
        pressedPressed = pressed * .8f;

        activeTextColor = text.color;

        button.onClick.AddListener(Toggle);

        Show = true;
        Toggle();
    }

    public void Toggle()
    {
        Show = !Show;
        var block = button.colors;
        if (Show)
        {
            block.normalColor = normalPressed;
            block.highlightedColor = highlightedPressed;
            block.pressedColor = pressedPressed;

            text.color = activeTextColor;
        }
        else
        {
            block.normalColor = normal;
            block.highlightedColor = highlighted;
            block.pressedColor = pressed;

            text.color = inactiveTextColor;
        }
        button.colors = block;
        ToggleChanged();
    }
}
