using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;

public class TacticalFightCameraSwitcher : MonoBehaviour
{
    public bool IsAfterPlayerPlaneButtonClickCameraChanged;
    [SerializeField]
    List<TacticalFightEnemyVisualizationSlot> TacticalFightEnemyUnitsVisualizations;
    [SerializeField]
    List<TacticalFightPlayerVisualizationSlot> TacticalFighPlayerPlaneVisualizations;
    private int lastSettedEnemyVisualisationIndex = 0;
    private int LastSettedPlayerVisualizationIndex = 0;

    public static TacticalFightCameraSwitcher Instance;

    private void Awake()
    {
        Instance = this;
        TacticalFightEnemyUnitsVisualizations = transform.GetChild(0).gameObject.GetComponentsInChildren<TacticalFightEnemyVisualizationSlot>(true).ToList();
        TacticalFighPlayerPlaneVisualizations = transform.GetChild(1).gameObject.GetComponentsInChildren<TacticalFightPlayerVisualizationSlot>(true).ToList();
    }

    public void SetCameraEnemyUnitActive(TacticalFightEnemyUnit enemyUnitToSet)
    {
        TacticalFightEnemyVisualizationSlot visualizationSlotToSet = null;

        TacticalFighPlayerPlaneVisualizations[LastSettedPlayerVisualizationIndex].DeaactivateVisualizationSlot();
        TacticalFightEnemyUnitsVisualizations[lastSettedEnemyVisualisationIndex].DeaactivateVisualizationSlot();

        if (TacticalFightEnemyUnitsVisualizations.FirstOrDefault(x => x.GetEnemyAcquiredForSlot() == enemyUnitToSet) != null)
            visualizationSlotToSet = TacticalFightEnemyUnitsVisualizations.FirstOrDefault(x => x.GetEnemyAcquiredForSlot() == enemyUnitToSet);

        lastSettedEnemyVisualisationIndex = TacticalFightEnemyUnitsVisualizations.IndexOf(visualizationSlotToSet);
        TacticalFightEnemyUnitsVisualizations[lastSettedEnemyVisualisationIndex].ActivateVisualizationSlot();
    }
    public void SetCameraPlayerUnitActive(TacticalFightPlayerUnit playerUnitToSet)
    {
        TacticalFightEnemyUnitsVisualizations[lastSettedEnemyVisualisationIndex].DeaactivateVisualizationSlot();
        TacticalFighPlayerPlaneVisualizations[LastSettedPlayerVisualizationIndex].DeaactivateVisualizationSlot();
        LastSettedPlayerVisualizationIndex = playerUnitToSet.GetPilotPoolIndex();
        TacticalFighPlayerPlaneVisualizations[LastSettedPlayerVisualizationIndex].ActivateVisualizationSlot(playerUnitToSet);
    }

    public TacticalFightPlayerVisualizationSlot GetPlayerVisualizationSlot(TacticalFightPlayerUnit playerUnit)
    {
        return TacticalFighPlayerPlaneVisualizations[playerUnit.GetPilotPoolIndex()];
    }

    public TacticalFightEnemyVisualizationSlot GetEnemyVisualizationSlot(TacticalFightEnemyUnit enemyUnit)
    {
            return TacticalFightEnemyUnitsVisualizations.FirstOrDefault(x => x.GetEnemyAcquiredForSlot() == enemyUnit);
    }

    public TacticalFightEnemyVisualizationSlot GetFirstFreeEnemyVisualizationSlot()
    {
        return TacticalFightEnemyUnitsVisualizations.FirstOrDefault(x => x.GetEnemyAcquiredForSlot() == null);
    }

    public void UnSetAllEnemyVisualizations()
    {
        TacticalFightEnemyUnitsVisualizations.ForEach(x => x.SetEnemyForSlot(null));
    }
}
