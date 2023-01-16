using System;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionChangeConfirmWindow : MonoBehaviour
{
    [SerializeField]
    private float timeToCancel = 10f;
    [SerializeField]
    private Text timerText = null;
    [SerializeField]
    private Button cancelButton = null;
    [SerializeField]
    private Button confirmButton = null;

    private float timer;
    private Action cancelAction;
    private Action confirmAction;

    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelAction);
        confirmButton.onClick.AddListener(ConfirmAction);
    }

    private void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(timer).ToString();
            if (timer <= 0f)
            {
                CancelAction();
            }
        }
    }

    private void OnEnable()
    {
        timer = timeToCancel;
        timerText.text = ((int)timer + 1).ToString();
    }

    public void Setup(Action cancelAction, Action confirmAction)
    {
        this.cancelAction = cancelAction;
        this.confirmAction = confirmAction;
        gameObject.SetActive(true);
    }

    private void CancelAction()
    {
        cancelAction?.Invoke();
        gameObject.SetActive(false);
    }

    private void ConfirmAction()
    {
        confirmAction?.Invoke();
        gameObject.SetActive(false);
    }
}
