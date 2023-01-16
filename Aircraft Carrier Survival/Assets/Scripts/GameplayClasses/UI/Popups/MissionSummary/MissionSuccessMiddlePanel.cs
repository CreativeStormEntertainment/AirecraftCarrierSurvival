using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionSuccessMiddlePanel : MonoBehaviour
{
    [SerializeField]
    private Text commandPoints = null;
    [SerializeField]
    private Text upgradePoints = null;
    [SerializeField]
    private Text expPoints = null;
    [SerializeField]
    private Image escortImage = null;
    [SerializeField]
    private Text escortName = null;
    [SerializeField]
    private GameObject escort = null;
    [SerializeField]
    private StrikeGroupData strikeGroupData = null;

    public void Setup()
    {
        ref var data = ref SaveManager.Instance.Data.IntermissionMissionData;
        upgradePoints.text = data.UpgradePoints.ToString();
        commandPoints.text = data.CommandsPoints.ToString();
        bool escortReward = data.EscortType >= 0;
        escort.SetActive(escortReward);
        if (escortReward)
        {
            var escortData = strikeGroupData.Data[data.EscortType];
            escortImage.sprite = escortData.Icon;
            escortName.text = LocalizationManager.Instance.GetText(escortData.NameID);
        }
        if (SaveManager.Instance.Data.GameMode == EGameMode.Sandbox)
        {
            upgradePoints.transform.parent.gameObject.SetActive(false);
            expPoints.transform.parent.gameObject.SetActive(true);
            expPoints.text = SaveManager.Instance.Data.SandboxData.CurrentMissionSaveData.SandboxMissionRewards.AdmiralExp.ToString();
        }
    }
}
