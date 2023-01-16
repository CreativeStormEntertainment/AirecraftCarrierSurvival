using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeComparer : IComparer<PathNode>
{
    public int Compare(PathNode x, PathNode y)
    {
        return Comparer<float>.Default.Compare(y.StartNodeDistance, x.StartNodeDistance);
    }
}
