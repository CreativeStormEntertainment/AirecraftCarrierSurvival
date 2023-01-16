using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//public class PlaneNodeOld : IslandNode
//{
//    public bool CanBeOccupied;
//    public List<PlaneNodeOld> NodesToCooccupy;
//    public PlaneMovement Occupant;
//    public bool ForcedOccupy;

//    public PlaneNodeOld()
//    {
//        NodesToCooccupy = new List<PlaneNodeOld>();
//    }

//    public PlaneNodeOld(PlaneNodeOld origin)
//    {
//        CanBeOccupied = origin.CanBeOccupied;
//        Position = origin.Position;
//    }

//    public List<PlaneNodeOld> FindPath(PlaneNodeOld dest)
//    {
//        if (this != dest)
//        {
//            var toVisit = new List<PlaneNodeOld>() { this };
//            var toVisitPath = new List<List<PlaneNodeOld>> { new List<PlaneNodeOld>() };
//            var visited = new HashSet<IslandNode>();
//            while (toVisit.Count > 0)
//            {
//                var node = toVisit[0];
//                var nodePath = toVisitPath[0];
//                toVisit.RemoveAt(0);
//                toVisitPath.RemoveAt(0);

//                nodePath.Add(node);

//                if (node == dest)
//                {
//                    return nodePath;
//                }
//                else
//                {
//                    foreach (PlaneNodeOld node2 in node.ConnectedNodes)
//                    {
//                        Assert.IsNotNull(node2);
//                        if (visited.Add(node2))
//                        {
//                            toVisit.Add(node2);
//                            toVisitPath.Add(new List<PlaneNodeOld>(nodePath));
//                        }
//                    }
//                }
//            }
//        }
//        return new List<PlaneNodeOld>();
//    }

//    public bool CanOccupy(PlaneMovement plane)
//    {
//        return (!CanBeOccupied && !ForcedOccupy) || !Occupied || Occupant == plane;
//    }

//    public void SetOccupy(PlaneMovement plane, bool occupy)
//    {
//        Assert.IsNotNull(plane);
//        if (!CanOccupy(plane))
//        {
//            Debug.LogError("Plane can't lock occupy node (plane isWaiting issue?)");
//        }
//        if (CanOccupy(plane) && (Occupied != occupy) && (CanBeOccupied || ForcedOccupy))
//        {
//            Occupied = occupy;

//            if (occupy)
//            {
//                Occupant = plane;
//            }
//            else
//            {
//                Occupant = null;
//                ForcedOccupy = false;
//            }

//            foreach (var node in NodesToCooccupy)
//            {
//                node.SetOccupy(plane, occupy);
//            }
//        }
//    }

//    public void ForceOccupy(PlaneMovement plane)
//    {
//        Assert.IsTrue(NodesToCooccupy.Count == 0);
//        ForcedOccupy = true;
//        SetOccupy(plane, true);
//    }
//}
