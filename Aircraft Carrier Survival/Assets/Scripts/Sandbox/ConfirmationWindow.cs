using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationWindow : MonoBehaviour, IPopupPanel, IEscapeablePopup
{
    public EWindowType Type => EWindowType.Other;

    [SerializeField]
    private Button confirm = null;
    [SerializeField]
    private Button cancel = null;

    private Action onConfirm;

    private void Awake()
    {
        confirm.onClick.AddListener(Confirm);
        cancel.onClick.AddListener(Cancel);
    }

    public void Show(Action onConfirm)
    {
        this.onConfirm = onConfirm;
        gameObject.SetActive(true);
        if (HudManager.Instance != null)
        {
            HudManager.Instance.PopupShown(this);
        }
        else
        {
            IntermissionManager.Instance.RegisterEscapeable(this);
        }
    }

    private void Confirm()
    {
        onConfirm?.Invoke();
    }

    private void Cancel()
    {
        if (HudManager.Instance != null)
        {
            Hide();
        }
        else
        {
            OnEscape();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        HudManager.Instance.PopupHidden(this);
    }

    public bool OnEscape()
    {
        bool success = gameObject.activeSelf;
        gameObject.SetActive(false);
        return success;
    }
}
