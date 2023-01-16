using System.Collections.Generic;
using UnityEngine;

public class Neighbours : MonoBehaviour
{
    [SerializeField]
    private List<NeighbourData> neighbours = null;

    private void Start()
    {
        var segment = GetComponent<SectionSegment>();
        foreach (NeighbourData data in neighbours)
        {
            if (data.Segment == null)
            {
                Debug.LogError(gameObject);
            }
            else
            {
                Check(segment, data.Segment, data.Direction);
                Check(data.Segment, segment, data.Direction);
                if (!segment.NeighboursDirectionDictionary.ContainsKey(data.Segment))
                {
                    segment.NeighboursDirectionDictionary.Add(data.Segment, data.Direction);
                }
                if (!data.Segment.NeighboursDirectionDictionary.ContainsKey(segment))
                {
                    data.Segment.NeighboursDirectionDictionary.Add(segment, GetOppositeDirection(data.Direction));
                }
                bool horizontal = data.Direction == ENeighbourDirection.Left || data.Direction == ENeighbourDirection.Right;
                if (horizontal)
                {
                    segment.HorizontalNeighbours.Add(data.Segment);
                    data.Segment.HorizontalNeighbours.Add(segment);
                }
                if (horizontal || data.Direction == ENeighbourDirection.FireOnly || data.Direction == ENeighbourDirection.FireAndWater)
                {
                    segment.FireNeighbours.Add(data.Segment);
                    data.Segment.FireNeighbours.Add(segment);
                }
                if (horizontal || data.Direction == ENeighbourDirection.WaterOnly || data.Direction == ENeighbourDirection.FireAndWater)
                {
                    segment.FloodNeighbours.Add(data.Segment);
                    data.Segment.FloodNeighbours.Add(segment);
                }
            }
        }
    }

    private ENeighbourDirection GetOppositeDirection(ENeighbourDirection dir)
    {
        switch (dir)
        {
            case ENeighbourDirection.Up:
                return ENeighbourDirection.Down;
            case ENeighbourDirection.Down:
                return ENeighbourDirection.Up;
            case ENeighbourDirection.Left:
                return ENeighbourDirection.Right;
            case ENeighbourDirection.Right:
                return ENeighbourDirection.Left;
            default:
                return dir;
        }
    }

    private void Check(SectionSegment segment, SectionSegment segment2, ENeighbourDirection direction)
    {
        if (direction == ENeighbourDirection.Left || direction == ENeighbourDirection.Right)
        {
            if (segment.Parent.Path.DCSegmentTransition.TryGetValue(segment, out var dict))
            {
                if (!dict.ContainsKey(segment2))
                {
                    Debug.Log(segment.name + " path missing to " + segment2.name);
                }
            }
            else
            {
                Debug.Log("NO DC SECTION/SEGMENT TRANSITION FOR - " + segment.name + " to " + segment2.name);
            }
        }
    }
}
