using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    [Header("Mission Ending")]
    [SerializeField] private EMissionLoseCause missionResult = default;
    [SerializeField] private bool missionSuccess = false;
    [SerializeField] private Button endMissionButton = null;

    private void Start()
    {
        endMissionButton.onClick.AddListener(() => {
            GameStateManager.Instance.ShowMissionSummary(missionSuccess, missionResult);
        });
    }
}
