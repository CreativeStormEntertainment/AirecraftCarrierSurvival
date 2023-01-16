using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TacticalMapCreator : MonoBehaviour
{
    public static Vector2 HalfGameResolution = new Vector2(960f, 540f);
    public static Vector2 GameResolution = new Vector2(1920f, 1080f);
    public static Vector2 TacticMapResolution = new Vector2(2880f, 1620f);
    public static Vector2 BigMapResolution = new Vector2(15360f, 8640f);

    public static readonly float WorldToTacticScale = BigMapResolution.y / TacticMapResolution.y;
    public static readonly float TacticToWorldMapScale = TacticMapResolution.y / BigMapResolution.y;

    public static readonly float TacticMapHeight = TacticToWorldMapScale * GameResolution.y;
    public static readonly float TacticMapWidth = TacticToWorldMapScale * GameResolution.x;

    public static float VerticalOffset = 100f;
    public static float HorizontalOffset = 100f;

    public static Vector2 TransformTacticMapPointToWorldMapPoint(Vector2 gamePoint, Vector2 tacticMapOffset)
    {
        return gamePoint * TacticToWorldMapScale + tacticMapOffset;
    }

    public static Vector2 TransformWorldMapPointToTacticMapPoint(Vector2 worldMapPoint, Vector2 tacticMapOffset)
    {
        return (worldMapPoint - tacticMapOffset) * WorldToTacticScale;
    }

    public static Vector2 TransformWorldMapPointToBigMapPoint(Vector2 worldMapPoint, out int mapIndex)
    {
        mapIndex = 0;
        Vector2 proportions = worldMapPoint / GameResolution;
        if (proportions.x > 0.5f)
        {
            proportions.x -= 0.5f;
            mapIndex += 1;
        }
        if (proportions.y > 0.5f)
        {
            proportions.y -= 0.5f;
        }
        else
        {
            mapIndex += 2;
        }
        return BigMapResolution * proportions;
    }

    public static Vector2 TransformWorldMapPointTo16kMapPoint(Vector2 worldMapPoint)
    {
        return BigMapResolution * worldMapPoint / GameResolution;
    }
}
