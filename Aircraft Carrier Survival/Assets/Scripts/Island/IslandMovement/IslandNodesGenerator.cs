using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class IslandNodesGenerator
{
    public IslandNodesGenerator(Transform mainTransform, Dictionary<EIslandRoomType, IslandRoom> islandRooms, int floorCount)
    {
        if (islandRooms[EIslandRoomType.Bridge].AlternativeNodes == null)
        {
            return;
        }
        IslandNode[] mainNodes = new IslandNode[floorCount];
        foreach (Transform t in mainTransform)
        {
            string floor = t.name.Split('_')[1];
            mainNodes[int.Parse(floor)] = new IslandNode();
            mainNodes[int.Parse(floor)].Position = t.position;
        }
        foreach (Transform t in mainTransform)
        {
            string str = t.name.Split('_')[1];
            int floor = int.Parse(str);
            foreach (Transform t1 in t)
            {
                if (t1.name.StartsWith("cway_"))
                {
                    IslandNode node = new IslandNode();
                    node.Position = t1.position;
                    string f = t1.name.Split('_')[1];

                    node.ConnectedNodes.Add(mainNodes[f[0] - '0']);
                    node.ConnectedNodes.Add(mainNodes[f[1] - '0']);
                    mainNodes[f[0] - '0'].ConnectedNodes.Add(node);
                    mainNodes[f[1] - '0'].ConnectedNodes.Add(node);
                }
                else
                {
                    IslandNode node = new IslandNode();
                    node.Position = t1.position;
                    mainNodes[floor].ConnectedNodes.Add(node);
                    node.ConnectedNodes.Add(mainNodes[floor]);
                    foreach (Transform t2 in t1)
                    {
                        IslandNode node1 = new IslandNode();
                        node1.Position = t2.position;
                        node.ConnectedNodes.Add(node1);
                        node1.ConnectedNodes.Add(node);
                        foreach (Transform t3 in t2)
                        {
                            IslandNode node2 = new IslandNode();
                            node2.Position = t3.position;
                            node2.ConnectedNodes.Add(node1);
                            node1.ConnectedNodes.Add(node2);
                            foreach (Transform t4 in t3)
                            {
                                IslandNode node3 = new IslandNode();
                                node3.Position = t4.position;
                                node2.ConnectedNodes.Add(node3);
                                node3.ConnectedNodes.Add(node2);

                                string roomName = t4.name.Split('_')[1];
                                EIslandRoomType roomType = EIslandRoomType.OperationsRoom;
                                switch (roomName)
                                {
                                    case "PilotsDebriefingRoom":
                                        roomType = EIslandRoomType.PilotDebriefingRoom;
                                        break;
                                    case "OfficersRoom":
                                        roomType = EIslandRoomType.OperationsRoom;
                                        break;
                                    case "WinningRoom":
                                        roomType = EIslandRoomType.CIC;
                                        break;
                                    case "RadioRoom":
                                        roomType = EIslandRoomType.RadioRoom;
                                        break;
                                    case "CodingRoom":
                                        roomType = EIslandRoomType.CodingRoom;
                                        break;
                                    case "MeteoRoom":
                                        roomType = EIslandRoomType.MeteorologyRoom;
                                        break;
                                    case "Bridge":
                                        roomType = EIslandRoomType.Bridge;
                                        break;
                                    case "NavigationRoom":
                                        roomType = EIslandRoomType.NavigationRoom;
                                        break;
                                    case "FlagPlottingRoom":
                                        roomType = EIslandRoomType.FlagPlottingRoom;
                                        break;
                                    case "OrdersRoom":
                                        roomType = EIslandRoomType.OrdersRoom;
                                        break;
                                    case "ExperimentalTactics":
                                        roomType = EIslandRoomType.ExperimentalTactics;
                                        break;
                                    case "ActiveDefenceRoom":
                                        roomType = EIslandRoomType.ActiveDefenceRoom;
                                        break;
                                    default:
                                        Debug.LogError("No such room as: " + roomName);
                                        break;
                                }
                                islandRooms[roomType].MainNode = node3;
                                foreach (Transform t5 in t4)
                                {
                                    IslandNode node4 = new IslandNode();
                                    node4.Position = t5.position;
                                    node4.ConnectedNodes.Add(node3);
                                    node3.ConnectedNodes.Add(node4);
                                    islandRooms[roomType].AlternativeNodes.Add(node4);
                                }
                            }
                        }
                    }
                }

            }

        }
    }

    public IslandNodesGenerator(Transform waypointsTransport)
    {
        IslandNode currentNode = new IslandNode();
        Transform parent = waypointsTransport;
        currentNode.Position = parent.position;
        List<Tuple<Transform, IslandNode>> possibleParents = new List<Tuple<Transform, IslandNode>>();
        while (parent != null)
        {
            foreach (Transform t in parent)
            {
                IslandNode node = new IslandNode();
                node.Position = t.position;
                node.ConnectedNodes.Add(currentNode);
                currentNode.ConnectedNodes.Add(node);
                possibleParents.Add(new Tuple<Transform, IslandNode>(t, node));
            }
            if (possibleParents.Count != 0)
            {
                currentNode = possibleParents[0].Item2;
                parent = possibleParents[0].Item1;
                possibleParents.RemoveAt(0);
            }
            else
            {
                parent = null;
            }
        }
    }
}
