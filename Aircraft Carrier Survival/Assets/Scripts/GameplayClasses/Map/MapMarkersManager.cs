using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MapMarkersManager : MonoBehaviour
{
    public static MapMarkersManager Instance;
    public RectTransform MapRectTransform;
    public RectTransform CanvasRect;
    public TacticalMapShip MapShip;

    public float BaseSightEffectiveness
    {
        get => baseSightEffectiveness;
        set => baseSightEffectiveness = value;
    }

    public float WeatherKnowledgeGainSpeed
    {
        get => Mathf.Clamp(weatherKnowledgeGainSpeed, 0f, 1f);
        set
        {
            weatherKnowledgeGainSpeed = value;
        }
    }

    [SerializeField]
    private float baseSightEffectiveness = 1f;

    //[SerializeField]
    //private Transform enemyMarkers = null;
    //[SerializeField]
    //private Transform weatherMarkers = null;
    //[SerializeField]
    //private Transform objectiveMarkers = null;

    //[SerializeField]
    //private GameObject enemyMarkerPrefab = null;
    //[SerializeField]
    //private GameObject weatherMarkerPrefab = null;
    //[SerializeField]
    //private GameObject objectiveMarkerPrefab = null;

    [SerializeField]
    private MapMovement mapMovement = null;

    //[SerializeField]
    //private string fileName = "mapdata.bytes";

    [SerializeField]
    private float weatherKnowledgeGainSpeed = .01f;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    private void Update()
    {
        MapShip.PositionUpdate();
        for (int i = 0; i < mapMovement.EnemyShips.Count;)
        {
            if (mapMovement.EnemyShips[i].IsDead)
            {
                mapMovement.EnemyShips.RemoveAt(i);
            }
            else
            {
                mapMovement.EnemyShips[i].PositionUpdate();
                i++;
            }
        }
    }

    //public MapMarkersData LoadMarkers()
    //{
    //    MapMarkersData mapData;
    //    TextAsset asset = Resources.Load<TextAsset>(fileName);
    //    Stream stream = new MemoryStream(asset.bytes);
    //    BinaryFormatter bf = new BinaryFormatter();
    //    mapData = (MapMarkersData)bf.Deserialize(stream);
    //    return mapData;
    //}

    //public void SpawnMarkers(MapMarkersData mapData)
    //{
    //    //SpawnEnemyMarkers(mapData);
    //    //SpawnWeatherMarkers(mapData);
    //    //SpawnObjectiveMarkers(mapData);
    //    //SetPlayerMarkerPosition(mapData);
    //}

    public void Setup()
    {
        //SpawnMarkers(scenarioData.mapMarkersData);
        MapRectTransform.GetComponent<TacticalMap>().Init();
        MapShip.InitMarkers();
    }

    //private void SpawnEnemyMarkers(MapMarkersData mapData)
    //{
    //    List<MarkerData> enemyMarkerList = mapData.enemyMarkersList;

    //    if (enemyMarkerList.Count < 1)
    //        return;

    //    int enemyOrder = 0;
    //    foreach (MarkerData mm in enemyMarkerList)
    //    {
    //        float xValue = mm.xPos * MapRectTransform.rect.width;
    //        float yValue = mm.yPos * MapRectTransform.rect.height;
    //        Vector3 vec = new Vector3(xValue, yValue, 0f);
    //        GameObject marker = GameObject.Instantiate(enemyMarkerPrefab, enemyMarkers);
    //        marker.GetComponent<RectTransform>().anchoredPosition = vec;
    //        marker.name = "Enemy#" + enemyOrder;
    //        enemyOrder++;
    //    }
    //}

    //private void SpawnWeatherMarkers(MapMarkersData mapData)
    //{
    //    List<MarkerData> cloudMarkerList = mapData.cloudMarkersList;

    //    if (cloudMarkerList.Count < 1)
    //        return;

    //    int cloudOrder = 0;
    //    foreach (MarkerData mm in cloudMarkerList)
    //    {
    //        float xValue = mm.xPos * MapRectTransform.rect.width;
    //        float yValue = mm.yPos * MapRectTransform.rect.height;
    //        Vector3 vec = new Vector3(xValue, yValue, 0f);
    //        GameObject marker = GameObject.Instantiate(weatherMarkerPrefab, weatherMarkers);
    //        marker.GetComponent<RectTransform>().anchoredPosition = vec;
    //        marker.name = "Cloud#" + cloudOrder;
    //        cloudOrder++;
    //    }
    //}

    //private void SpawnObjectiveMarkers(MapMarkersData mapData)
    //{
    //    List<MarkerData> objectiveMarkerList = mapData.objectiveMarkersList;

    //    if (objectiveMarkerList != null)
    //    {
    //        if (objectiveMarkerList.Count < 1)
    //            return;
    //    }

    //    int objectiveOrder = 0;
    //    foreach (MarkerData mm in objectiveMarkerList)
    //    {
    //        float xValue = mm.xPos * MapRectTransform.rect.width;
    //        float yValue = mm.yPos * MapRectTransform.rect.height;
    //        Vector3 vec = new Vector3(xValue, yValue, 0f);
    //        GameObject marker = GameObject.Instantiate(objectiveMarkerPrefab, objectiveMarkers);
    //        marker.GetComponent<RectTransform>().anchoredPosition = vec;
    //        marker.name = "Objective#" + objectiveOrder;
    //        //marker.GetComponent<MapIndicator>().SetObjective(mm.objectiveDataList);
    //        objectiveOrder++;
    //    }
    //}

    //private void SetPlayerMarkerPosition(MapMarkersData mapData)
    //{
    //    MarkerData playerData = mapData.playerData;
    //    float xValue = playerData.xPos * MapRectTransform.rect.width;
    //    float yValue = playerData.yPos * MapRectTransform.rect.height;
    //    Vector3 vec = new Vector3(xValue, yValue, 0f);
    //    MapShip.GetComponent<RectTransform>().anchoredPosition = vec;
    //}
}
