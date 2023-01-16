using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SandboxNode : MonoBehaviour
{
    public Vector2 Position => rectTransform.anchoredPosition;
    public RectTransform OnWaterTransform => onWaterTransform;
    public bool CanSpawnPoi => canSpawnPoi;
    public bool CanSpawnRepairSpot => canSpawnRepairSpot;
    public bool CanSpawnBase => canSpawnBase;

    public bool Occupied
    {
        get => occupied;
        set
        {
            occupied = value;
            foreach (var node in neighbours)
            {
                node.CheckDistance();
            }
            CheckDistance();
            SandboxManager.Instance.PoiManager.UpdateLists(!occupied, neighbours);
        }
    }

    public bool BlockedByDistance
    {
        get;
        private set;
    }

    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private RectTransform onWaterTransform = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private bool canSpawnPoi = false;
    [SerializeField]
    private bool canSpawnRepairSpot = false;
    [SerializeField]
    private bool canSpawnBase = false;

    [SerializeField]
    private List<SandboxNode> neighbours = null;

    private bool occupied;

    public void CheckDistance()
    {
        BlockedByDistance = false;
        foreach (var node in neighbours)
        {
            if (node.Occupied)
            {
                BlockedByDistance = true;
            }
        }
    }

    public void SetNeighbours(List<SandboxNode> list)
    {
        neighbours = new List<SandboxNode>(list);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var color = Color.white;
        if (canSpawnPoi)
        {
            color -= Color.red;
        }
        if (canSpawnRepairSpot)
        {
            color -= Color.blue;
        }
        if (canSpawnBase)
        {
            color -= Color.green;
        }
        color = new Color(color.r, color.g, color.b, 1f);
        image.color = color;
    }
#endif
}
