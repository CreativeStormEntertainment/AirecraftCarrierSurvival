using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TacticalFightEnemyManager : MonoBehaviour
{
    public static TacticalFightEnemyManager Instance;
    [SerializeField]
    List<TacticalFightEnemyUnit> enemiesToInstiate = null;

    private void Awake()
    {
        Instance = this;
    }

    public TacticalFightEnemyUnit GetEnemyUnit(ETacticalFightEnemyType enemyType)
    {
        return enemiesToInstiate[(int)enemyType];
    }
}