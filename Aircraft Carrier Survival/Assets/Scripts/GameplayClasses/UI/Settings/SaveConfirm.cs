using System;
using UnityEngine;
using UnityEngine.UI;

public class SaveConfirm : MonoBehaviour
{
    public event Action<bool> ConfirmClosed = delegate { };

    [SerializeField]
    private Button accept = null;
    [SerializeField]
    private Button cancel = null;

    [SerializeField]
    private Text text = null;
    [SerializeField]
    private Text acceptText = null;

    private void Awake()
    {
        accept.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                ConfirmClosed(true);
            });
        cancel.onClick.AddListener(Hide);
    }

    public void Show(string text, string acceptText)
    {
        this.text.text = text;
        this.acceptText.text = acceptText;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        ConfirmClosed(false);
        gameObject.SetActive(false);
    }
}
