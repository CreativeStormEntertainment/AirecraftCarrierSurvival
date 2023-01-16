using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatrolData
{
    public List<RectTransform> Nodes;
    [HideInInspector]
    public List<int> Poses;

    public PathNode Last => SNodePoses[SNodePoses.Count - 1];

    public List<PathNode> SNodePoses
    {
        get;
        private set;
    }

    public void Init()
    {
        SNodePoses = new List<PathNode>();
        var tMan = TacticManager.Instance;
        foreach (int pos in Poses)
        {
            var node = tMan.MapNodes.Find(pos);
            SNodePoses.Add(node);
        }
    }
}
