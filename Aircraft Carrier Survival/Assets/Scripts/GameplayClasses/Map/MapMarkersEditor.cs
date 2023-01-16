//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.UI;

//public class MapMarkersEditor : MonoBehaviour
//{
//    [Header("Marker Parents")]
//    public Transform enemyMarkersParent;
//    public Transform weatherMarkersParent;
//    public Transform objectiveMarkersParent;
//    public Transform playerMarkerParent;

//    private MapMarkersData markersData;
//    [Header("Refs")]
//    public RectTransform mapRectTransform;

//    [Header("Prefabs")]
//    public GameObject enemyMarkerPrefab;
//    public GameObject weatherMarkerPrefab;
//    public GameObject objectiveMarkerPrefab;
//    public GameObject playerMarkerPrefab;

//    [Space(10)]
//    public List<GameObject> enemyOrderedList = new List<GameObject>();
//    public List<GameObject> cloudOrderedList = new List<GameObject>();
//    public List<GameObject> objectiveOrderedList = new List<GameObject>();

//    [Space(10)]
//    public string filePath = "Assets/Resources/";
//    public string fileName = "mapdata";
//    private string extension = ".bytes";

//    public Canvas canvas;

//    public void SaveMarkersPositioning()
//    {
//        if (!ValidateEnemyOrderedList())
//        {
//            Debug.LogError("Enemy list validation failed.");
//            return;
//        }

//        if (!ValidateWeatherOrderedList())
//        {
//            Debug.LogError("Cloud list validation failed.");
//            return;
//        }

//        if (!ValidateObjectiveOrderedList())
//        {
//            Debug.LogError("Cloud list validation failed.");
//            return;
//        }

//        if (!ValidatePlayerMarker())
//        {
//            Debug.LogError("Player marker validation failed.");
//            return;
//        }

//        MapMarkersData mapData = new MapMarkersData();

//        foreach (GameObject mark in enemyOrderedList)
//        {
//            float xValue = mark.GetComponent<RectTransform>().anchoredPosition.x / mapRectTransform.rect.width;
//            float yValue = mark.GetComponent<RectTransform>().anchoredPosition.y / mapRectTransform.rect.height;
//            mapData.enemyMarkersList.Add(new MarkerData(new Vector3(xValue, yValue), 0f));
//        }

//        foreach (GameObject mark in cloudOrderedList)
//        {
//            float xValue = mark.GetComponent<RectTransform>().anchoredPosition.x / mapRectTransform.rect.width;
//            float yValue = mark.GetComponent<RectTransform>().anchoredPosition.y / mapRectTransform.rect.height;
//            mapData.cloudMarkersList.Add(new MarkerData(new Vector3(xValue, yValue), 0f));
//        }

//        foreach (GameObject mark in objectiveOrderedList)
//        {
//            float xValue = mark.GetComponent<RectTransform>().anchoredPosition.x / mapRectTransform.rect.width;
//            float yValue = mark.GetComponent<RectTransform>().anchoredPosition.y / mapRectTransform.rect.height;
//            EditorMapMarker emm = mark.GetComponent<EditorMapMarker>();
//            //mapData.objectiveMarkersList.Add(new MarkerData(new Vector3(xValue, yValue), emm.objectiveDatas));
//        }

//        float playerXValue = playerMarkerParent.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x / mapRectTransform.rect.width;
//        float playerYValue = playerMarkerParent.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y / mapRectTransform.rect.height;
//        mapData.playerData = new MarkerData(new Vector3(playerXValue, playerYValue), 0f);

//        SaveMarkers(mapData);
//    }

//    public MapMarkersData LoadMarkers()
//    {
//        string path = filePath + fileName + extension;
//        if (File.Exists(path))
//        {
//            return BinUtils.LoadBinary<MapMarkersData>(path);
//        }
//        return null;
//    }

//    public void SaveMarkers(MapMarkersData mapData)
//    {
//        string path = filePath + fileName + extension;
//        BinUtils.SaveBinary(mapData, path);
//        Debug.Log("MapData saved as " + path);
//    }

//    public void LoadMarkersPositioning()
//    {
//        var newMarkersData = LoadMarkers();
//        if (newMarkersData == null)
//        {
//            Debug.LogError(filePath + fileName + extension + " doesn't exist");
//            return;
//        }
//        markersData = newMarkersData;
//        ClearEnemyMarkers();
//        ClearWeatherMarkers();
//        ClearObjectiveMarkers();
//        ClearPlayerMarker();
//        SpawnMarkers(markersData);
//    }

//    public void SpawnMarkers(MapMarkersData mapData)
//    {
//        SpawnEnemyMarkers(mapData);
//        SpawnWeatherMarkers(mapData);
//        SpawnObjectiveMarkers(mapData);
//        SpawnPlayerMarkerFromData(mapData);
//    }

//    public void SpawnPlayerMarker()
//    {
//        if (playerMarkerParent.childCount > 0)
//        {
//            Debug.Log("Player marker already exists.");
//            return;
//        }
//        GameObject marker = GameObject.Instantiate(playerMarkerPrefab, playerMarkerParent);
//        marker.name = "Player";
//        Text markerText = marker.transform.GetChild(0).GetComponent<Text>();
//        markerText.gameObject.SetActive(true);
//        markerText.text = marker.name;
//    }

//    public void SpawnPlayerMarkerFromData(MapMarkersData mapData)
//    {
//        MarkerData playerData = mapData.playerData;
//        float xValue = playerData.xPos * mapRectTransform.sizeDelta.x;
//        float yValue = playerData.yPos * mapRectTransform.sizeDelta.y;
//        Vector3 vec = new Vector3(xValue, yValue, 0f);
//        GameObject marker = GameObject.Instantiate(playerMarkerPrefab, vec, Quaternion.identity, playerMarkerParent);
//        marker.GetComponent<RectTransform>().anchoredPosition = vec;
//        marker.name = "Player";
//        Text markerText = marker.transform.GetChild(0).GetComponent<Text>();
//        markerText.gameObject.SetActive(true);
//        markerText.text = marker.name;
//    }

//    private void SpawnEnemyMarkers(MapMarkersData mapData)
//    {
//        List<MarkerData> enemyMarkerList = mapData.enemyMarkersList;

//        if (enemyMarkerList.Count < 1)
//            return;

//        int enemyOrder = 0;
//        foreach (MarkerData mm in enemyMarkerList)
//        {
//            float xValue = mm.xPos * mapRectTransform.sizeDelta.x;
//            float yValue = mm.yPos * mapRectTransform.sizeDelta.y;
//            Vector3 vec = new Vector3(xValue, yValue, 0f);
//            GameObject marker = GameObject.Instantiate(enemyMarkerPrefab, vec, Quaternion.identity, enemyMarkersParent);
//            marker.GetComponent<RectTransform>().anchoredPosition = vec;
//            marker.name = "Enemy#" + enemyOrder;
//            Text markerText = marker.transform.GetChild(1).GetComponent<Text>();
//            markerText.gameObject.SetActive(true);
//            markerText.text = marker.name;
//            enemyOrder++;
//            enemyOrderedList.Add(marker);
//        }
//    }

//    private void SpawnWeatherMarkers(MapMarkersData mapData)
//    {
//        List<MarkerData> cloudMarkersList = mapData.cloudMarkersList;

//        if (cloudMarkersList.Count < 1)
//            return;

//        int cloudOrder = 0;
//        foreach (MarkerData mm in cloudMarkersList)
//        {
//            float xValue = mm.xPos * mapRectTransform.sizeDelta.x;
//            float yValue = mm.yPos * mapRectTransform.sizeDelta.y;
//            Vector3 vec = new Vector3(xValue, yValue, 0f);
//            GameObject marker = GameObject.Instantiate(weatherMarkerPrefab, vec, Quaternion.identity, weatherMarkersParent);
//            marker.GetComponent<RectTransform>().anchoredPosition = vec;
//            marker.name = "Cloud#" + cloudOrder;
//            Text markerText = marker.transform.GetChild(1).GetComponent<Text>();
//            markerText.gameObject.SetActive(true);
//            markerText.text = marker.name;
//            cloudOrder++;
//            cloudOrderedList.Add(marker);
//        }
//    }

//    private void SpawnObjectiveMarkers(MapMarkersData mapData)
//    {
//        List<MarkerData> objectiveMarkersList = mapData.objectiveMarkersList;

//        if (objectiveMarkersList.Count < 1)
//            return;

//        int objectiveOrder = 0;
//        foreach (MarkerData mm in objectiveMarkersList)
//        {
//            float xValue = mm.xPos * mapRectTransform.sizeDelta.x;
//            float yValue = mm.yPos * mapRectTransform.sizeDelta.y;
//            Vector3 vec = new Vector3(xValue, yValue, 0f);
//            GameObject marker = GameObject.Instantiate(objectiveMarkerPrefab, vec, Quaternion.identity, objectiveMarkersParent);
//            marker.GetComponent<RectTransform>().anchoredPosition = vec;
//            EditorMapMarker editorMarker = marker.GetComponent<EditorMapMarker>();
//            //editorMarker.objectiveDatas = mm.objectiveDataList;

//            marker.name = "Objective#" + objectiveOrder;
//            Text markerText = marker.transform.GetChild(0).GetComponent<Text>();
//            markerText.gameObject.SetActive(true);
//            markerText.text = marker.name;
//            objectiveOrder++;
//            objectiveOrderedList.Add(marker);
//        }
//    }

//    public void ClearEnemyMarkers()
//    {
//        if (enemyMarkersParent.transform.childCount < 1)
//        {
//            Debug.LogWarning("No enemy marks to remove");
//            return;
//        }

//        List<GameObject> gameObjects = new List<GameObject>();
//        foreach (Transform tf in enemyMarkersParent.transform)
//        {
//            gameObjects.Add(tf.gameObject);
//        }

//        foreach (GameObject go in gameObjects)
//        {
//            DestroyImmediate(go);
//        }
//        enemyOrderedList.Clear();
//    }

//    public void ClearWeatherMarkers()
//    {
//        if (weatherMarkersParent.transform.childCount < 1)
//        {
//            Debug.LogWarning("No weather marks to remove");
//            return;
//        }

//        List<GameObject> gameObjects = new List<GameObject>();
//        foreach (Transform tf in weatherMarkersParent.transform)
//        {
//            gameObjects.Add(tf.gameObject);
//        }

//        foreach (GameObject go in gameObjects)
//        {
//            DestroyImmediate(go);
//        }
//        cloudOrderedList.Clear();
//    }

//    public void ClearObjectiveMarkers()
//    {
//        if (objectiveMarkersParent.transform.childCount < 1)
//        {
//            Debug.LogWarning("No objective marks to remove");
//            return;
//        }

//        List<GameObject> gameObjects = new List<GameObject>();
//        foreach (Transform tf in objectiveMarkersParent.transform)
//        {
//            gameObjects.Add(tf.gameObject);
//        }

//        foreach (GameObject go in gameObjects)
//        {
//            DestroyImmediate(go);
//        }
//        objectiveOrderedList.Clear();
//    }

//    public void ClearPlayerMarker()
//    {
//        if (playerMarkerParent.childCount < 1)
//            return;

//        List<GameObject> gameObjects = new List<GameObject>();
//        foreach (Transform tf in playerMarkerParent.transform)
//        {
//            gameObjects.Add(tf.gameObject);
//        }

//        foreach (GameObject go in gameObjects)
//        {
//            DestroyImmediate(go);
//        }
//    }

//    public void SpawnEnemyMarker()
//    {
//        GameObject marker = GameObject.Instantiate(enemyMarkerPrefab, enemyMarkersParent);
//        int nextID = enemyMarkersParent.childCount - 1;
//        marker.name = "Enemy#" + nextID;
//        Text markerText = marker.transform.GetChild(1).GetComponent<Text>();
//        markerText.gameObject.SetActive(true);
//        markerText.text = marker.name;
//        enemyOrderedList.Add(marker);
//    }

//    public void SpawnWeatherMarker()
//    {
//        GameObject marker = GameObject.Instantiate(weatherMarkerPrefab, weatherMarkersParent);
//        int nextID = enemyMarkersParent.childCount - 1;
//        marker.name = "Cloud#" + nextID;
//        Text markerText = marker.transform.GetChild(1).GetComponent<Text>();
//        markerText.gameObject.SetActive(true);
//        markerText.text = marker.name;
//        cloudOrderedList.Add(marker);
//    }

//    public void SpawnObjectiveMarker()
//    {
//        GameObject marker = GameObject.Instantiate(objectiveMarkerPrefab, objectiveMarkersParent);
//        int nextID = objectiveMarkersParent.childCount - 1;
//        marker.name = "Objective#" + nextID;
//        Text markerText = marker.transform.GetChild(0).GetComponent<Text>();
//        markerText.gameObject.SetActive(true);
//        markerText.text = marker.name;
//        objectiveOrderedList.Add(marker);
//    }

//    public void RefreshOrderedList()
//    {
//        int i = 0, j = 0, k = 0;
//        while (i < enemyOrderedList.Count)
//        {
//            if (enemyOrderedList[i] == null)
//            {
//                enemyOrderedList.RemoveAt(i);
//                continue;
//            }
//            i++;
//        }
//        while (j < cloudOrderedList.Count)
//        {
//            if (cloudOrderedList[j] == null)
//            {
//                cloudOrderedList.RemoveAt(j);
//                continue;
//            }
//            j++;
//        }
//        while (k < objectiveOrderedList.Count)
//        {
//            if (objectiveOrderedList[k] == null)
//            {
//                objectiveOrderedList.RemoveAt(k);
//                continue;
//            }
//            k++;
//        }
//    }

//    private bool ValidateEnemyOrderedList()
//    {
//        if (enemyOrderedList.Count < 1)
//        {
//            Debug.Log("There are no elements in the enemy order list");
//            return false;
//        }

//        for (int i = 0; i < enemyOrderedList.Count; i++)
//        {
//            if (enemyOrderedList[i] == null)
//            {
//                Debug.Log("Enemy list element #" + i + " is null");
//                return false;
//            }
//        }

//        if (enemyOrderedList.Count != enemyMarkersParent.childCount)
//        {
//            Debug.Log("Marker counts in enemy ordered list and on map are different. Add missing markers to enemy ordered list or remove unused markers from EnemyMarksParent");
//            return false;
//        }

//        for (int i = 0; i < enemyOrderedList.Count - 1; i++)
//        {
//            for (int j = i + 1; j < enemyOrderedList.Count; j++)
//            {
//                if (enemyOrderedList[i] == enemyOrderedList[j])
//                {
//                    Debug.Log("Enemy list element #" + j + " is duplicate of list element #" + i);
//                    return false;
//                }
//            }
//        }
//        return true;
//    }

//    private bool ValidateWeatherOrderedList()
//    {
//        if (cloudOrderedList.Count < 1)
//        {
//            Debug.Log("There are no elements in the cloud order list");
//            return false;
//        }

//        for (int i = 0; i < cloudOrderedList.Count; i++)
//        {
//            if (cloudOrderedList[i] == null)
//            {
//                Debug.Log("Cloud list element #" + i + " is null");
//                return false;
//            }
//        }

//        if (cloudOrderedList.Count != weatherMarkersParent.childCount)
//        {
//            Debug.Log("Marker counts in cloud ordered list and on map are different. Add missing markers to cloud ordered list or remove unused markers from WeatherMarksParent");
//            return false;
//        }

//        for (int i = 0; i < cloudOrderedList.Count - 1; i++)
//        {
//            for (int j = i + 1; j < cloudOrderedList.Count; j++)
//            {
//                if (cloudOrderedList[i] == cloudOrderedList[j])
//                {
//                    Debug.Log("Cloud list element #" + j + " is duplicate of list element #" + i);
//                    return false;
//                }
//            }
//        }
//        return true;
//    }

//    private bool ValidateObjectiveOrderedList()
//    {
//        if (objectiveOrderedList.Count < 1)
//        {
//            Debug.Log("There are no elements in the objective order list");
//            return false;
//        }

//        for (int i = 0; i < objectiveOrderedList.Count; i++)
//        {
//            if (objectiveOrderedList[i] == null)
//            {
//                Debug.Log("Objective list element #" + i + " is null");
//                return false;
//            }
//        }

//        if (objectiveOrderedList.Count != objectiveMarkersParent.childCount)
//        {
//            Debug.Log("Marker counts in objective ordered list and on map are different. Add missing markers to objective ordered list or remove unused markers from ObjectiveMarksParent");
//            return false;
//        }

//        for (int i = 0; i < objectiveOrderedList.Count - 1; i++)
//        {
//            for (int j = i + 1; j < objectiveOrderedList.Count; j++)
//            {
//                if (objectiveOrderedList[i] == objectiveOrderedList[j])
//                {
//                    Debug.Log("Objective list element #" + j + " is duplicate of list element #" + i);
//                    return false;
//                }
//            }
//        }
//        return true;
//    }

//    private bool ValidatePlayerMarker()
//    {
//        if (playerMarkerParent.childCount < 1)
//        {
//            Debug.Log("Player marker's missing.");
//            return false;
//        }
//        if (playerMarkerParent.childCount > 1)
//        {
//            Debug.Log("Too many player markers. Only one allowed.");
//            return false;
//        }
//        return true;
//    }
//}
