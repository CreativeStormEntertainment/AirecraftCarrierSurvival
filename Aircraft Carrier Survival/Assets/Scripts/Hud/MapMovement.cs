using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MapMovement : MonoBehaviour
{
    public List<MapEnemyShip> EnemyShips => movingObjects;
    [SerializeField]
    private MapPathsGenerator pathsGenerator = null;
    [SerializeField]
    private List<MapEnemyShip> movingObjects = null;

    private List<MapEnemyShip> currentFleets;

    public void ChangeFleet(List<TacticalEnemyMapButton> fleets, List<RectTransform> pathsRoots)
    {
        if (currentFleets != null)
        {
            foreach (var fleet in currentFleets)
            {
                fleet.Hide();
            }
        }
        else
        {
            currentFleets = new List<MapEnemyShip>();
        }
        currentFleets.Clear();

        Assert.IsTrue(fleets.Count == pathsRoots.Count);
        var paths = pathsGenerator.GeneratePaths(pathsRoots);
        for (int i = 0; i < fleets.Count; i++)
        {
            fleets[i].ShipScript.Setup(paths[i], 1f);
            currentFleets.Add(fleets[i].ShipScript);
            fleets[i].ShipScript.Show();
        }
    }
}
