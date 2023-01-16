using UnityEngine;

public class CreateMissionButton : MonoBehaviour
{
    [SerializeField]
    private EMissionOrderType missionType = EMissionOrderType.Airstrike;

    public void AddMission()
    {
        TacticManager.Instance.AddNewMission(missionType);
    }
}
