using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbortMissionPopup : MonoBehaviour
{
    [SerializeField]
    private Button confirm = null;
    [SerializeField]
    private Button cancel = null;

    private void Awake()
    {
        confirm.onClick.AddListener(() =>
        {
            GameStateManager.Instance.ShowMissionSummary(false, EMissionLoseCause.MissionAborted);
            Hide();
        });
        cancel.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
