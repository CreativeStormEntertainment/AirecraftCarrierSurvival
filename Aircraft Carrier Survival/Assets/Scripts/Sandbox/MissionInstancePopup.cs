using UnityEngine;
using UnityEngine.UI;

public class MissionInstancePopup : SandboxPopup
{
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private Text missionType = null;
    [SerializeField]
    private Text enemyForces = null;

    private SOTacticMap selectedMap;

    public override void Show(SandboxPoi poi)
    {
        base.Show(poi);
        buttonA.interactable = CheckInRange();
        var locMan = LocalizationManager.Instance;
        var sandMan = SandboxManager.Instance;
        var worldMap = WorldMap.Instance;
        var data = worldMap.NodeMaps.NodeDatas[poi.Data.NodeIndex].Maps[poi.Data.MapIndex];
        description.text = locMan.GetText("Map_" + poi.Data.RegionIndex + "_" +
            sandMan.SandboxTerritoryManager.GetClosestNode(poi.RectTransform.anchoredPosition).TerritoryType + "_" + poi.Data.DescriptionIndex.ToString("00"));
        missionType.text = locMan.GetText(worldMap.NodeMaps.NodeDatas[poi.Data.NodeIndex].Maps[poi.Data.MapIndex].Type.ToString() + "_" + poi.Data.ObjectiveDescriptionIndex.ToString("00"));
        enemyForces.text = locMan.GetText("EnemyStrength" + data.EnemiesCount + "_" + poi.Data.EnemyForcesDescriptionIndex.ToString("00"));
    }

    /// EnterMission
    protected override void OnClickA()
    {
        base.OnClickA();
        HudManager.Instance.HideWorldMap();
        var sandMan = SandboxManager.Instance;
        sandMan.SetMission(selectedMap, sandMan.PoiManager.GetNode(poi.Data.NodeIndex));
        sandMan.PoiManager.RemovePoi(poi);
    }

    protected override void OnClickB()
    {
        base.OnClickB();
    }
}
