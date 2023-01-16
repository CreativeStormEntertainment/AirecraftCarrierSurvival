//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public class MarkerData
//{
//    public float xPos = 0;
//    public float yPos = 0;
//    public float zPos = 0;
//    public float markerRange = 0;

//    public MarkerData(Vector3 vec, float markerRange)
//    {
//        xPos = vec.x;
//        yPos = vec.y;
//        zPos = vec.z;
//        this.markerRange = markerRange;
//    }
//}

//[Serializable]
//public class MapMarkersData
//{
//    [SerializeField]
//    public List<MarkerData> enemyMarkersList = new List<MarkerData>();
//    [SerializeField]
//    public List<MarkerData> cloudMarkersList = new List<MarkerData>();
//    [SerializeField]
//    public MarkerData playerData;
//    [SerializeField]
//    public List<MarkerData> objectiveMarkersList = new List<MarkerData>();
//}
