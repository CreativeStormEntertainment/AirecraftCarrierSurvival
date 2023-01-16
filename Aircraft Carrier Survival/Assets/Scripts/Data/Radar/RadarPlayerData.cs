//using UnityEngine;

//public class RadarPlayerData : RadarEnemyData
//{
//    public RadarEnemyData Enemy;
//    public int MaxRing;

//    public RadarPlayerData(EDirection direction, Vector2 dirVector, float rotation, RectTransform trans, int maxRing) : base(ERadarEnemyType.Plane, direction, 1, Vector2.zero, dirVector, rotation, trans, null)
//    {
//        MaxRing = maxRing;
//    }

//    public RadarPlayerData(RadarEnemyData enemy, RectTransform trans, int maxRing) : this(enemy.Direction, -enemy.DirVector, enemy.Rotation, trans, maxRing)
//    {
//        Enemy = enemy;
//    }

//    public bool DestroyedEnemy()
//    {
//        return Enemy != null && ((RingProgress + (Ring - 1)) > (Enemy.Ring - Enemy.RingProgress));
//        //return Enemy != null && (Ring >= Enemy.Ring || ((Ring - 1) == Enemy.Ring && RingProgress > (1f - Enemy.RingProgress)));
//    }

//    protected override bool ChangeRing()
//    {
//        return ++Ring > MaxRing;
//    }
//}
