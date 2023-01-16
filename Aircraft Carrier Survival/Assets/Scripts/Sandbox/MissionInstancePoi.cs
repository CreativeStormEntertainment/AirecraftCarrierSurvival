using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionInstancePoi : SandboxPoi
{
    protected List<ESandboxObjectiveType> sandboxObjectiveTypes = new List<ESandboxObjectiveType>();

    public override void Setup(SandboxPoiData data)
    {
        base.Setup(data);
        if (data.ObjectiveDescriptionIndex == -1)
        {
            var sandMan = SandboxManager.Instance;
            var maps = WorldMap.Instance.NodeMaps.NodeDatas[Data.NodeIndex].Maps;
            if (data.ObjectiveType != ESandboxObjectiveType.None)
            {
                var objective = maps.Find(map => map.Type == data.ObjectiveType);
                if (objective != null)
                {
                    data.MapIndex = maps.IndexOf(objective);
                }
                else
                {
                    data.MapIndex = maps.IndexOf(RandomUtils.GetRandom(maps));
                }
            }
            else
            {
                sandboxObjectiveTypes = new List<ESandboxObjectiveType>(sandMan.PoiManager.GetSandboxObjectiveBasket(data.PoiType));
                var availableMaps = maps.FindAll(map => sandboxObjectiveTypes.Contains(map.Type));
                if (availableMaps != null && availableMaps.Count > 0)
                {
                    data.MapIndex = maps.IndexOf(RandomUtils.GetRandom(availableMaps));
                }
                else
                {
                    data.MapIndex = maps.IndexOf(RandomUtils.GetRandom(maps));
                }
            }
            var worldMap = WorldMap.Instance;
            var mapData = worldMap.NodeMaps.NodeDatas[data.NodeIndex].Maps[data.MapIndex];
            var enemiesCount = worldMap.SandboxMapSpawner.Init(mapData, data.Difficulty, data.AdditionalEnemiesCount);
            data.AdditionalEnemiesCount = mapData.AdditionalFleets;
            var rewards = sandMan.RewardsScriptable.RewardsList.Find(item => item.ObjectiveType == maps[data.MapIndex].Type).GetRewardsData(Data.Difficulty);
            data.MissionRewards = new SandboxMissionRewards(rewards.AdmiralExp, rewards.CommandPoints);
            data.DescriptionIndex = UnityEngine.Random.Range(1, 3);
            data.ObjectiveDescriptionIndex = UnityEngine.Random.Range(1, 6);
            data.EnemyForcesDescriptionIndex = UnityEngine.Random.Range(1, 6);
        }
    }

    public override void Load(SandboxPoiData data)
    {
        base.Load(data);
    }

    public override void OnClick()
    {
        base.OnClick();
        if (InRange && !Locked)
        {
            var sandMan = SandboxManager.Instance;
            ref var sandboxSave = ref SaveManager.Instance.Data.SandboxData;
            sandboxSave.CurrentMissionSaveData = new SandboxCurrentMissionSaveData(Data.NodeIndex, Data.MissionRewards, Data.ObjectiveType, Data.PoiType);
            TimeManager.Instance.SaveData(ref sandboxSave.MissionStartTime);
            var worldMap = WorldMap.Instance;
            var data = worldMap.NodeMaps.NodeDatas[Data.NodeIndex].Maps[Data.MapIndex];
            var selectedMap = worldMap.SandboxMapSpawner.Spawn(data);
            sandMan.SetMission(selectedMap, sandMan.PoiManager.GetNode(Data.NodeIndex));
            sandMan.PoiManager.RemovePoi(this);
        }
    }
}
