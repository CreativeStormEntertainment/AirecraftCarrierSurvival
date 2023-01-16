using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapMarker : MonoBehaviour
{
    public RectTransform RectTransform => rectTransform;

    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private RectTransform rotateTransform = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Sprite uoSprite = null;
    [SerializeField]
    private Sprite enemyFleetSprite = null;
    [SerializeField]
    private Sprite neutralFleetSprite = null;

    public void Setup(EWorldMapFleetType fleetType)
    {
        SetMarkerSprite(fleetType, true);
    }

    public void SetMarkerSprite(EWorldMapFleetType fleetType, bool uo)
    {
        Sprite sprite = null;
        if (uo)
        {
            sprite = uoSprite;
        }
        else
        {
            switch (fleetType)
            {
                case EWorldMapFleetType.Neutral:
                    sprite = neutralFleetSprite;
                    break;
                case EWorldMapFleetType.EnemyFleeing:
                case EWorldMapFleetType.EnemyAggressive:
                    sprite = enemyFleetSprite;
                    break;
            }
        }
        image.sprite = sprite;
        image.SetNativeSize();
        image.rectTransform.sizeDelta /= 2f;
    }

    public void UpdatePositionAndRotation(Vector2 pos, Quaternion rotation)
    {
        rectTransform.anchoredPosition = pos;
        rotateTransform.rotation = rotation;
    }
}
