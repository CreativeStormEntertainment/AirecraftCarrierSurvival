using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForcesBar : MonoBehaviour
{
    public static ForcesBar Instance;

    [SerializeField]
    private GameObject root = null;
    [SerializeField]
    private Transform enemiesRoot = null;
    [SerializeField]
    private Transform alliesRoot = null;
    [SerializeField]
    private Sprite lightEnemy = null;
    [SerializeField]
    private Sprite enemy = null;
    [SerializeField]
    private Sprite lightAlly = null;
    [SerializeField]
    private Sprite ally = null;
    [SerializeField]
    private Sprite bg = null;

    private List<TacticalEnemyShip> allies;
    private List<TacticalEnemyShip> enemies;
    private List<Image> alliesBlocks;
    private List<Image> enemiesBlocks;

    private List<int> ships;

    private void Awake()
    {
        Instance = this;

        root.SetActive(false);

        allies = new List<TacticalEnemyShip>();
        enemies = new List<TacticalEnemyShip>();

        alliesBlocks = new List<Image>();
        enemiesBlocks = new List<Image>();

        foreach (Transform child in alliesRoot)
        {
            alliesBlocks.Add(child.GetComponent<Image>());
        }
        foreach (Transform child in enemiesRoot)
        {
            enemiesBlocks.Add(child.GetComponent<Image>());
        }

        ships = new List<int>();
    }

    public void LoadData(List<int> ships)
    {
        if (ships != null && ships.Count == 0)
        {
            ships = null;
        }
        SetBar(ships);
    }

    public List<int> SaveData()
    {
        if (root.activeSelf)
        {
            return ships;
        }
        return null;
    }

    public void SetBar(List<int> ships)
    {
        var tacMan = TacticManager.Instance;
        tacMan.BlockDestroyed -= OnBlockDestroyed;
        tacMan.ObjectDestroyed -= OnObjectDestroyed;
        if (ships == null)
        {
            root.SetActive(false);
        }
        else
        {
            this.ships.Clear();
            this.ships.AddRange(ships);

            tacMan.BlockDestroyed += OnBlockDestroyed;
            tacMan.ObjectDestroyed += OnObjectDestroyed;

            allies.Clear();
            enemies.Clear();
            foreach (var id in ships)
            {
                var ship = tacMan.GetShip(id);
                (ship.Side == ETacticalObjectSide.Friendly ? allies : enemies).Add(ship);
            }
            int alliesBlockCount = 0;
            foreach (var ally in allies)
            {
                alliesBlockCount += ally.Blocks.Count;
            }
            int enemiesBlockCount = 0;
            foreach (var enemy in enemies)
            {
                enemiesBlockCount += enemy.Blocks.Count;
            }
            for (int i = 0; i < alliesBlocks.Count; i++)
            {
                alliesBlocks[i].gameObject.SetActive(i < alliesBlockCount);
                enemiesBlocks[i].gameObject.SetActive(i < enemiesBlockCount);
            }
            Refresh();

            root.SetActive(true);
        }
    }

    private void OnObjectDestroyed(int _, bool __)
    {
        Refresh();
    }

    private void OnBlockDestroyed(EnemyManeuverData data)
    {
        Refresh();
    }

    private void Refresh()
    {
        int alliesBlockCount = CheckShipList(allies);
        int enemiesBlockCount = CheckShipList(enemies);

        for (int i = 0; i < alliesBlocks.Count; i++)
        {
            alliesBlocks[i].sprite = i < alliesBlockCount ? ally : bg;
            enemiesBlocks[i].sprite = i < enemiesBlockCount ? enemy : bg;
        }
        if (alliesBlockCount > 0)
        {
            alliesBlocks[alliesBlockCount - 1].sprite = lightAlly;
        }
        if (enemiesBlockCount > 0)
        {
            enemiesBlocks[enemiesBlockCount - 1].sprite = lightEnemy;
        }
    }

    private int CheckShipList(List<TacticalEnemyShip> list)
    {
        int result = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Dead)
            {
                list.RemoveAt(i--);
            }
            else
            {
                foreach (var block in list[i].Blocks)
                {
                    if (!block.Dead)
                    {
                        result++;
                    }
                }
            }
        }
        return result;
    }
}
