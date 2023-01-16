using UnityEngine;

public class WorldMapSectorNode : MonoBehaviour
{
    public EWorldMapSector Sector => sector;
    public RectTransform RectTransform => rectTransform;

    [SerializeField]
    private EWorldMapSector sector = default;
    [SerializeField]
    private RectTransform rectTransform = null;
}
