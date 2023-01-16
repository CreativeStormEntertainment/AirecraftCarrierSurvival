using System;
using UnityEngine;

public class MouseCatcher : MonoBehaviour
{
    public event Action MouseEntered = delegate { };
    public event Action MouseExited = delegate { };
    public event Action MouseClicked = delegate { };

    private void OnMouseEnter()
    {
        MouseEntered();
    }

    private void OnMouseExit()
    {
        MouseExited();
    }

    private void OnMouseUpAsButton()
    {
        MouseClicked();
    }
}
