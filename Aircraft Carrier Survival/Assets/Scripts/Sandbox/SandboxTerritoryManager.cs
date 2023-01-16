using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SandboxTerritoryManager : MonoBehaviour
{
    [SerializeField]
    private int maxRedWatersNodes = 25;

    private Dictionary<ETerritoryType, List<TerritoryNode>> territoryNodeTypesDictionary = new Dictionary<ETerritoryType, List<TerritoryNode>>();
    private Dictionary<ESectorType, List<TerritoryNode>> territoryNodeSectorsDictionary = new Dictionary<ESectorType, List<TerritoryNode>>();

    private Dictionary<ESectorType, List<TerritoryNode>> japanTerritoryNodesDictionary = new Dictionary<ESectorType, List<TerritoryNode>>();
    private Dictionary<ESectorType, List<TerritoryNode>> usaTerritoryNodesDictionary = new Dictionary<ESectorType, List<TerritoryNode>>();

    private WorldMapShip worldMapShip;

    private List<TerritoryNode> tempList = new List<TerritoryNode>();
    private List<TerritoryNode> frontNodesTempList = new List<TerritoryNode>();

    private List<TerritoryNode> territoryNodes = new List<TerritoryNode>();

    public void Save(ref SandboxSaveData data)
    {
        data.TerritoryNodes.Clear();
        foreach (var node in territoryNodes)
        {
            data.TerritoryNodes.Add(node.TerritoryType);
        }
    }

    public void Setup()
    {
        var worldMap = WorldMap.Instance;
        worldMapShip = worldMap.MapShip as WorldMapShip;
        for (int i = 0; i < (int)ETerritoryType.Count; i++)
        {
            List<TerritoryNode> list = new List<TerritoryNode>();
            territoryNodeTypesDictionary.Add((ETerritoryType)i, list);
        }
        for (int i = 0; i < (int)ESectorType.Count; i++)
        {
            List<TerritoryNode> listA = new List<TerritoryNode>();
            List<TerritoryNode> listB = new List<TerritoryNode>();
            List<TerritoryNode> listC = new List<TerritoryNode>();
            //territoryNodeSectorsDictionary.Add((ESectorType)i, listA);
            //japanTerritoryNodesDictionary.Add((ESectorType)i, listB);
            //usaTerritoryNodesDictionary.Add((ESectorType)i, listC);
        }
        territoryNodes = worldMap.TerritoryNodes;
        var saveData = SaveManager.Instance.Data;
        var savedNodes = saveData.SandboxData.TerritoryNodes;
        if (saveData.SandboxData.IsSaved)
        {
            for (int i = 0; i < territoryNodes.Count; i++)
            {
                var node = territoryNodes[i];
                territoryNodeTypesDictionary[savedNodes[i]].Add(node);
                territoryNodes[i].SetTerritoryType(savedNodes[i]);
                //territoryNodeSectorsDictionary[node.Sector].Add(node);
                //if (savedNodes[i] == ETerritoryType.RedWaters)
                //{
                //    japanTerritoryNodesDictionary[node.Sector].Add(node);
                //}
                //else
                //{
                //    usaTerritoryNodesDictionary[node.Sector].Add(node);
                //}
            }
        }
        else
        {
            foreach (var node in territoryNodes)
            {
                territoryNodeTypesDictionary[node.TerritoryType].Add(node);
                node.SetTerritoryType(node.TerritoryType);
                //territoryNodeSectorsDictionary[node.Sector].Add(node);
                //if (node.TerritoryType == ETerritoryType.RedWaters)
                //{
                //    japanTerritoryNodesDictionary[node.Sector].Add(node);
                //}
                //else
                //{
                //    usaTerritoryNodesDictionary[node.Sector].Add(node);
                //}
            }
        }
        worldMapShip.SetClosestNode();
        UpdateRedWatersMaskTexture();
    }

    public void ConquerTerritory(bool succes)
    {
        if (succes)
        {
            var conqueredNode = GetRandomFrontNode(territoryNodeTypesDictionary[ETerritoryType.RedWaters]);
            ConquerNode(conqueredNode);
            var lostNode = GetRandomFrontNode(territoryNodeTypesDictionary[ETerritoryType.USA]);
            LoseNode(lostNode);
        }
        else
        {
            if (territoryNodeTypesDictionary[ETerritoryType.RedWaters].Count > maxRedWatersNodes)
            {
                var conqueredNode = GetRandomFrontNode(territoryNodeTypesDictionary[ETerritoryType.RedWaters]);
                ConquerNode(conqueredNode);
                var lostNode = GetRandomFrontNode(territoryNodeTypesDictionary[ETerritoryType.USA]);
                LoseNode(lostNode);
            }
            else
            {
                var lostNode = GetRandomFrontNode(territoryNodeTypesDictionary[ETerritoryType.USA]);
                LoseNode(lostNode);
            }
        }
        UpdateRedWatersMaskTexture();
    }

    public TerritoryNode GetClosestNode(Vector2 position)
    {
        TerritoryNode closestNode = null;
        float distance = 9999999f;
        foreach (var node in territoryNodes)
        {
            float newDistance = (node.Position - position).sqrMagnitude;
            if (newDistance < distance)
            {
                distance = newDistance;
                closestNode = node;
            }
        }
        return closestNode;
    }

    public TerritoryNode GetClosestNodeToShip()
    {
        TerritoryNode closestNode = null;
        float distance = 9999999f;
        if (worldMapShip == null)
        {
            worldMapShip = WorldMap.Instance.MapShip as WorldMapShip;
        }
        if (worldMapShip.CurrentNode == null)
        {
            foreach (var node in territoryNodes)
            {
                float newDistance = (node.Position - worldMapShip.Rect.anchoredPosition).sqrMagnitude;
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestNode = node;
                }
            }
        }
        else
        {
            foreach (var index in worldMapShip.CurrentNode.Neighbours)
            {
                var node = WorldMap.Instance.TerritoryNodes[index];
                float newDistance = (node.Position - worldMapShip.Rect.anchoredPosition).sqrMagnitude;
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestNode = node;
                }
            }
            var newDist = (worldMapShip.CurrentNode.Position - worldMapShip.Rect.anchoredPosition).sqrMagnitude;
            if (newDist < distance)
            {
                closestNode = worldMapShip.CurrentNode;
            }
        }
        return closestNode;
    }



    private TerritoryNode GetRandomFrontNode(List<TerritoryNode> list)
    {
        frontNodesTempList.Clear();
        frontNodesTempList.AddRange(list);
        for (int i = frontNodesTempList.Count - 1; i >= 0; i--)
        {
            if (!frontNodesTempList[i].IsFrontSector)
            {
                frontNodesTempList.RemoveAt(i);
            }
        }
        return RandomUtils.GetRandom(frontNodesTempList);
    }

    private void ConquerNode(TerritoryNode conqueredNode)
    {
        conqueredNode.SetTerritoryType(ETerritoryType.USA);
        territoryNodeTypesDictionary[ETerritoryType.USA].Add(conqueredNode);
        territoryNodeTypesDictionary[ETerritoryType.RedWaters].Remove(conqueredNode);
        //japanTerritoryNodesDictionary[conqueredNode.Sector].Remove(conqueredNode);
        //usaTerritoryNodesDictionary[conqueredNode.Sector].Add(conqueredNode);
    }

    private void LoseNode(TerritoryNode conqueredNode)
    {
        conqueredNode.SetTerritoryType(ETerritoryType.RedWaters);
        territoryNodeTypesDictionary[ETerritoryType.USA].Remove(conqueredNode);
        territoryNodeTypesDictionary[ETerritoryType.RedWaters].Add(conqueredNode);
        //japanTerritoryNodesDictionary[conqueredNode.Sector].Add(conqueredNode);
        //usaTerritoryNodesDictionary[conqueredNode.Sector].Remove(conqueredNode);
    }
    private void UpdateRedWatersMaskTexture()
    {
        var worldMap = WorldMap.Instance;
        var zonesTexture = worldMap.EnemyZonesColorsTexture;
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
