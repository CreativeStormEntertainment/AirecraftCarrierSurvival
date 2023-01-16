using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SandboxNodesTools
{
    private static List<Texture2D> landMasks;
    private static TacticalMapGrid CurrentMapGrid;
    private static PathNodeComparer PathNodeComparer;

    [MenuItem("Tools/Sandbox/SetupNodes")]
    private static void SetupNodes()
    {
        PathNodeComparer = new PathNodeComparer();
        HashSet<SandboxNode> enemyFleetsNodes = new HashSet<SandboxNode>();
        HashSet<SandboxNode> enemyBasesNodes = new HashSet<SandboxNode>();
        HashSet<SandboxNode> poiNodes = new HashSet<SandboxNode>();
        HashSet<SandboxNode> repairSpotNodes = new HashSet<SandboxNode>();
        List<Vector2> borderNodes = new List<Vector2>();

        var availableEnemyFleetsNodes = new List<Vector2>();
        var availableEnemyBasesNodes = new List<Vector2>();
        var availableEnemyBasesOnWaterNodes = new List<Vector2>();

        List<SandboxNode> neighbours = new List<SandboxNode>();

        var worldMap = SceneUtils.FindObjectOfType<WorldMap>();
        landMasks = worldMap.LandMasks;
        CurrentMapGrid = new TacticalMapGrid(landMasks, 1920f, 1080f, 200, 100);
        if (worldMap == null)
        {
            Debug.LogError("This scene doesn't have WorldMap!");
            return;
        }
        var spawner = SceneUtils.FindObjectOfType<SandboxMapSpawner>();
        if (spawner == null)
        {
            Debug.LogError("This scene doesn't have SandboxMapSpawner!");
            return;
        }
        spawner.SetupEditor();

        int index = 1;
        List<SandboxNode> allNodes = SceneUtils.FindObjectsOfType<SandboxNode>();
        foreach (var obj in allNodes)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Nodes init", $"In progress...{index}/{allNodes.Count}", (float)index / allNodes.Count))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
            if (obj.CanSpawnBase)
            {
                enemyBasesNodes.Add(obj);
                if (obj.CanSpawnRepairSpot)
                {
                    repairSpotNodes.Add(obj);
                }
            }
            else
            {
                if (!obj.CanSpawnRepairSpot)
                {
                    if (obj.CanSpawnPoi)
                    {
                        poiNodes.Add(obj);
                    }
                    enemyFleetsNodes.Add(obj);
                }
                else
                {
                    repairSpotNodes.Add(obj);
                }
            }
            index++;
        }
        worldMap.AllSandboxNodes = new List<SandboxNode>(allNodes);
        worldMap.PoiNodes = new List<SandboxNode>(poiNodes);
        worldMap.EnemyFleetsNodes = new List<SandboxNode>(enemyFleetsNodes);
        worldMap.EnemyBasesNodes = new List<SandboxNode>(enemyBasesNodes);
        worldMap.RepairSpotNodes = new List<SandboxNode>(repairSpotNodes);
        var poiAndRepairSpots = new List<SandboxNode>(poiNodes);
        poiAndRepairSpots.AddRange(repairSpotNodes);
        EditorUtility.ClearProgressBar();

        //Setup neighbours
        var minDistanceBetweenPoisSqr = worldMap.MinDistanceBetweenPois * worldMap.MinDistanceBetweenPois;
        foreach (var node in poiAndRepairSpots)
        {
            neighbours.Clear();
            foreach (var node1 in poiAndRepairSpots)
            {
                if ((node.Position - node1.Position).sqrMagnitude < minDistanceBetweenPoisSqr)
                {
                    neighbours.Add(node1);
                }
            }
            node.SetNeighbours(neighbours);
        }

        //Setup poi enemy nodes
        index = 1;
        var halfHeight = TacticalMapCreator.TacticMapHeight / 2f;
        var halfWidth = TacticalMapCreator.TacticMapWidth / 2f - TacticalMapCreator.HorizontalOffset * TacticalMapCreator.TacticToWorldMapScale;
        Vector2 offset = new Vector2(0f, halfHeight);
        var now = DateTime.Now;
        HashSet<PathNode> pathNodes = new HashSet<PathNode>();
        List<PathNode> nodeList = new List<PathNode>();
        List<int> startNodeList = new List<int>();
        List<List<float>> distancesList = new List<List<float>>();
        int maxFleetsCount = 0;
        int minFleetsCount = 100;
        int maxBasesCount = 0;
        int minBasesCount = 100;
        SandboxNode maxFleetsNode = null;
        SandboxNode minFleetsNode = null;
        SandboxNode maxBasesNode = null;
        SandboxNode minBasesNode = null;
        Dictionary<PathNode, int> fleetNodesDict = new Dictionary<PathNode, int>();
        Dictionary<PathNode, int> basesOnWaterNodesDict = new Dictionary<PathNode, int>();
        Dictionary<PathNode, int> edgeNodesDict = new Dictionary<PathNode, int>();
        worldMap.NodeMaps.NodeDatas.Clear();
        try
        {
            foreach (var node in poiNodes)
            {
                var tacticMapOffset = node.Position + offset;
                var nodeTacticMapPoint = TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(node.Position, tacticMapOffset);
                if (EditorUtility.DisplayCancelableProgressBar("Nodes setup", $"In progress... {index}/{poiNodes.Count}", (float)index / poiNodes.Count))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                CurrentMapGrid.UpdateIsOnLand(tacticMapOffset);
                SetupPathNodes(nodeTacticMapPoint, pathNodes, nodeList, node);
                foreach (var enemyNode in enemyFleetsNodes)
                {
                    if (node != enemyNode && HasPath(TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(enemyNode.Position, tacticMapOffset), pathNodes))
                    {
                        availableEnemyFleetsNodes.Add(enemyNode.Position);
                    }
                }
                foreach (var baseNode in enemyBasesNodes)
                {
                    if (node != baseNode && HasPath(TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(baseNode.Position + baseNode.OnWaterTransform.anchoredPosition, tacticMapOffset), pathNodes))
                    {
                        availableEnemyBasesNodes.Add(baseNode.Position);
                        availableEnemyBasesOnWaterNodes.Add(baseNode.Position + baseNode.OnWaterTransform.anchoredPosition);
                    }
                }
                if (availableEnemyFleetsNodes.Count > maxFleetsCount)
                {
                    maxFleetsCount = availableEnemyFleetsNodes.Count;
                    maxFleetsNode = node;
                }
                if (availableEnemyFleetsNodes.Count < minFleetsCount)
                {
                    minFleetsCount = availableEnemyFleetsNodes.Count;
                    minFleetsNode = node;
                }
                if (availableEnemyBasesNodes.Count > maxBasesCount)
                {
                    maxBasesCount = availableEnemyBasesNodes.Count;
                    maxBasesNode = node;
                }
                if (availableEnemyBasesNodes.Count < minBasesCount)
                {
                    minBasesCount = availableEnemyBasesNodes.Count;
                    minBasesNode = node;
                }
                for (int j = 0; j < 2; j++)
                {
                    int sign = j == 0 ? 1 : -1;
                    var posA = new Vector2(node.Position.x - halfWidth, node.Position.y + halfHeight * (1 - sign) - j * TacticalMapCreator.VerticalOffset * 2f * TacticalMapCreator.TacticToWorldMapScale);
                    var posB = new Vector2(node.Position.x + halfWidth, node.Position.y + halfHeight * (1 - sign) - j * TacticalMapCreator.VerticalOffset * 2f * TacticalMapCreator.TacticToWorldMapScale);

                    AddNode(borderNodes, posA, tacticMapOffset, pathNodes);
                    AddNode(borderNodes, posB, tacticMapOffset, pathNodes);

                    for (int i = 1; i < 17; i++)
                    {
                        var pos = Vector2.Lerp(posA, posB, i / 17f);
                        AddNode(borderNodes, pos, tacticMapOffset, pathNodes);
                    }
                    posA = new Vector2(node.Position.x + halfWidth * sign, node.Position.y + halfHeight * 2f - TacticalMapCreator.VerticalOffset * 2f * TacticalMapCreator.TacticToWorldMapScale);
                    posB = new Vector2(node.Position.x + halfWidth * sign, node.Position.y);
                    for (int i = 1; i < 10; i++)
                    {
                        var pos = Vector2.Lerp(posA, posB, i / 10f);
                        AddNode(borderNodes, pos, tacticMapOffset, pathNodes);
                    }
                }
                //foreach (var n in borderNodes)
                //{
                //    var o = GameObject.Instantiate(worldMap.dotPrefab, worldMap.transform);
                //    o.GetComponent<RectTransform>().anchoredPosition = n;
                //}


                UpdateBigNodes(availableEnemyBasesOnWaterNodes, availableEnemyFleetsNodes, borderNodes, basesOnWaterNodesDict, fleetNodesDict, edgeNodesDict, tacticMapOffset);
                startNodeList.Clear();
                distancesList.Clear();

                foreach (var enemyNode in fleetNodesDict.Keys)
                {
                    if ((enemyNode.Position - nodeTacticMapPoint).sqrMagnitude < worldMap.StartNodesDistanceSqr)
                    {
                        startNodeList.Add(fleetNodesDict[enemyNode]);
                        distancesList.Add(new List<float>());
                        for (int i = 0; i < availableEnemyFleetsNodes.Count; i++)
                        {
                            distancesList[distancesList.Count - 1].Add(-1);
                        }
                        distancesList.Add(new List<float>());
                        for (int i = 0; i < availableEnemyBasesOnWaterNodes.Count; i++)
                        {
                            distancesList[distancesList.Count - 1].Add(-1);
                        }
                        distancesList.Add(new List<float>());
                        for (int i = 0; i < borderNodes.Count; i++)
                        {
                            distancesList[distancesList.Count - 1].Add(-1);
                        }
                    }
                }
                CalculatePaths(startNodeList, distancesList, basesOnWaterNodesDict, fleetNodesDict, edgeNodesDict, availableEnemyFleetsNodes);


                worldMap.NodeMaps.NodeDatas.Add(new SandboxNodeMapsData());
                for (int i = 0; i < (int)ESandboxObjectiveType.Count; i++)
                {
                    var data = spawner.CanSpawnMission((ESandboxObjectiveType)i, availableEnemyBasesNodes, availableEnemyBasesOnWaterNodes, availableEnemyFleetsNodes, borderNodes, node.Position, startNodeList, distancesList);
                    if (data != null)
                    {
                        worldMap.NodeMaps.NodeDatas[index - 1].Maps.Add(data);
                    }
                }
                borderNodes.Clear();
                availableEnemyFleetsNodes.Clear();
                availableEnemyBasesNodes.Clear();
                availableEnemyBasesOnWaterNodes.Clear();
                //if (index == 1)
                //{
                //    break;
                //}
                index++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.ClearProgressBar();
            return;
        }
        EditorUtility.SetDirty(worldMap.NodeMaps);
        Debug.Log($"Min fleets node - {minFleetsNode} : {minFleetsCount}");
        Debug.Log($"Max fleets node - {maxFleetsNode} : {maxFleetsCount}");
        Debug.Log($"Max Bases node - {maxBasesNode} : {maxBasesCount}");
        Debug.Log($"Min Bases node - {minBasesNode} : {minBasesCount}");
        //EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log($"Time passed: ({(DateTime.Now - now).TotalSeconds}");
        EditorUtility.ClearProgressBar();
    }

    private static void UpdateBigNodes(List<Vector2> outpostOnWaterNodes, List<Vector2> fleetNodes, List<Vector2> edgeNodes, Dictionary<PathNode, int> outpostOnWaterNodesDict,
        Dictionary<PathNode, int> fleetNodesDict, Dictionary<PathNode, int> edgeNodesDict, Vector2 tacticMapOffset)
    {
        outpostOnWaterNodesDict.Clear();
        fleetNodesDict.Clear();
        edgeNodesDict.Clear();
        for (int index = 0; index < outpostOnWaterNodes.Count; index++)
        {
            var pos = TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(outpostOnWaterNodes[index], tacticMapOffset);
            PathNode pathNode = CurrentMapGrid.Find(pos);
            outpostOnWaterNodesDict[pathNode] = index;
        }
        for (int index = 0; index < fleetNodes.Count; index++)
        {
            var pos = TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(fleetNodes[index], tacticMapOffset);
            PathNode pathNode = CurrentMapGrid.Find(pos);
            fleetNodesDict[pathNode] = index;
        }
        for (int index = 0; index < edgeNodes.Count; index++)
        {
            var pos = TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(edgeNodes[index], tacticMapOffset);
            PathNode pathNode = CurrentMapGrid.Find(pos);
            edgeNodesDict[pathNode] = index;
        }
    }

    private static void AddNode(List<Vector2> hashSet, Vector2 pos, Vector2 tacticMapOffset, HashSet<PathNode> pathNodes)
    {
        if (HasPath(TacticalMapCreator.TransformWorldMapPointToTacticMapPoint(pos, tacticMapOffset), pathNodes))
        {
            hashSet.Add(pos);
        }
    }

    private static void SetupPathNodes(Vector2 from, HashSet<PathNode> pathNodes, List<PathNode> nodeList, SandboxNode sandboxNode)
    {
        pathNodes.Clear();
        nodeList.Clear();
        PathNode startNode = CurrentMapGrid.Find(from);
        if (startNode.IsOnLand)
        {
            Debug.LogError(sandboxNode.name + " is on land! | " + from + " | " + sandboxNode.Position + " | " + startNode.Position);
        }
        nodeList.Add(startNode);
        while (nodeList.Count > 0)
        {
            var node = nodeList[nodeList.Count - 1];
            nodeList.RemoveAt(nodeList.Count - 1);
            foreach (var neighbour in node.GetNeighbourList(CurrentMapGrid))
            {
                if (!neighbour.IsOnLand && !pathNodes.Contains(neighbour) && node != neighbour)
                {
                    pathNodes.Add(neighbour);
                    nodeList.Add(neighbour);
                }
            }
        }
    }

    private static bool HasPath(Vector2 to, HashSet<PathNode> pathNodes)
    {
        if (Mathf.Abs(to.x) > (TacticalMapCreator.GameResolution.x / 2f) - TacticalMapCreator.HorizontalOffset ||
            to.y > TacticalMapCreator.GameResolution.y / 2f - TacticalMapCreator.VerticalOffset * 2f - 1f || to.y < -TacticalMapCreator.GameResolution.y / 2f)
        {
            return false;
        }
        PathNode endNode = CurrentMapGrid.Find(to);
        if (pathNodes.Contains(endNode))
        {
            return true;
        }
        return false;
    }

    private static void CalculatePaths(List<int> startNodeList, List<List<float>> distances, Dictionary<PathNode, int> outpostOnWaterNodesDict, Dictionary<PathNode, int> fleetNodesDict,
        Dictionary<PathNode, int> edgeNodesDict, List<Vector2> enemyFleetsNodes)
    {
        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closeList = new HashSet<PathNode>();
        for (int i = 0; i < startNodeList.Count; i++)
        {
            openList.Clear();
            openList.Add(CurrentMapGrid.Find(enemyFleetsNodes[startNodeList[i]]));
            openList[0].CameFromNode = null;
            openList[0].StartNodeDistance = 0f;
            closeList.Clear();
            while (openList.Count > 0)
            {
                var currentNode = openList[openList.Count - 1];
                openList.RemoveAt(openList.Count - 1);
                closeList.Add(currentNode);

                foreach (PathNode newNode in currentNode.GetNeighbourList(CurrentMapGrid))
                {
                    if (!closeList.Add(newNode))
                    {
                        continue;
                    }

                    if (newNode.IsOnLand)
                    {
                        continue;
                    }

                    newNode.CameFromNode = currentNode;
                    newNode.StartNodeDistance = newNode.CameFromNode.StartNodeDistance + TacticalMapGrid.CalculateRealDistance(currentNode.MapSNode, newNode.MapSNode);
                    int index = openList.BinarySearch(newNode, PathNodeComparer);
                    if (index < 0)
                    {
                        openList.Insert(~index, newNode);
                    }
                    else
                    {
                        openList.Insert(index, newNode);
                    }
                    bool assigned = false;
                    if (fleetNodesDict.TryGetValue(newNode, out int nodeIndex))
                    {
                        //int newStartNodeIndex = startNodeList.IndexOf(nodeIndex);
                        //if (newStartNodeIndex != -1)
                        //{
                        //    var cameNode = newNode.CameFromNode;
                        //    while (cameNode != null)
                        //    {
                        //        if (fleetNodesDict.TryGetValue(cameNode, out int nodeIndexA))
                        //        {
                        //            if (distances[newStartNodeIndex * 3][nodeIndexA] == -1)
                        //            {
                        //                distances[newStartNodeIndex * 3][nodeIndexA] = newNode.StartNodeDistance - cameNode.StartNodeDistance;
                        //            }
                        //        }
                        //        if (outpostOnWaterNodesDict.TryGetValue(cameNode, out nodeIndexA))
                        //        {
                        //            if (distances[1 + newStartNodeIndex * 3][nodeIndexA] == -1)
                        //            {
                        //                distances[1 + newStartNodeIndex * 3][nodeIndexA] = newNode.StartNodeDistance - cameNode.StartNodeDistance;
                        //            }
                        //        }
                        //        if (edgeNodesDict.TryGetValue(cameNode, out nodeIndexA))
                        //        {
                        //            if (distances[2 + newStartNodeIndex * 3][nodeIndexA] == -1)
                        //            {
                        //                distances[2 + newStartNodeIndex * 3][nodeIndexA] = newNode.StartNodeDistance - cameNode.StartNodeDistance;
                        //            }
                        //        }
                        //        cameNode = cameNode.CameFromNode;
                        //    }
                        //}
                        if (distances[i * 3][nodeIndex] == -1)
                        {
                            assigned = true;
                            distances[i * 3][nodeIndex] = newNode.StartNodeDistance;
                        }
                    }
                    if (outpostOnWaterNodesDict.TryGetValue(newNode, out nodeIndex))
                    {
                        if (distances[1 + i * 3][nodeIndex] == -1)
                        {
                            if (assigned)
                            {
                                Debug.Log("Multiple node!");
                            }
                            assigned = true;
                            distances[1 + i * 3][nodeIndex] = newNode.StartNodeDistance;
                        }
                    }
                    if (edgeNodesDict.TryGetValue(newNode, out nodeIndex))
                    {
                        if (distances[2 + i * 3][nodeIndex] == -1)
                        {
                            if (assigned)
                            {
                                Debug.Log("Multiple node!");
                            }
                            assigned = true;
                            distances[2 + i * 3][nodeIndex] = newNode.StartNodeDistance;
                        }
                    }
                }
            }
        }
        return;
    }

    [MenuItem("Tools/Sandbox/SetupTerritoryNodes")]
    private static void SetupTerritoryNodes()
    {
        var worldMap = SceneUtils.FindObjectOfType<WorldMap>();
        var zonesTexture = worldMap.EnemyZonesColorsTexture;

        Color32 currentColor = Color.green;
        Color32 nextColor = Color.green;
        Dictionary<int, Dictionary<int, int>> nodeNeighbours = new Dictionary<int, Dictionary<int, int>>();
        Dictionary<int, TerritoryNode> territoryNodesDict = new Dictionary<int, TerritoryNode>();
        try
        {
            for (int x = 0; x < zonesTexture.width; x++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Territory nodes init", $"In progress...{x}/{zonesTexture.width}", (float)x / zonesTexture.width))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                bool first = true;
                for (int y = 0; y < zonesTexture.height; y++)
                {
                    nextColor = zonesTexture.GetPixel(x, y);
                    if (nextColor != Color.white && nextColor.a == 255f && nextColor.g == 0f && nextColor.b == 0f && nextColor.r < 140f)
                    {
                        if (nextColor.r != currentColor.r)
                        {
                            if (!nodeNeighbours.ContainsKey(nextColor.r))
                            {
                                nodeNeighbours[nextColor.r] = new Dictionary<int, int>();
                            }
                            if (first)
                            {
                                currentColor = nextColor;
                                first = false;
                                continue;
                            }
                            if (!nodeNeighbours[currentColor.r].ContainsKey(nextColor.r))
                            {
                                nodeNeighbours[currentColor.r][nextColor.r] = 0;
                            }
                            nodeNeighbours[currentColor.r][nextColor.r]++;
                            currentColor = nextColor;
                        }
                    }
                    else
                    {
                        first = true;
                    }
                }
            }
            foreach (var pair in nodeNeighbours)
            {
                foreach (var neighbour in pair.Value)
                {
                    if (!nodeNeighbours.ContainsKey(neighbour.Key))
                    {
                        nodeNeighbours[neighbour.Key] = new Dictionary<int, int>();
                    }
                    if (!nodeNeighbours[neighbour.Key].ContainsKey(pair.Key))
                    {
                        nodeNeighbours[neighbour.Key][pair.Key] = 0;
                    }
                    nodeNeighbours[neighbour.Key][pair.Key] += neighbour.Value;
                }
            }
            var colors = zonesTexture.GetPixels32();
            List<Vector2> tempPositions = new List<Vector2>();
            float scale = zonesTexture.width / TacticalMapCreator.GameResolution.x;
            for (int i = 0; i < 140; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Territory nodes position setup", $"In progress...{i}/{255}", (float)i / 255f))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                bool found = false;
                tempPositions.Clear();
                for (int j = 0; j < colors.Length; j++)
                {
                    if (colors[j].r == i)
                    {
                        found = true;
                        int posX = j % zonesTexture.width;
                        int poxY = j / zonesTexture.width;
                        //float scale = zonesTexture.width / TacticalMapCreator.GameResolution.x;
                        tempPositions.Add(new Vector2(posX, poxY + 1));
                    }
                }
                if (!found)
                {
                    break;
                }
                Vector2 nodePosition = Vector2.zero;
                Vector2 minPos = tempPositions[0];
                Vector2 maxPos = tempPositions[0];
                foreach (var pos in tempPositions)
                {
                    minPos = new Vector2(Mathf.Min(minPos.x, pos.x), Mathf.Min(minPos.y, pos.y));
                    maxPos = new Vector2(Mathf.Max(maxPos.x, pos.x), Mathf.Max(maxPos.y, pos.y));
                    //nodePosition += pos;
                }
                //nodePosition /= tempPositions.Count;
                nodePosition = (minPos + maxPos) / 2f;
                nodePosition /= scale;
                nodePosition -= TacticalMapCreator.GameResolution / 2f;
                territoryNodesDict[i] = new TerritoryNode(nodePosition);
            }
            worldMap.TerritoryNodes.Clear();
            foreach (var pair in territoryNodesDict)
            {
                worldMap.TerritoryNodes.Add(null);
            }
            int index = 0;
            var set = new HashSet<int>();
            foreach (var pair in territoryNodesDict)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Territory nodes list setup", $"In progress...{index}/{territoryNodesDict.Count}", (float)index / (float)territoryNodesDict.Count))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                set.Clear();
                foreach (var neighbour in nodeNeighbours[pair.Key])
                {
                    if (neighbour.Value > 10)
                    {
                        set.Add(neighbour.Key);
                    }
                }
                pair.Value.SetNeighbours(set);
                worldMap.TerritoryNodes[pair.Key] = pair.Value;
                index++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.ClearProgressBar();
            return;
        }
        EditorUtility.SetDirty(worldMap);
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Sandbox/CheckObjectives")]
    private static void CheckObjectives()
    {
        var datas = AssetDatabase.LoadAssetAtPath<SandboxNodeMaps>("Assets/GameplayAssets/ScriptableData/SandboxScriptables/SandboxNodeMaps.asset");
        List<int> objectives = new List<int>();
        for (int i = 0; i < (int)ESandboxObjectiveType.Count; i++)
        {
            objectives.Add(0);
        }
        foreach (var data in datas.NodeDatas)
        {
            foreach (var objective in data.Maps)
            {
                objectives[(int)objective.Type]++;
            }
        }
        for (int j = 0; j < objectives.Count; j++)
        {
            Debug.Log((ESandboxObjectiveType)j + " count = " + objectives[j]);
        }
    }

    [MenuItem("Tools/Sandbox/CreateRedWatersMaskTextureTest")]
    private static void CreateRedWatersMaskTextureTest()
    {
        var worldMap = SceneUtils.FindObjectOfType<WorldMap>();
        var zonesTexture = worldMap.EnemyZonesColorsTexture;
        //var targetTexture = new Texture2D(zonesTexture.width, zonesTexture.height);
        var colors = zonesTexture.GetPixels32();
        for (int i = 0; i < colors.Length; i++)
        {
            if (worldMap.TerritoryNodes.Count > colors[i].r && worldMap.TerritoryNodes[colors[i].r].TerritoryType != ETerritoryType.RedWaters)
            {
                colors[i].a = 0;
            }
            colors[i].r = 255;
            colors[i].g = 255;
            colors[i].b = 255;
        }
        worldMap.EnemyZonesTargetTexture.SetPixels32(colors);
        worldMap.EnemyZonesTargetTexture.Apply();
    }
}
