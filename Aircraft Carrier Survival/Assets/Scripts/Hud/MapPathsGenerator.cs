using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class MapPathsGenerator
{
    [SerializeField] private List<RectTransform> startPoints;

    public List<MapNode> GeneratePaths(List<RectTransform> pathsRoots)
    {
        List<MapNode> list = new List<MapNode>();
        foreach (RectTransform rect in pathsRoots)
        {
            RectTransform currRect = rect.GetChild(0) as RectTransform;
            MapNode node = new MapNode(currRect.anchoredPosition);
            Vector2 position = Vector2.zero;
            RectTransform nextNode;
            list.Add(node);
            while (currRect != null)
            {
                nextNode = null;
                position += currRect.anchoredPosition;
                foreach (RectTransform t in currRect)
                {
                    if (t.name.StartsWith("Exit"))
                    {
                        node.ExitNode = new MapNode(t.anchoredPosition);
                    }
                    else
                    {
                        nextNode = t;
                        node.NextNode = new MapNode(position + t.anchoredPosition);
                    }
                }
                currRect = nextNode;
                node = node.NextNode;
            }
        }
        return list;
    }
}
