using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarkersList : MonoBehaviour
{
    [Header("Enemies")]
    public List<MapMarker> enemyMarkers = null;

    [Header("Weather")]
    public List<MapMarker> cloudMarkers = null;
}
