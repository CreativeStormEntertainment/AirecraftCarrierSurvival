using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using UnityRandom = UnityEngine.Random;

public class SandboxMapSpawner : MonoBehaviour
{
    [SerializeField]
    private SOTacticMap dummy = null;

    [SerializeField]
    private TacticManager tacticManager = null;

    [SerializeField]
    private SandboxAdmiralLevels levels = null;

    [SerializeField]
    private UnitBucketData fleetBucket = null;
    [SerializeField]
    private UnitBucketData oneshotFleetBucket = null;
    [SerializeField]
    private UnitBucketData outpostBucket = null;

    [SerializeField]
    private List<GameObject> cloudsPrefabs = null;

    [SerializeField]
    private string destroyOutpostID = null;
    [SerializeField]
    private string destroyOutpostDescID = null;
    [SerializeField]
    private List<string> destroyOutpost2ID = null;
    [SerializeField]
    private List<string> destroyOutpost2DescID = null;
    [SerializeField]
    private List<string> destroyOutpost3ID = null;
    [SerializeField]
    private List<string> destroyOutpost3DescID = null;
    [SerializeField]
    private string seekAndDestroySeekID = null;
    [SerializeField]
    private string seekAndDestroySeekDescID = null;
    [SerializeField]
    private string seekAndDestroyDestroyID = null;
    [SerializeField]
    private string seekAndDestroyDestroyDescID = null;
    [SerializeField]
    private List<string> seekAndDestroy2SeekID = null;
    [SerializeField]
    private List<string> seekAndDestroy2SeekDescID = null;
    [SerializeField]
    private List<string> seekAndDestroy2DestroyID = null;
    [SerializeField]
    private List<string> seekAndDestroy2DestroyDescID = null;
    [SerializeField]
    private List<string> seekAndDestroy3SeekID = null;
    [SerializeField]
    private List<string> seekAndDestroy3SeekDescID = null;
    [SerializeField]
    private List<string> seekAndDestroy3DestroyID = null;
    [SerializeField]
    private List<string> seekAndDestroy3DestroyDescID = null;
    [SerializeField]
    private List<string> destroyTimedFleets2ID = null;
    [SerializeField]
    private List<string> destroyTimedFleets2DescID = null;
    [SerializeField]
    private List<string> destroyTimedFleets3ID = null;
    [SerializeField]
    private List<string> destroyTimedFleets3DescID = null;
    [SerializeField]
    private List<string> destroyTimedTimerIDs = null;
    [SerializeField]
    private string destroyChaseID = null;
    [SerializeField]
    private string destroyChaseDescID = null;
    [SerializeField]
    private List<string> destroyChase2ID = null;
    [SerializeField]
    private List<string> destroyChase2DescID = null;
    [SerializeField]
    private List<string> retrieveTimedPlanes2ID = null;
    [SerializeField]
    private List<string> retrieveTimedPlanes2DescID = null;
    [SerializeField]
    private List<string> retrieveTimedPlanes3ID = null;
    [SerializeField]
    private List<string> retrieveTimedPlanes3DescID = null;
    [SerializeField]
    private List<string> retrieveSurvivorsID = null;
    [SerializeField]
    private List<string> retrieveSurvivorsDescID = null;
    [SerializeField]
    private List<string> retrieveSurvivorsUnknownID = null;
    [SerializeField]
    private List<string> retrieveSurvivorsUnknownDescID = null;
    [SerializeField]
    private List<string> retrieveSurvivorsTimerIDs = null;
    [SerializeField]
    private string defendOutpostRepelID = null;
    [SerializeField]
    private string defendOutpostRepelDescID = null;
    [SerializeField]
    private string defendOutpostSurviveID = null;
    [SerializeField]
    private string defendOutpostSurviveDescID = null;
    [SerializeField]
    private List<string> defendOutpostTimerIDs = null;
    [SerializeField]
    private string defendOutpostsRepelID = null;
    [SerializeField]
    private string defendOutpostsRepelDescID = null;
    [SerializeField]
    private string defendOutpostsSurviveID = null;
    [SerializeField]
    private string defendOutpostsSurviveDescID = null;
    [SerializeField]
    private List<string> defendOutpostsTimerIDs = null;
    [SerializeField]
    private List<string> defendFleetDestroyEnemy7ID = null;
    [SerializeField]
    private List<string> defendFleetDestroyEnemy7DescID = null;
    [SerializeField]
    private string defendFleetProtectID = null;
    [SerializeField]
    private string defendFleetProtectDescID = null;
    [SerializeField]
    private string defendFleetsProtectID = null;
    [SerializeField]
    private string defendFleetsProtectDescID = null;
    [SerializeField]
    private string timeDefendFleetRepelID = null;
    [SerializeField]
    private string timeDefendFleetRepelDescID = null;
    [SerializeField]
    private string timeDefendFleetProtectAllyID = null;
    [SerializeField]
    private string timeDefendFleetProtectAllyDescID = null;
    [SerializeField]
    private string timeDefendFleetTimerID = null;
    [SerializeField]
    private string timeDefendFleetTimerTooltipID = null;
    [SerializeField]
    private string timeDefendFleetTimerTooltipDescID = null;
    [SerializeField]
    private string escortRetreatEscortFleetID = null;
    [SerializeField]
    private string escortRetreatEscortFleetDescID = null;
    [SerializeField]
    private string escortRetreatProtectAllyID = null;
    [SerializeField]
    private string escortRetreatProtectAllyDescID = null;
    [SerializeField]
    private string escortAttackEscortFleetID = null;
    [SerializeField]
    private string escortAttackEscortFleetDescID = null;
    [SerializeField]
    private string escortAttackProtectAllyID = null;
    [SerializeField]
    private string escortAttackProtectAllyDescID = null;
    [SerializeField]
    private string escortInvasionEscortFleetID = null;
    [SerializeField]
    private string escortInvasionEscortFleetDescID = null;
    [SerializeField]
    private string escortInvasionProtectAllyID = null;
    [SerializeField]
    private string escortInvasionProtectAllyDescID = null;
    [SerializeField]
    private string destroyUndetectedDestroyID = null;
    [SerializeField]
    private string destroyUndetectedDestroyDescID = null;
    [SerializeField]
    private string destroyUndetectedStayUndetectedID = null;
    [SerializeField]
    private string destroyUndetectedStayUndetectedDescID = null;
    [SerializeField]
    private List<string> defendUndetectedDefendTime3ID = null;
    [SerializeField]
    private List<string> defendUndetectedDefendTime3DescID = null;
    [SerializeField]
    private string defendUndetectedProtectOutpostID = null;
    [SerializeField]
    private string defendUndetectedProtectOutpostDescID = null;
    [SerializeField]
    private string defendUndetectedStayUndetectedID = null;
    [SerializeField]
    private string defendUndetectedStayUndetectedDescID = null;
    [SerializeField]
    private string defendUndetectedTimerID = null;
    [SerializeField]
    private string defendUndetectedTimerTooltipID = null;
    [SerializeField]
    private string defendUndetectedTimerTooltipDescID = null;
    [SerializeField]
    private List<string> defendUndetectedFleetDefendTime3ID = null;
    [SerializeField]
    private List<string> defendUndetectedFleetDefendTime3DescID = null;
    [SerializeField]
    private string defendUndetectedFleetProtectFleetID = null;
    [SerializeField]
    private string defendUndetectedFleetProtectFleetDescID = null;
    [SerializeField]
    private string defendUndetectedFleetStayUndetectedID = null;
    [SerializeField]
    private string defendUndetectedFleetStayUndetectedDescID = null;
    [SerializeField]
    private string defendUndetectedFleetTimerID = null;
    [SerializeField]
    private string defendUndetectedFleetTimerTooltipID = null;
    [SerializeField]
    private string defendUndetectedFleetTimerTooltipDescID = null;
    [SerializeField]
    private List<string> scoutDiscover3ID = null;
    [SerializeField]
    private List<string> scoutDiscover3DescID = null;
    [SerializeField]
    private List<string> scoutDiscover4ID = null;
    [SerializeField]
    private List<string> scoutDiscover4DescID = null;
    [SerializeField]
    private List<string> scoutIdentify3ID = null;
    [SerializeField]
    private List<string> scoutIdentify3DescID = null;
    [SerializeField]
    private List<string> scoutIdentify4ID = null;
    [SerializeField]
    private List<string> scoutIdentify4DescID = null;
    [SerializeField]
    private string scoutStayUndetectedID = null;
    [SerializeField]
    private string scoutStayUndetectedDescID = null;
    [SerializeField]
    private List<string> scoutOutpostDiscover3ID = null;
    [SerializeField]
    private List<string> scoutOutpostDiscover3DescID = null;
    [SerializeField]
    private List<string> scoutOutpostDiscover4ID = null;
    [SerializeField]
    private List<string> scoutOutpostDiscover4DescID = null;
    [SerializeField]
    private List<string> scoutOutpostIdentify3ID = null;
    [SerializeField]
    private List<string> scoutOutpostIdentify3DescID = null;
    [SerializeField]
    private List<string> scoutOutpostIdentify4ID = null;
    [SerializeField]
    private List<string> scoutOutpostIdentify4DescID = null;
    [SerializeField]
    private string scoutOutpostStayUndetectedID = null;
    [SerializeField]
    private string scoutOutpostStayUndetectedDescID = null;
    [SerializeField]
    private List<string> patrolDiscover3ID = null;
    [SerializeField]
    private List<string> patrolDiscover3DescID = null;
    [SerializeField]
    private List<string> patrolDiscover4ID = null;
    [SerializeField]
    private List<string> patrolDiscover4DescID = null;
    [SerializeField]
    private List<string> patrolIdentify3ID = null;
    [SerializeField]
    private List<string> patrolIdentify3DescID = null;
    [SerializeField]
    private List<string> patrolIdentify4ID = null;
    [SerializeField]
    private List<string> patrolIdentify4DescID = null;
    [SerializeField]
    private List<string> patrolOutpostDiscover3ID = null;
    [SerializeField]
    private List<string> patrolOutpostDiscover3DescID = null;
    [SerializeField]
    private List<string> patrolOutpostDiscover4ID = null;
    [SerializeField]
    private List<string> patrolOutpostDiscover4DescID = null;
    [SerializeField]
    private List<string> patrolOutpostIdentify3ID = null;
    [SerializeField]
    private List<string> patrolOutpostIdentify3DescID = null;
    [SerializeField]
    private List<string> patrolOutpostIdentify4ID = null;
    [SerializeField]
    private List<string> patrolOutpostIdentify4DescID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover3ID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover3DescID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover4ID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover4DescID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover5ID = null;
    [SerializeField]
    private List<string> patrolUndetectedDiscover5DescID = null;
    [SerializeField]
    private string patrolUndetectedStayUndetectedID = null;
    [SerializeField]
    private string patrolUndetectedStayUndetectedDescID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover3ID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover3DescID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover4ID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover4DescID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover5ID = null;
    [SerializeField]
    private List<string> patrolUndetectedOutpostDiscover5DescID = null;
    [SerializeField]
    private string redWatersDestroyEnemyID = null;
    [SerializeField]
    private string redWatersDestroyEnemyDescID = null;

    [SerializeField]
    private int maxAdditionalEnemies = 4;

    [SerializeField]
    private float undetectedMinEnemyDistance = 500f;

    [SerializeField]
    private float fleetMaxDistanceFromOutpost = 600f;
    [SerializeField]
    private List<float> seekAndDestroyDistances = null;
    [SerializeField]
    private List<float> seekAndDestroyMultipleDistances = null;
    [SerializeField]
    private float destroyTimedSpawnMaxDistance = 1000f;
    [SerializeField]
    private float destroyChaseMaxDistance = 200f;
    [SerializeField]
    private float destroyChaseMinEdgeDistance = 1000f;
    [SerializeField]
    private float destroyChaseMultipleMaxDistance = 300f;
    [SerializeField]
    private float destroyChaseMultipleDistanceBetween = 300f;
    [SerializeField]
    private float retrievePlanesFirstMaxDistance = 600f;
    [SerializeField]
    private List<float> retrievePlanesMinDistanceBetween = null;
    [SerializeField]
    private List<float> retrievePlanesMaxDistanceBetween = null;
    [SerializeField]
    private float retrievePlanesMaxPatrolDistance = 300f;

    [SerializeField]
    private List<FightSquadronData> retrievePlanesOptions = null;

    [SerializeField]
    private float retrieveSurvivorsFirstMinDistance = 400f;
    [SerializeField]
    private float retrieveSurvivorsFirstMaxDistance = 600f;
    [SerializeField]
    private List<float> retrieveSurvivorsDistanceBetween = null;
    [SerializeField]
    private float allyOutpostMaxDistance = 600f;
    [SerializeField]
    private float fleetMinDistanceFromAllyOutpost = 200f;
    [SerializeField]
    private float fleetMaxDistanceFromAllyOutpost = 500f;
    [SerializeField]
    private float allyOutpostsMaxDistance = 700f;
    [SerializeField]
    private float allyOutpostsDistanceBetween = 700f;
    [SerializeField]
    private float fleetMinDistanceFromAllyOutposts = 200f;
    [SerializeField]
    private float fleetMaxDistanceFromAllyOutposts = 500f;
    [SerializeField]
    private float defendFleetMaxDistance = 200f;
    [SerializeField]
    private float defendFleetTargetMinDistance = 800f;
    [SerializeField]
    private float defendFleetsMaxDistance = 200f;
    [SerializeField]
    private float defendFleetsMinDistanceBetween = 50f;
    [SerializeField]
    private float defendFleetsTargetMinDistance = 800f;
    [SerializeField]
    private float timeDefendFleetMaxDistance = 200f;
    [SerializeField]
    private float timeDefendFleetTargetMinDistance = 800f;
    [SerializeField]
    private List<float> escortRetreatSwimDistance = null;
    [SerializeField]
    private List<float> escortAttackSwimDistance = null;
    [SerializeField]
    private List<float> escortInvasionSwimDistance = null;

    [SerializeField]
    private List<float> destroyUndetectedFleetDistances = null;
    [SerializeField]
    private float defendUndetectedOutpostMinDistance = 300f;
    [SerializeField]
    private float defendUndetectedOutpostMaxDistance = 500f;
    [SerializeField]
    private float defendUndetectedEnemyMaxDistanceFromOutpost = 500f;
    [SerializeField]
    private float defendFleetUndetectedMaxFleetDistance = 200f;
    [SerializeField]
    private float defendFleetUndetectedTargetMinDistance = 800f;
    [SerializeField]
    private float patrolEnemyMinDistance = 200f;
    [SerializeField]
    private float patrolUndetectedUOMinDistance = 300f;
    [SerializeField]
    private float redWatersMaxDistance = 150f;

    [SerializeField]
    private List<int> destroyTimeTicks = null;
    [SerializeField]
    private List<float> destroyChaseSpeeds = null;
    [SerializeField]
    private List<float> destroyChaseMultipleSpeeds = null;
    [SerializeField]
    private List<int> retrieveTimedPlanesHours = null;
    [SerializeField]
    private List<int> retrieveSurvivorsTicks = null;
    [SerializeField]
    private List<int> defendOutpostTicks = null;
    [SerializeField]
    private List<int> defendOutpostsTicks = null;
    [SerializeField]
    private List<int> timeDefendFleetTicks = null;
    [SerializeField]
    private List<int> defendUndetectedTicks = null;
    [SerializeField]
    private List<int> defendUndetectedFleetTicks = null;
    [SerializeField]
    private List<EnemyUnitData> possibleOutposts1 = null;
    [SerializeField]
    private List<EnemyUnitData> possibleOutposts2 = null;
    [SerializeField]
    private List<EnemyUnitData> possibleOutposts3 = null;
    [SerializeField]
    private List<EnemyUnitData> possibleFleets1 = null;
    [SerializeField]
    private List<EnemyUnitData> possibleFleets2 = null;
    [SerializeField]
    private List<EnemyUnitData> possibleFleets3 = null;

    [SerializeField]
    private List<string> enemyFleetNames = null;
    [SerializeField]
    private List<string> enemyOutpostNames = null;
    [SerializeField]
    private List<string> friendlyFleetNames = null;
    [SerializeField]
    private List<string> friendlyOutpostNames = null;

    [SerializeField]
    private float attackRange = 50f;
    [SerializeField]
    private float detectRange = 50f;
    [SerializeField]
    private float revealRange = 50f;
    [SerializeField]
    private float maxOffset = 50f;
    [SerializeField]
    private float speed = 50f;

    private List<int> intHelper = new List<int>();
    private List<int> intHelper2 = new List<int>();
    private List<int> intHelper3 = new List<int>();
    private List<int> intHelper4 = new List<int>();
    private HashSet<int> setHelper = new HashSet<int>();
    private HashSet<int> setHelper2 = new HashSet<int>();

    private List<int> moveToBack = new List<int>();

    private List<GameObject> bucket = new List<GameObject>();

    [NonSerialized]
    private float sqrFleetMaxDistanceFromOutpost;
    [NonSerialized]
    private float tacticToWorldSqr;
    [NonSerialized]
    private List<float> seekAndDestroyWorldSqrDistances;
    [NonSerialized]
    private List<float> seekAndDestroyMultipleWorldSqrDistances;
    [NonSerialized]
    private List<float> retrievePlanesMinWorldSqrDistanceBetween;
    [NonSerialized]
    private List<float> retrievePlanesMaxWorldSqrDistanceBetween;
    [NonSerialized]
    private List<float> escortRetreatSwimSqrDistance;
    [NonSerialized]
    private List<float> escortAttackSwimSqrDistance;
    [NonSerialized]
    private List<float> escortInvasionSwimSqrDistance;
    [NonSerialized]
    private List<float> destroyUndetectedFleetWorldSqrDistances;
    [NonSerialized]
    private float undetectedMinEnemyWorldSqrDistance;
    [NonSerialized]
    private float retrieveSurvivorsFirstMinWorldSqrDistance;
    [NonSerialized]
    private float retrieveSurvivorsFirstMaxWorldSqrDistance;
    [NonSerialized]
    private float retrieveSurvivorsWorldSqrDistanceBetween;
    [NonSerialized]
    private float allyOutpostMaxWorldSqrDistance;
    [NonSerialized]
    private float fleetMaxWorldSqrDistanceFromAllyOutpost;
    [NonSerialized]
    private float fleetMinWorldSqrDistanceFromAllyOutpost;
    [NonSerialized]
    private float allyOutpostsMaxWorldSqrDistance;
    [NonSerialized]
    private float allyOutpostsWorldSqrDistanceBetween;
    [NonSerialized]
    private float fleetMinWorldSqrDistanceFromAllyOutposts;
    [NonSerialized]
    private float fleetMaxWorldSqrDistanceFromAllyOutposts;
    [NonSerialized]
    private float defendFleetMaxWorldSqrDistance;
    [NonSerialized]
    private float defendFleetTargetMinWorldSqrDistance;
    [NonSerialized]
    private float defendFleetsMaxWorldSqrDistance;
    [NonSerialized]
    private float defendFleetsMinWorldSqrDistanceBetween;
    [NonSerialized]
    private float defendFleetsTargetMinWorldSqrDistance;
    [NonSerialized]
    private float timeDefendFleetMaxWorldSqrDistance;
    [NonSerialized]
    private float timeDefendFleetTargetMinWorldSqrDistance;
    [NonSerialized]
    private float defendUndetectedOutpostMinWorldSqrDistance;
    [NonSerialized]
    private float defendUndetectedOutpostMaxWorldSqrDistance;
    [NonSerialized]
    private float defendUndetectedEnemyMaxWorldSqrDistanceFromOutpost;
    [NonSerialized]
    private float defendFleetUndetectedMaxFleetWorldSqrDistance;
    [NonSerialized]
    private float defendFleetUndetectedTargetMinWorldSqrDistance;
    [NonSerialized]
    private float patrolEnemyMinWorldSqrDistance;
    [NonSerialized]
    private float patrolUndetectedUOMinWorldSqrDistance;
    [NonSerialized]
    private float redWatersMaxWorldSqrDistance;

    [NonSerialized]
    private MapSpawnData mapSpawnData;

    [NonSerialized]
    private List<string> enemyFleetNamesBucket;
    [NonSerialized]
    private List<string> enemyOutpostNamesBucket;
    [NonSerialized]
    private List<string> friendlyFleetNamesBucket;
    [NonSerialized]
    private List<string> friendlyOutpostNamesBucket;

    public void Setup()
    {
        fleetBucket.Setup(levels);
        oneshotFleetBucket.Setup(levels);
        outpostBucket.Setup(levels);

        SetBlocksIndex(possibleOutposts1);
        SetBlocksIndex(possibleOutposts2);
        SetBlocksIndex(possibleOutposts3);
        SetBlocksIndex(possibleFleets1);
        SetBlocksIndex(possibleFleets2);
        SetBlocksIndex(possibleFleets3);

        enemyFleetNamesBucket = new List<string>();
        enemyOutpostNamesBucket = new List<string>();
        friendlyFleetNamesBucket = new List<string>();
        friendlyOutpostNamesBucket = new List<string>();
    }

#if UNITY_EDITOR
    public void SetupEditor()
    {
        sqrFleetMaxDistanceFromOutpost = fleetMaxDistanceFromOutpost * fleetMaxDistanceFromOutpost;

        tacticToWorldSqr = TacticalMapCreator.TacticToWorldMapScale;
        tacticToWorldSqr *= tacticToWorldSqr;

        seekAndDestroyWorldSqrDistances = new List<float>();
        foreach (float distance in seekAndDestroyDistances)
        {
            seekAndDestroyWorldSqrDistances.Add(distance * distance * tacticToWorldSqr);
        }
        seekAndDestroyMultipleWorldSqrDistances = new List<float>();
        foreach (float distance in seekAndDestroyMultipleDistances)
        {
            seekAndDestroyMultipleWorldSqrDistances.Add(distance * distance * tacticToWorldSqr);
        }
        retrievePlanesMinWorldSqrDistanceBetween = new List<float>();
        foreach (float distance in retrievePlanesMinDistanceBetween)
        {
            retrievePlanesMinWorldSqrDistanceBetween.Add(distance * distance * tacticToWorldSqr);
        }
        retrievePlanesMaxWorldSqrDistanceBetween = new List<float>();
        foreach (float distance in retrievePlanesMaxDistanceBetween)
        {
            retrievePlanesMaxWorldSqrDistanceBetween.Add(distance * distance * tacticToWorldSqr);
        }
        escortRetreatSwimSqrDistance = new List<float>();
        foreach (float distance in escortRetreatSwimDistance)
        {
            escortRetreatSwimSqrDistance.Add(distance * distance);
        }
        escortAttackSwimSqrDistance = new List<float>();
        foreach (float distance in escortAttackSwimDistance)
        {
            escortAttackSwimSqrDistance.Add(distance * distance);
        }
        escortInvasionSwimSqrDistance = new List<float>();
        foreach (float distance in escortInvasionSwimDistance)
        {
            escortInvasionSwimSqrDistance.Add(distance * distance);
        }
        destroyUndetectedFleetWorldSqrDistances = new List<float>();
        foreach (float distance in destroyUndetectedFleetDistances)
        {
            destroyUndetectedFleetWorldSqrDistances.Add(distance * distance * tacticToWorldSqr);
        }

        undetectedMinEnemyWorldSqrDistance = undetectedMinEnemyDistance * undetectedMinEnemyDistance * tacticToWorldSqr;
        retrieveSurvivorsFirstMinWorldSqrDistance = retrieveSurvivorsFirstMinDistance * retrieveSurvivorsFirstMinDistance * tacticToWorldSqr;
        retrieveSurvivorsFirstMaxWorldSqrDistance = retrieveSurvivorsFirstMaxDistance * retrieveSurvivorsFirstMaxDistance * tacticToWorldSqr;
        retrieveSurvivorsWorldSqrDistanceBetween = retrieveSurvivorsDistanceBetween[0] * retrieveSurvivorsDistanceBetween[0] * tacticToWorldSqr;
        allyOutpostMaxWorldSqrDistance = allyOutpostMaxDistance * allyOutpostMaxDistance * tacticToWorldSqr;
        fleetMinWorldSqrDistanceFromAllyOutpost = fleetMinDistanceFromAllyOutpost * fleetMinDistanceFromAllyOutpost * tacticToWorldSqr;
        fleetMaxWorldSqrDistanceFromAllyOutpost = fleetMaxDistanceFromAllyOutpost * fleetMaxDistanceFromAllyOutpost * tacticToWorldSqr;
        allyOutpostsMaxWorldSqrDistance = allyOutpostsMaxDistance * allyOutpostsMaxDistance * tacticToWorldSqr;
        allyOutpostsWorldSqrDistanceBetween = allyOutpostsDistanceBetween * allyOutpostsDistanceBetween * tacticToWorldSqr;
        fleetMinWorldSqrDistanceFromAllyOutposts = fleetMinDistanceFromAllyOutposts * fleetMinDistanceFromAllyOutposts * tacticToWorldSqr;
        fleetMaxWorldSqrDistanceFromAllyOutposts = fleetMaxDistanceFromAllyOutposts * fleetMaxDistanceFromAllyOutposts * tacticToWorldSqr;
        defendFleetMaxWorldSqrDistance = defendFleetMaxDistance * defendFleetMaxDistance * tacticToWorldSqr;
        defendFleetTargetMinWorldSqrDistance = defendFleetTargetMinDistance * defendFleetTargetMinDistance * tacticToWorldSqr;
        defendFleetsMaxWorldSqrDistance = defendFleetsMaxDistance * defendFleetsMaxDistance * tacticToWorldSqr;
        defendFleetsMinWorldSqrDistanceBetween = defendFleetsMinDistanceBetween * defendFleetsMinDistanceBetween * tacticToWorldSqr;
        defendFleetsTargetMinWorldSqrDistance = defendFleetsTargetMinDistance * defendFleetsTargetMinDistance * tacticToWorldSqr;
        timeDefendFleetMaxWorldSqrDistance = timeDefendFleetMaxDistance * timeDefendFleetMaxDistance * tacticToWorldSqr;
        timeDefendFleetTargetMinWorldSqrDistance = timeDefendFleetTargetMinDistance * timeDefendFleetTargetMinDistance * tacticToWorldSqr;
        defendUndetectedOutpostMinWorldSqrDistance = defendUndetectedOutpostMinDistance * defendUndetectedOutpostMinDistance * tacticToWorldSqr;
        defendUndetectedOutpostMaxWorldSqrDistance = defendUndetectedOutpostMaxDistance * defendUndetectedOutpostMaxDistance * tacticToWorldSqr;
        defendUndetectedEnemyMaxWorldSqrDistanceFromOutpost = defendUndetectedEnemyMaxDistanceFromOutpost * defendUndetectedEnemyMaxDistanceFromOutpost * tacticToWorldSqr;
        defendFleetUndetectedMaxFleetWorldSqrDistance = defendFleetUndetectedMaxFleetDistance * defendFleetUndetectedMaxFleetDistance * tacticToWorldSqr;
        defendFleetUndetectedTargetMinWorldSqrDistance = defendFleetUndetectedTargetMinDistance * defendFleetUndetectedTargetMinDistance * tacticToWorldSqr;
        patrolEnemyMinWorldSqrDistance = patrolEnemyMinDistance * patrolEnemyMinDistance * tacticToWorldSqr;
        patrolUndetectedUOMinWorldSqrDistance = patrolUndetectedUOMinDistance * patrolUndetectedUOMinDistance * tacticToWorldSqr;
        redWatersMaxWorldSqrDistance = redWatersMaxDistance * redWatersMaxDistance * tacticToWorldSqr;
    }

    public MapSpawnData CanSpawnMission(ESandboxObjectiveType type, IEnumerable<Vector2> outpostNodes, IEnumerable<Vector2> outpostOnWaterNodes, IEnumerable<Vector2> nodes, IEnumerable<Vector2> edgeNodes,
        Vector2 mapPos, List<int> startNodes, List<List<float>> distances)
    {
        float airstrikeSqrRange = AirstrikeSqrRange() * tacticToWorldSqr;
        float sqrMaxDistance = sqrFleetMaxDistanceFromOutpost * tacticToWorldSqr;

        var result = new MapSpawnData(type, nodes, mapPos);
        int flag;
        switch (type)
        {
            case ESandboxObjectiveType.DestroyBase:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }
                result.Outposts = new List<MyVector2>();
                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.FlagList.Add(0);
                }
                foreach (var node in outpostNodes)
                {
                    if (Vector2.SqrMagnitude(node - mapPos) < airstrikeSqrRange)
                    {
                        continue;
                    }
                    intHelper.Clear();
                    intHelper.AddRange(GetNodesWithin(ToVector2(result.Nodes), node, sqrMaxDistance));
                    if (intHelper.Count > 3)
                    {
                        Assert.IsTrue(result.Outposts.Count < 32, "Tooooo many outposts on map");
                        int outpostBit = 1 << result.Outposts.Count;
                        result.Outposts.Add(node);
                        foreach (int index in intHelper)
                        {
                            result.FlagList[index] |= outpostBit;
                        }
                    }
                }
                return result.Outposts.Count > 0 ? result : null;
            case ESandboxObjectiveType.DestroyMultipleBases:
                if (result.Nodes.Count < 9)
                {
                    return null;
                }
                result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
                Assert.IsTrue(result.Outposts.Count < 33, "Tooooo many outposts on map");
                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Outposts), mapPos, airstrikeSqrRange))
                {
                    setHelper.Add(index);
                }
                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    int value = 0;
                    foreach (int index in GetNodesWithout(ToVector2(result.Outposts), result.Nodes[i], sqrMaxDistance))
                    {
                        value |= 1 << index;
                    }
                    result.FlagList.Add(value);
                }
                result.TripletList = new List<int>();
                for (int i = 0; i < result.Outposts.Count; i++)
                {
                    int iFlag = 1 << i;
                    for (int j = i + 1; j < result.Outposts.Count; j++)
                    {
                        int jFlag = 1 << j;
                        for (int k = j + 1; k < result.Outposts.Count; k++)
                        {
                            if (!setHelper.Contains(i) && !setHelper.Contains(j) && !setHelper.Contains(k))
                            {
                                continue;
                            }

                            flag = iFlag | jFlag | (1 << k);
                            if (FlagCount(result.FlagList, flag) > 3)
                            {
                                result.TripletList.Add(flag);
                            }
                        }
                    }
                }
                if (result.TripletList.Count == 0)
                {
                    return null;
                }
                result.AdditionalInt = new List<int>(setHelper);
                return result;
            case ESandboxObjectiveType.SeekAndDestroy:
                if (result.Nodes.Count < 6)
                {
                    return null;
                }

                return CheckSeekAndDestroy(seekAndDestroyWorldSqrDistances, mapPos, 1, result);
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }
                return CheckSeekAndDestroy(seekAndDestroyMultipleWorldSqrDistances, mapPos, 3, result);
            case ESandboxObjectiveType.DestroyTimedFleets:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }

                result.FlagList = new List<int>(GetNodesWithin(ToVector2(result.Nodes), mapPos, destroyTimedSpawnMaxDistance * destroyTimedSpawnMaxDistance * tacticToWorldSqr));
                return result.FlagList.Count > 2 ? result : null;
            case ESandboxObjectiveType.DestroyChase:
                if (result.Nodes.Count < 6)
                {
                    return null;
                }
                GetChaseNodes(result, edgeNodes, mapPos, destroyChaseMaxDistance, destroyChaseMinEdgeDistance);
                return result.AdditionalInt.Count > 0 ? result : null;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                if (result.Nodes.Count < 7)
                {
                    return null;
                }
                GetChaseNodes(result, edgeNodes, mapPos, destroyChaseMultipleMaxDistance, destroyChaseMinEdgeDistance);

                float destroyChaseMultipleSqrDistanceBetween = destroyChaseMultipleDistanceBetween * destroyChaseMultipleDistanceBetween;
                destroyChaseMultipleSqrDistanceBetween *= tacticToWorldSqr;

                result.TripletList = new List<int>();
                Assert.IsTrue(result.AdditionalInt.Count < 33);
                for (int i = 0; i < result.AdditionalInt.Count; i++)
                {
                    for (int j = i + 1; j < result.AdditionalInt.Count; j++)
                    {
                        if (Vector2.SqrMagnitude((Vector2)result.Nodes[result.AdditionalInt[i]] - result.Nodes[result.AdditionalInt[j]]) < destroyChaseMultipleSqrDistanceBetween)
                        {
                            result.TripletList.Add(1 << i | 1 << j);
                        }
                    }
                }
                return result.TripletList.Count > 0 ? result : null;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
                if (result.Nodes.Count < 11)
                {
                    return null;
                }

                setHelper.Clear();
                foreach (int index in GetNodesWithin(ToVector2(result.Nodes), mapPos, retrievePlanesFirstMaxDistance * retrievePlanesFirstMaxDistance * tacticToWorldSqr))
                {
                    setHelper.Add(index);
                }
                intHelper.Clear();
                int index11 = -1;
                int index22 = -1;
                Assert.IsTrue(result.Nodes.Count < 511);
                for (int i = 0; i < retrievePlanesMinWorldSqrDistanceBetween.Count; i++)
                {
                    for (int j = 0; j < result.Nodes.Count; j++)
                    {
                        for (int k = j + 1; k < result.Nodes.Count; k++)
                        {
                            if (!CheckDistanceWithin(result.Nodes, j, k, retrievePlanesMinWorldSqrDistanceBetween[i], retrievePlanesMaxWorldSqrDistanceBetween[i]))
                            {
                                continue;
                            }
                            for (int l = k + 1; l < result.Nodes.Count; l++)
                            {
                                if ((!CheckDistanceWithin(result.Nodes, j, l, retrievePlanesMinWorldSqrDistanceBetween[i], retrievePlanesMaxWorldSqrDistanceBetween[i]))
                                    || (!CheckDistanceWithin(result.Nodes, k, l, retrievePlanesMinWorldSqrDistanceBetween[i], retrievePlanesMaxWorldSqrDistanceBetween[i])))
                                {
                                    continue;
                                }
                                if (!setHelper.Contains(i) && !setHelper.Contains(j) && !!setHelper.Contains(k))
                                {
                                    continue;
                                }
                                intHelper.Add(j << 20 | k << 10 | l);

                                setHelper2.Clear();
                                setHelper2.Add(j);
                                setHelper2.Add(k);
                                setHelper2.Add(l);
                                foreach (int index in GetTripletIndices(j << 20 | k << 10 | l, 10))
                                {
                                    Assert.IsTrue(setHelper2.Contains(index));
                                }
                            }
                        }
                    }
                    switch (i)
                    {
                        case 0:
                            result.HelperIndex = index11 = intHelper.Count;
                            break;
                        case 1:
                            result.HelperIndex2 = index22 = intHelper.Count;
                            break;
                        case 2:
                            break;
                        default:
                            Assert.IsTrue(false);
                            break;
                    }
                }
                Assert.IsTrue(intHelper.Count < 65535);
                Assert.IsTrue(index11 > -1);
                Assert.IsTrue(index22 > -1);
                intHelper2.Clear();
                for (int i = 0; i < intHelper.Count; i++)
                {
                    intHelper2.Add(0);
                }
                result.AdditionalInt = new List<int>(setHelper);
                float retrievePlanesMaxPatrolMaxSqrDistance = retrievePlanesMaxPatrolDistance * retrievePlanesMaxPatrolDistance;
                retrievePlanesMaxPatrolMaxSqrDistance *= tacticToWorldSqr;
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    setHelper.Clear();
                    foreach (int index in GetNodesWithin(ToVector2(result.Nodes), result.Nodes[i], retrievePlanesMaxPatrolMaxSqrDistance))
                    {
                        setHelper.Add(index);
                    }
                    setHelper.Remove(i);
                    for (int j = 0; j < intHelper.Count; j++)
                    {
                        bool found = false;
                        foreach (int index in GetTripletIndices(intHelper[i], 10))
                        {
                            if (setHelper.Contains(index))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            intHelper2[j]++;
                        }
                    }
                }
                result.TripletList = new List<int>();
                for (int i = 0; i < intHelper2.Count; i++)
                {
                    if (intHelper2[i] < 2)
                    {
                        if (i < index11)
                        {
                            result.HelperIndex--;
                        }
                        if (i < index22)
                        {
                            result.HelperIndex2--;
                        }
                        continue;
                    }
                    result.TripletList.Add(intHelper[i]);
                }
                return (result.HelperIndex > 0) && ((result.HelperIndex2 - result.HelperIndex) > 0) && ((result.TripletList.Count - result.HelperIndex2) > 0) ? result : null;
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                if (result.Nodes.Count < 12)
                {
                    return null;
                }

                setHelper.Clear();
                foreach (int index in GetNodesWithin(ToVector2(result.Nodes), mapPos, retrieveSurvivorsFirstMaxWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                foreach (int index in GetNodesWithin(ToVector2(result.Nodes), mapPos, retrieveSurvivorsFirstMinWorldSqrDistance))
                {
                    setHelper.Remove(index);
                }
                intHelper.Clear();
                intHelper.AddRange(setHelper);
                result.AdditionalInt = new List<int>();
                result.TripletList = new List<int>();
                result.LongFlagList = new List<long>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.LongFlagList.Add(0L);
                }

                Assert.IsTrue(result.Nodes.Count < 256);

                var set = new HashSet<int>();

                foreach (int index in intHelper)
                {
                    intHelper2.Clear();
                    intHelper2.AddRange(GetNodesWithin(ToVector2(result.Nodes), result.Nodes[index], retrieveSurvivorsWorldSqrDistanceBetween));
                    if (intHelper2.Count < 4)
                    {
                        continue;
                    }

                    int count = 0;
                    bool ok = false;
                    for (int i = 0; i < intHelper2.Count; i++)
                    {
                        setHelper.Clear();
                        foreach (int index2 in intHelper2)
                        {
                            setHelper.Add(index2);
                        }
                        setHelper.Remove(index);

                        if (!setHelper.Contains(intHelper2[i]))
                        {
                            continue;
                        }
                        intHelper3.Clear();
                        foreach (int index2 in GetNodesWithin(ToVector2(result.Nodes), result.Nodes[intHelper2[i]], retrieveSurvivorsWorldSqrDistanceBetween))
                        {
                            if (setHelper.Contains(index2))
                            {
                                intHelper3.Add(index2);
                            }
                        }

                        for (int j = i + 1; j < intHelper2.Count; j++)
                        {
                            setHelper.Clear();
                            foreach (int index2 in intHelper3)
                            {
                                setHelper.Add(index2);
                            }
                            setHelper.Remove(index);
                            setHelper.Remove(intHelper2[i]);
                            if (!setHelper.Contains(intHelper2[j]))
                            {
                                continue;
                            }

                            setHelper2.Clear();
                            foreach (int index2 in GetNodesWithin(ToVector2(result.Nodes), result.Nodes[intHelper2[j]], retrieveSurvivorsWorldSqrDistanceBetween))
                            {
                                if (setHelper.Contains(index2))
                                {
                                    setHelper2.Add(index2);
                                }
                            }
                            setHelper2.Remove(intHelper2[j]);
                            for (int k = j + 1; k < intHelper2.Count; k++)
                            {
                                if (!setHelper2.Contains(intHelper2[k]))
                                {
                                    continue;
                                }
                                intHelper4.Clear();
                                foreach (int index2 in GetNodesWithin(ToVector2(result.Nodes), result.Nodes[intHelper2[k]], retrieveSurvivorsWorldSqrDistanceBetween))
                                {
                                    if (index2 != intHelper2[k] && setHelper2.Contains(index2))
                                    {
                                        intHelper4.Add(index2);
                                    }
                                }
                                if (intHelper4.Count > 2)
                                {
                                    foreach (int index2 in intHelper4)
                                    {
                                        result.LongFlagList[index2] |= 1L << result.TripletList.Count;
                                    }

                                    result.TripletList.Add(index | intHelper2[i] << 8 | intHelper2[j] << 16 | intHelper2[k] << 24);

                                    set.Clear();
                                    set.Add(index);
                                    set.Add(intHelper2[i]);
                                    set.Add(intHelper2[j]);
                                    set.Add(intHelper2[k]);
                                    foreach (int index2 in GetTripletIndices(index | intHelper2[i] << 8 | intHelper2[j] << 16 | intHelper2[k] << 24, 8))
                                    {
                                        Assert.IsTrue(set.Contains(index2));
                                    }

                                    ok = true;
                                    if (result.TripletList.Count == 63 || ++count > 10)
                                    {
                                        intHelper2.Clear();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (ok)
                    {
                        result.AdditionalInt.Add(index);
                    }
                    if (result.TripletList.Count == 63)
                    {
                        break;
                    }
                }
                if (result.AdditionalInt.Count == 0)
                {
                    return null;
                }
                result.AdditionalInt.Clear();
                return result;
            case ESandboxObjectiveType.DefendBase:
                if (result.Nodes.Count < 9)
                {
                    return null;
                }
                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.FlagList.Add(0);
                }
                result.LongFlagList = new List<long>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.LongFlagList.Add(0L);
                }
                result.Outposts = new List<MyVector2>();
                foreach (var node in outpostNodes)
                {
                    if (Vector2.SqrMagnitude(node - mapPos) > allyOutpostMaxWorldSqrDistance)
                    {
                        continue;
                    }

                    intHelper.Clear();
                    intHelper.AddRange(GetNodesWithin(ToVector2(result.Nodes), node, fleetMaxWorldSqrDistanceFromAllyOutpost));
                    intHelper2.Clear();
                    intHelper2.AddRange(intHelper);
                    foreach (int index in GetNodesWithin(ToVector2(result.Nodes), node, fleetMinWorldSqrDistanceFromAllyOutpost))
                    {
                        intHelper2.Remove(index);
                    }
                    if (intHelper2.Count > 3)
                    {
                        Assert.IsTrue(result.Outposts.Count < 32, "Tooooo many outposts on map");
                        int outpostBit = 1 << result.Outposts.Count;
                        long outpostBitL = 1L << result.Outposts.Count;
                        result.Outposts.Add(node);
                        foreach (int index in intHelper2)
                        {
                            result.FlagList[index] |= outpostBit;
                        }
                        foreach (int index in intHelper)
                        {
                            result.LongFlagList[index] |= outpostBitL;
                        }
                    }
                }
                return result.Outposts.Count > 0 ? result : null;
            case ESandboxObjectiveType.DefendMultipleBases:
                if (result.Nodes.Count < 10)
                {
                    return null;
                }
                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.FlagList.Add(0);
                }
                result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
                Assert.IsTrue(result.Outposts.Count < 33, "Tooooo many outposts on map");
                setHelper.Clear();
                foreach (int index in GetNodesWithin(ToVector2(result.Outposts), mapPos, allyOutpostsMaxWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                result.FlagList = new List<int>();
                result.LongFlagList = new List<long>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    int value = 0;
                    long value2 = 0L;
                    intHelper.Clear();
                    intHelper.AddRange(GetNodesWithin(ToVector2(result.Outposts), result.Nodes[i], fleetMaxWorldSqrDistanceFromAllyOutposts));
                    intHelper2.Clear();
                    intHelper2.AddRange(intHelper);
                    foreach (int index in GetNodesWithin(ToVector2(result.Outposts), result.Nodes[i], fleetMinWorldSqrDistanceFromAllyOutposts))
                    {
                        intHelper2.Remove(index);
                    }
                    foreach (int index in intHelper2)
                    {
                        value |= 1 << index;
                    }
                    foreach (int index in intHelper)
                    {
                        value2 |= 1L << index;
                    }
                    result.FlagList.Add(value);
                    result.LongFlagList.Add(value2);
                }
                result.TripletList = new List<int>();
                for (int i = 0; i < result.Outposts.Count; i++)
                {
                    int iFlag = 1 << i;
                    for (int j = i + 1; j < result.Outposts.Count; j++)
                    {
                        if (!setHelper.Contains(i) && !setHelper.Contains(j))
                        {
                            continue;
                        }
                        if (Vector2.SqrMagnitude((Vector2)result.Outposts[i] - result.Outposts[j]) < allyOutpostsWorldSqrDistanceBetween)
                        {
                            continue;
                        }

                        flag = iFlag | (1 << j);
                        if (FlagCount(result.FlagList, flag) > 3)
                        {
                            result.TripletList.Add(flag);
                        }
                    }
                }
                if (result.TripletList.Count == 0)
                {
                    return null;
                }
                result.AdditionalInt = new List<int>(setHelper);
                return result;
            case ESandboxObjectiveType.DefendFleet:
                return CanDefendFleet(result, mapPos, 9, defendFleetMaxWorldSqrDistance, defendFleetTargetMinWorldSqrDistance);
            case ESandboxObjectiveType.DefendMultipleFleets:
                result = CanDefendFleet(result, mapPos, 11, defendFleetsMaxWorldSqrDistance, defendFleetsTargetMinWorldSqrDistance);
                if (result == null)
                {
                    return null;
                }
                result.TripletList = new List<int>();
                for (int i = 0; i < result.AdditionalInt.Count; i++)
                {
                    for (int j = i + 1; j < result.AdditionalInt.Count; j++)
                    {
                        if (Vector3.SqrMagnitude((Vector2)result.Nodes[result.AdditionalInt[i]] - result.Nodes[result.AdditionalInt[j]]) > defendFleetsMinWorldSqrDistanceBetween)
                        {
                            int flag1 = 1 << i;
                            int flag2 = 1 << j;
                            foreach (int flags in result.FlagList)
                            {
                                if (((flags & flag1) == 0) != ((flags & flag2) == 0))
                                {
                                    result.TripletList.Add(flag1 | flag2);
                                    break;
                                }
                            }
                        }
                    }
                }
                return result.TripletList.Count > 0 ? result : null;
            case ESandboxObjectiveType.DefendAllyTimed:
                return CanDefendFleet(result, mapPos, 10, timeDefendFleetMaxWorldSqrDistance, timeDefendFleetTargetMinWorldSqrDistance);
            case ESandboxObjectiveType.EscortRetreat:
                if (result.Nodes.Count < 9)
                {
                    Debug.Log("nodes");
                    return null;
                }
                result = CheckAddStartNodes(result, startNodes, distances, escortRetreatSwimSqrDistance, new List<MyVector2>(ToMyVector2(edgeNodes)), 2, 4);
                return (result != null && result.AdditionalInt.Count > 0) ? result : null;
            case ESandboxObjectiveType.EscortAttack:
                if (result.Nodes.Count < 10)
                {
                    return null;
                }
                result = CheckAddStartNodes(result, startNodes, distances, escortAttackSwimSqrDistance, null, 0, 4);
                return (result != null && result.AdditionalInt.Count > 0) ? result : null;
            case ESandboxObjectiveType.EscortInvasion:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }
                result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
                result = CheckAddStartNodes(result, startNodes, distances, escortInvasionSwimSqrDistance, new List<MyVector2>(ToMyVector2(outpostOnWaterNodes)), 1, 4);
                return (result != null && result.AdditionalInt.Count > 0) ? result : null;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                if (result.Nodes.Count < 6)
                {
                    return null;
                }
                result.AdditionalInt = new List<int>();
                for (int i = 0; i < destroyUndetectedFleetWorldSqrDistances.Count; i++)
                {
                    float distance = destroyUndetectedFleetWorldSqrDistances[i];
                    result.AdditionalInt.Add(-1);
                    float diff = float.PositiveInfinity;
                    for (int j = 0; j < result.Nodes.Count; j++)
                    {
                        float newDiff = Mathf.Abs(Vector2.SqrMagnitude(mapPos - result.Nodes[j]) - distance);
                        if (diff > newDiff)
                        {
                            diff = newDiff;
                            result.AdditionalInt[i] = j;
                        }
                    }
                    if (diff == float.PositiveInfinity)
                    {
                        return null;
                    }
                }
                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                setHelper.Remove(result.AdditionalInt[0]);
                setHelper.Remove(result.AdditionalInt[1]);
                setHelper.Remove(result.AdditionalInt[2]);
                if (setHelper.Count < maxAdditionalEnemies)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);

                return result;
            case ESandboxObjectiveType.DefendBaseUndetected:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }

                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.FlagList.Add(0);
                }

                result.AdditionalInt = new List<int>();
                result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
                Assert.IsTrue(result.Outposts.Count < 32, "too many outposts");
                for (int i = 0; i < result.Outposts.Count; i++)
                {
                    float dist = Vector2.SqrMagnitude(mapPos - result.Outposts[i]);
                    if (dist < defendUndetectedOutpostMinWorldSqrDistance || dist >= defendUndetectedOutpostMaxWorldSqrDistance)
                    {
                        continue;
                    }
                    int count = 0;
                    flag = 1 << i;
                    foreach (int index in GetNodesWithin(ToVector2(result.Nodes), result.Outposts[i], defendUndetectedEnemyMaxWorldSqrDistanceFromOutpost))
                    {
                        if (Vector2.SqrMagnitude(result.Nodes[index] - mapPos) > undetectedMinEnemyWorldSqrDistance)
                        {
                            count++;
                            result.FlagList[index] |= flag;
                        }
                    }
                    if (count > 2)
                    {
                        result.AdditionalInt.Add(i);
                    }
                }
                if (result.AdditionalInt.Count == 0)
                {
                    return null;
                }

                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                if (setHelper.Count < 7)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);

                return result;
            case ESandboxObjectiveType.DefendFleetUndetected:
                if (result.Nodes.Count < 9)
                {
                    return null;
                }
                result.FlagList = new List<int>();
                for (int i = 0; i < result.Nodes.Count; i++)
                {
                    result.FlagList.Add(0);
                }

                result.AdditionalInt = new List<int>();
                flag = 1;
                foreach (int index in GetNodesWithin(ToVector2(result.Nodes), mapPos, defendFleetUndetectedMaxFleetWorldSqrDistance))
                {
                    bool ok = false;
                    foreach (int index2 in GetNodesWithout(ToVector2(result.Nodes), result.Nodes[index], defendFleetUndetectedTargetMinWorldSqrDistance))
                    {
                        ok = true;
                        result.FlagList[index2] |= flag;
                    }
                    if (ok)
                    {
                        Assert.IsTrue(result.AdditionalInt.Count < 31, "too many");
                        result.AdditionalInt.Add(index);
                        flag = 1 << result.AdditionalInt.Count;
                    }
                }

                if (result.AdditionalInt.Count == 0)
                {
                    return null;
                }
                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                if (setHelper.Count < 7)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);
                return result;
            case ESandboxObjectiveType.Scout:
                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                if (setHelper.Count < maxAdditionalEnemies)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);
                return CanScout(result, 9, 8, mapPos, undetectedMinEnemyWorldSqrDistance);
            case ESandboxObjectiveType.ScoutBase:
                result = CanScoutOutpost(result, outpostNodes, mapPos, undetectedMinEnemyWorldSqrDistance);
                if (result == null)
                {
                    return null;
                }

                setHelper.Clear();
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                if (setHelper.Count < maxAdditionalEnemies)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);
                return result;
            case ESandboxObjectiveType.Patrol:
                return CanScout(result, 13, 4, mapPos, patrolEnemyMinWorldSqrDistance);
            case ESandboxObjectiveType.PatrolBase:
                return CanScoutOutpost(result, outpostNodes, mapPos, patrolEnemyMinWorldSqrDistance);
            case ESandboxObjectiveType.PatrolUndetected:
                if (result.Nodes.Count < 10)
                {
                    return null;
                }
                result.AdditionalInt = new List<int>(GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance));
                if (result.AdditionalInt.Count < 6)
                {
                    return null;
                }
                result.TripletList = new List<int>(GetNodesWithout(ToVector2(result.Nodes), mapPos, patrolUndetectedUOMinWorldSqrDistance));
                return result.TripletList.Count > 4 ? result : null;
            case ESandboxObjectiveType.UndetectedPatrolBase:
                if (result.Nodes.Count < 8)
                {
                    return null;
                }
                result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
                result.AdditionalInt = new List<int>(GetNodesWithout(ToVector2(result.Outposts), mapPos, undetectedMinEnemyWorldSqrDistance));
                if (result.AdditionalInt.Count < 2)
                {
                    return null;
                }
                result.TripletList = new List<int>(GetNodesWithout(ToVector2(result.Nodes), mapPos, patrolUndetectedUOMinWorldSqrDistance));
                if (result.TripletList.Count < 3)
                {
                    return null;
                }
                foreach (int index in GetNodesWithout(ToVector2(result.Nodes), mapPos, undetectedMinEnemyWorldSqrDistance))
                {
                    setHelper.Add(index);
                }
                if (setHelper.Count < maxAdditionalEnemies)
                {
                    return null;
                }
                result.Undetected = new List<int>(setHelper);
                return result;
            case ESandboxObjectiveType.EnemyFleetInstance:
                if (result.Nodes.Count < 6)
                {
                    return null;
                }
                result.AdditionalInt = new List<int>(GetNodesWithin(ToVector2(result.Nodes), mapPos, redWatersMaxWorldSqrDistance));
                return result.AdditionalInt.Count > 0 ? result : null;
            default:
                Debug.Log(type.ToString());
                Assert.IsTrue(false);
                return null;
        }
    }
#endif
    public EEnemiesCount Init(MapSpawnData data, EMissionDifficulty difficulty, int overrideFleets = -1)
    {
        data.Difficulty = difficulty;
        if (overrideFleets == -1)
        {
            data.AdditionalFleets = data.Type == ESandboxObjectiveType.EnemyFleetInstance ? 0 : UnityRandom.Range(0, maxAdditionalEnemies);
        }
        else
        {
            data.AdditionalFleets = overrideFleets;
        }
        data.EnemiesCount = GetEnemiesCount(data);
        return data.EnemiesCount;
    }

    public IEnumerable<string> GetObjectives(ESandboxObjectiveType type, MapSpawnData data)
    {
        var locMan = LocalizationManager.Instance;
        switch (type)
        {
            case ESandboxObjectiveType.DestroyBase:
                yield return locMan.GetText(destroyOutpostID);
                yield return string.IsNullOrWhiteSpace(destroyOutpostDescID) ? "" : locMan.GetText(destroyOutpostDescID);
                break;
            case ESandboxObjectiveType.DestroyMultipleBases:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(destroyOutpost2ID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(destroyOutpost2DescID[0]) ? "" : locMan.GetText(destroyOutpost2DescID[0], "0", "2");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(destroyOutpost2ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(destroyOutpost2DescID[0]) ? "" : locMan.GetText(destroyOutpost2DescID[0], "0", "3");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.SeekAndDestroy:
                yield return locMan.GetText(seekAndDestroySeekID);
                yield return string.IsNullOrWhiteSpace(seekAndDestroySeekDescID) ? "" : locMan.GetText(seekAndDestroySeekDescID);
                yield return locMan.GetText(seekAndDestroyDestroyID);
                yield return string.IsNullOrWhiteSpace(seekAndDestroyDestroyDescID) ? "" : locMan.GetText(seekAndDestroyDestroyDescID);
                break;
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(seekAndDestroy2SeekID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(seekAndDestroy2SeekDescID[0]) ? "" : locMan.GetText(seekAndDestroy2SeekDescID[0], "0", "2");
                        yield return locMan.GetText(seekAndDestroy2DestroyID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(seekAndDestroy2DestroyDescID[0]) ? "" : locMan.GetText(seekAndDestroy2DestroyDescID[0], "0", "2");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(seekAndDestroy2SeekID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(seekAndDestroy2SeekDescID[0]) ? "" : locMan.GetText(seekAndDestroy2SeekDescID[0], "0", "3");
                        yield return locMan.GetText(seekAndDestroy2DestroyID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(seekAndDestroy2DestroyDescID[0]) ? "" : locMan.GetText(seekAndDestroy2DestroyDescID[0], "0", "3");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DestroyTimedFleets:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(destroyTimedFleets2ID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(destroyTimedFleets2DescID[0]) ? "" : locMan.GetText(destroyTimedFleets2DescID[0], "0", "2");
                        break;
                    case EMissionDifficulty.Medium:
                        yield return locMan.GetText(destroyTimedFleets2ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(destroyTimedFleets2DescID[0]) ? "" : locMan.GetText(destroyTimedFleets2DescID[0], "0", "3");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DestroyChase:
                yield return locMan.GetText(destroyChaseID);
                yield return string.IsNullOrWhiteSpace(destroyChaseDescID) ? "" : locMan.GetText(destroyChaseDescID);
                break;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                yield return locMan.GetText(destroyChase2ID[0], "0", "2");
                yield return string.IsNullOrWhiteSpace(destroyChase2DescID[0]) ? "" : locMan.GetText(destroyChase2DescID[0], "0", "2");
                break;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        yield return locMan.GetText(retrieveTimedPlanes2ID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(retrieveTimedPlanes2DescID[0]) ? "" : locMan.GetText(retrieveTimedPlanes2DescID[0], "0", "2");
                        break;
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(retrieveTimedPlanes2ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(retrieveTimedPlanes2DescID[0]) ? "" : locMan.GetText(retrieveTimedPlanes2DescID[0], "0", "3");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(retrieveSurvivorsID[0], "0", "2");
                        yield return string.IsNullOrWhiteSpace(retrieveSurvivorsDescID[0]) ? "" : locMan.GetText(retrieveSurvivorsDescID[0], "0", "2");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(retrieveSurvivorsID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(retrieveSurvivorsDescID[0]) ? "" : locMan.GetText(retrieveSurvivorsDescID[0], "0", "3");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(retrieveSurvivorsUnknownID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(retrieveSurvivorsUnknownDescID[0]) ? "" : locMan.GetText(retrieveSurvivorsUnknownDescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(retrieveSurvivorsUnknownID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(retrieveSurvivorsUnknownDescID[0]) ? "" : locMan.GetText(retrieveSurvivorsUnknownDescID[0], "0", "4");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DefendBase:
                var text = (defendOutpostTicks[(int)data.Difficulty] / 180).ToString();
                yield return locMan.GetText(defendOutpostRepelID, text);
                yield return string.IsNullOrWhiteSpace(defendOutpostRepelDescID) ? "" : locMan.GetText(defendOutpostRepelDescID, text);
                yield return locMan.GetText(defendOutpostSurviveID);
                yield return string.IsNullOrWhiteSpace(defendOutpostSurviveDescID) ? "" : locMan.GetText(defendOutpostSurviveDescID);
                break;
            case ESandboxObjectiveType.DefendMultipleBases:
                var text2 = (defendOutpostsTicks[(int)data.Difficulty] / 180).ToString();
                yield return locMan.GetText(defendOutpostsRepelID, text2);
                yield return string.IsNullOrWhiteSpace(defendOutpostsRepelDescID) ? "" : locMan.GetText(defendOutpostsRepelDescID, text2);
                yield return locMan.GetText(defendOutpostsSurviveID);
                yield return string.IsNullOrWhiteSpace(defendOutpostsSurviveDescID) ? "" : locMan.GetText(defendOutpostsSurviveDescID);
                break;
            case ESandboxObjectiveType.DefendFleet:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        string text10 = (1 + data.AdditionalFleets).ToString();
                        yield return locMan.GetText(defendFleetDestroyEnemy7ID[0], "0", text10);
                        yield return string.IsNullOrWhiteSpace(defendFleetDestroyEnemy7DescID[0]) ? "" : locMan.GetText(defendFleetDestroyEnemy7DescID[0], "0", text10);
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        string text11 = (2 + data.AdditionalFleets).ToString();
                        yield return locMan.GetText(defendFleetDestroyEnemy7ID[0], "0", text11);
                        yield return string.IsNullOrWhiteSpace(defendFleetDestroyEnemy7DescID[0]) ? "" : locMan.GetText(defendFleetDestroyEnemy7DescID[0], "0", text11);
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(defendFleetProtectID);
                yield return string.IsNullOrWhiteSpace(defendFleetProtectDescID) ? "" : locMan.GetText(defendFleetProtectDescID);
                break;
            case ESandboxObjectiveType.DefendMultipleFleets:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        string text10 = (1 + data.AdditionalFleets).ToString();
                        yield return locMan.GetText(defendFleetDestroyEnemy7ID[0], "0", text10);
                        yield return string.IsNullOrWhiteSpace(defendFleetDestroyEnemy7DescID[0]) ? "" : locMan.GetText(defendFleetDestroyEnemy7DescID[0], "0", "1");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        string text11 = (2 + data.AdditionalFleets).ToString();
                        yield return locMan.GetText(defendFleetDestroyEnemy7ID[0], "0", text11);
                        yield return string.IsNullOrWhiteSpace(defendFleetDestroyEnemy7DescID[0]) ? "" : locMan.GetText(defendFleetDestroyEnemy7DescID[0], "0", "2");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(defendFleetsProtectID);
                yield return string.IsNullOrWhiteSpace(defendFleetsProtectDescID) ? "" : locMan.GetText(defendFleetsProtectDescID);
                break;
            case ESandboxObjectiveType.DefendAllyTimed:
                var text7 = (timeDefendFleetTicks[(int)data.Difficulty] / 180).ToString();
                yield return locMan.GetText(timeDefendFleetRepelID, text7);
                yield return string.IsNullOrWhiteSpace(timeDefendFleetRepelDescID) ? "" : locMan.GetText(timeDefendFleetRepelDescID, text7);
                yield return locMan.GetText(timeDefendFleetProtectAllyID);
                yield return string.IsNullOrWhiteSpace(timeDefendFleetProtectAllyDescID) ? "" : locMan.GetText(timeDefendFleetProtectAllyDescID);
                break;
            case ESandboxObjectiveType.EscortRetreat:
                yield return locMan.GetText(escortRetreatEscortFleetID);
                yield return string.IsNullOrWhiteSpace(escortRetreatEscortFleetDescID) ? "" : locMan.GetText(escortRetreatEscortFleetDescID);
                yield return locMan.GetText(escortRetreatProtectAllyID);
                yield return string.IsNullOrWhiteSpace(escortRetreatProtectAllyDescID) ? "" : locMan.GetText(escortRetreatProtectAllyDescID);
                break;
            case ESandboxObjectiveType.EscortAttack:
                yield return locMan.GetText(escortAttackEscortFleetID);
                yield return string.IsNullOrWhiteSpace(escortAttackEscortFleetDescID) ? "" : locMan.GetText(escortAttackEscortFleetDescID);
                yield return locMan.GetText(escortAttackProtectAllyID);
                yield return string.IsNullOrWhiteSpace(escortAttackProtectAllyDescID) ? "" : locMan.GetText(escortAttackProtectAllyDescID);
                break;
            case ESandboxObjectiveType.EscortInvasion:
                yield return locMan.GetText(escortInvasionEscortFleetID);
                yield return string.IsNullOrWhiteSpace(escortInvasionEscortFleetDescID) ? "" : locMan.GetText(escortInvasionEscortFleetDescID);
                yield return locMan.GetText(escortInvasionProtectAllyID);
                yield return string.IsNullOrWhiteSpace(escortInvasionProtectAllyDescID) ? "" : locMan.GetText(escortInvasionProtectAllyDescID);
                break;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                yield return locMan.GetText(destroyUndetectedDestroyID);
                yield return string.IsNullOrWhiteSpace(destroyUndetectedDestroyDescID) ? "" : locMan.GetText(destroyUndetectedDestroyDescID);
                yield return locMan.GetText(destroyUndetectedStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(destroyUndetectedStayUndetectedDescID) ? "" : locMan.GetText(destroyUndetectedStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.DefendBaseUndetected:
                var text3 = (defendUndetectedTicks[(int)data.Difficulty] / 180).ToString();
                yield return locMan.GetText(defendUndetectedDefendTime3ID[0], text3);
                yield return string.IsNullOrWhiteSpace(defendUndetectedDefendTime3DescID[0]) ? "" : locMan.GetText(defendUndetectedDefendTime3DescID[0], text3);
                yield return locMan.GetText(defendUndetectedStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(defendUndetectedStayUndetectedDescID) ? "" : locMan.GetText(defendUndetectedStayUndetectedDescID);
                yield return locMan.GetText(defendUndetectedProtectOutpostID);
                yield return string.IsNullOrWhiteSpace(defendUndetectedFleetStayUndetectedDescID) ? "" : locMan.GetText(defendUndetectedFleetStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.DefendFleetUndetected:
                var text4 = (defendUndetectedFleetTicks[(int)data.Difficulty] / 180).ToString();
                yield return locMan.GetText(defendUndetectedFleetDefendTime3ID[0], text4);
                yield return string.IsNullOrWhiteSpace(defendUndetectedFleetDefendTime3DescID[0]) ? "" : locMan.GetText(defendUndetectedFleetDefendTime3DescID[0], text4);
                yield return locMan.GetText(defendUndetectedFleetStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(defendUndetectedFleetStayUndetectedDescID) ? "" : locMan.GetText(defendUndetectedFleetStayUndetectedDescID);
                yield return locMan.GetText(defendUndetectedFleetProtectFleetID);
                yield return string.IsNullOrWhiteSpace(defendUndetectedFleetProtectFleetDescID) ? "" : locMan.GetText(defendUndetectedFleetProtectFleetDescID);
                break;
            case ESandboxObjectiveType.Scout:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(scoutDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(scoutDiscover3DescID[0]) ? "" : locMan.GetText(scoutDiscover3DescID[0], "0", "3");
                        yield return locMan.GetText(scoutIdentify3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(scoutIdentify3DescID[0]) ? "" : locMan.GetText(scoutIdentify3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(scoutDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(scoutDiscover3DescID[0]) ? "" : locMan.GetText(scoutDiscover3DescID[0], "0", "4");
                        yield return locMan.GetText(scoutIdentify3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(scoutIdentify3DescID[0]) ? "" : locMan.GetText(scoutIdentify3DescID[0], "0", "4");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(scoutStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(scoutStayUndetectedDescID) ? "" : locMan.GetText(scoutStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.ScoutBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(scoutOutpostDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(scoutOutpostDiscover3DescID[0]) ? "" : locMan.GetText(scoutOutpostDiscover3DescID[0], "0", "3");
                        yield return locMan.GetText(scoutOutpostIdentify3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(scoutOutpostIdentify3DescID[0]) ? "" : locMan.GetText(scoutOutpostIdentify3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(scoutOutpostDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(scoutOutpostDiscover3DescID[0]) ? "" : locMan.GetText(scoutOutpostDiscover3DescID[0], "0", "4");
                        yield return locMan.GetText(scoutOutpostIdentify3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(scoutOutpostIdentify3DescID[0]) ? "" : locMan.GetText(scoutOutpostIdentify3DescID[0], "0", "4");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(scoutOutpostStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(scoutOutpostStayUndetectedDescID) ? "" : locMan.GetText(scoutOutpostStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.Patrol:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(patrolDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolDiscover3DescID[0]) ? "" : locMan.GetText(patrolDiscover3DescID[0], "0", "3");
                        yield return locMan.GetText(patrolIdentify3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolIdentify3DescID[0]) ? "" : locMan.GetText(patrolIdentify3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(patrolDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolDiscover3DescID[0]) ? "" : locMan.GetText(patrolDiscover3DescID[0], "0", "4");
                        yield return locMan.GetText(patrolIdentify3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolIdentify3DescID[0]) ? "" : locMan.GetText(patrolIdentify3DescID[0], "0", "4");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.PatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(patrolOutpostDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolOutpostDiscover3DescID[0]) ? "" : locMan.GetText(patrolOutpostDiscover3DescID[0], "0", "3");
                        yield return locMan.GetText(patrolOutpostIdentify3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolOutpostIdentify3DescID[0]) ? "" : locMan.GetText(patrolOutpostIdentify3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(patrolOutpostDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolOutpostDiscover3DescID[0]) ? "" : locMan.GetText(patrolOutpostDiscover3DescID[0], "0", "4");
                        yield return locMan.GetText(patrolOutpostIdentify3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolOutpostIdentify3DescID[0]) ? "" : locMan.GetText(patrolOutpostIdentify3DescID[0], "0", "4");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.PatrolUndetected:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(patrolUndetectedDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedDiscover3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                        yield return locMan.GetText(patrolUndetectedDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedDiscover3DescID[0], "0", "4");
                        break;
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(patrolUndetectedDiscover3ID[0], "0", "5");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedDiscover3DescID[0], "0", "5");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(patrolUndetectedStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(patrolUndetectedStayUndetectedDescID) ? "" : locMan.GetText(patrolUndetectedStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.UndetectedPatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        yield return locMan.GetText(patrolUndetectedOutpostDiscover3ID[0], "0", "3");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedOutpostDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedOutpostDiscover3DescID[0], "0", "3");
                        break;
                    case EMissionDifficulty.Medium:
                        yield return locMan.GetText(patrolUndetectedOutpostDiscover3ID[0], "0", "4");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedOutpostDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedOutpostDiscover3DescID[0], "0", "4");
                        break;
                    case EMissionDifficulty.Hard:
                        yield return locMan.GetText(patrolUndetectedOutpostDiscover3ID[0], "0", "5");
                        yield return string.IsNullOrWhiteSpace(patrolUndetectedOutpostDiscover3DescID[0]) ? "" : locMan.GetText(patrolUndetectedOutpostDiscover3DescID[0], "0", "5");
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                yield return locMan.GetText(patrolUndetectedStayUndetectedID);
                yield return string.IsNullOrWhiteSpace(patrolUndetectedStayUndetectedDescID) ? "" : locMan.GetText(patrolUndetectedStayUndetectedDescID);
                break;
            case ESandboxObjectiveType.EnemyFleetInstance:
                yield return locMan.GetText(redWatersDestroyEnemyID);
                yield return string.IsNullOrWhiteSpace(redWatersDestroyEnemyDescID) ? "" : locMan.GetText(redWatersDestroyEnemyDescID);
                break;
            default:
                Assert.IsTrue(false);
                yield break;
        }
    }

    public SOTacticMap Spawn(MapSpawnData data)
    {
        var diff = Init(data);
        int fleetCount = 0;
        int helper;

        var objData = MainObjective(out var winEffect);

        List<EnemyUnitData> possibles = null;
        int ticks;
        bool undetected = false;

        switch (data.Type)
        {
            case ESandboxObjectiveType.DestroyBase:
                int outpostIndex = CreateOutpost(data.Outposts, diff);
                GetNameFromBasket(dummy.EnemyUnits[0], enemyOutpostNamesBucket, enemyOutpostNames);
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        break;
                    case EMissionDifficulty.Medium:
                        fleetCount = 1;
                        break;
                    case EMissionDifficulty.Hard:
                        fleetCount = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                CreateFleet(fleetCount, dummy.Nodes, FlagList(data.FlagList, outpostIndex));
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i + 1], enemyFleetNamesBucket, enemyFleetNames);
                }

                Destroy1(destroyOutpostID, destroyOutpostDescID, objData);
                break;
            case ESandboxObjectiveType.DestroyMultipleBases:
                intHelper.Clear();
                intHelper.AddRange(GetFlagTripletIndices(RandomUtils.GetRandom(data.TripletList)));
                Assert.IsTrue(intHelper.Count == 3);
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        RemoveFromTriplet(intHelper, data.AdditionalInt);

                        fleetCount = 2;
                        TargetiveObjective(EObjectiveType.Destroy, destroyOutpost2ID, destroyOutpost2DescID, objData, 0, 1);
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        fleetCount = 3;
                        TargetiveObjective(EObjectiveType.Destroy, destroyOutpost3ID, destroyOutpost3DescID, objData, 0, 1, 2);
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < intHelper.Count; i++)
                {
                    CreateOutpost(data.Outposts[intHelper[i]], diff);
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyOutpostNamesBucket, enemyOutpostNames);
                }
                CreateFleet(fleetCount, dummy.Nodes, FlagListMultipleOutposts(data.FlagList, intHelper));
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[dummy.EnemyUnits.Count - i - 1], enemyFleetNamesBucket, enemyFleetNames);
                }
                break;
            case ESandboxObjectiveType.SeekAndDestroy:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        break;
                    case EMissionDifficulty.Medium:
                        dummy.AdditionalObjectsToSpawnMin += 2;
                        break;
                    case EMissionDifficulty.Hard:
                        dummy.AdditionalObjectsToSpawnMin += 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                CreateFleetAllNodes(1, dummy.Nodes, FlagList(data.FlagList, (int)data.Difficulty));
                GetNameFromBasket(dummy.EnemyUnits[0], enemyFleetNamesBucket, enemyFleetNames);

                var newObjData = new ObjectiveData();
                newObjData.Effects = new List<ObjectiveEffectData>();
                Destroy1(seekAndDestroySeekID, seekAndDestroySeekDescID, newObjData);
                newObjData.Type = EObjectiveType.Reveal;
                newObjData.Active = true;
                newObjData.Visible = true;

                Destroy1(seekAndDestroyDestroyID, seekAndDestroyDestroyDescID, objData);
                break;
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
                int flag;
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        flag = 0;
                        fleetCount = 2;

                        objData.Effects.Clear();
                        TargetiveObjective(EObjectiveType.Reveal, seekAndDestroy2SeekID, seekAndDestroy2SeekDescID, objData, 0, 1);

                        objData = new ObjectiveData();
                        objData.Visible = true;
                        objData.Active = true;
                        objData.Effects = new List<ObjectiveEffectData>() { winEffect };
                        TargetiveObjective(EObjectiveType.Destroy, seekAndDestroy2DestroyID, seekAndDestroy2DestroyDescID, objData, false, 0, 1);
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        flag = data.Difficulty == EMissionDifficulty.Medium ? 0 : 1;
                        fleetCount = 3;

                        objData.Effects.Clear();
                        TargetiveObjective(EObjectiveType.Reveal, seekAndDestroy3SeekID, seekAndDestroy3SeekDescID, objData, 0, 1, 2);

                        objData = new ObjectiveData();
                        objData.Visible = true;
                        objData.Active = true;
                        objData.Effects = new List<ObjectiveEffectData>() { winEffect };
                        TargetiveObjective(EObjectiveType.Destroy, seekAndDestroy3DestroyID, seekAndDestroy3DestroyDescID, objData, false, 0, 1, 2);
                        break;
                    default:
                        flag = -1;
                        Assert.IsTrue(false);
                        break;
                }
                CreateFleetAllNodes(fleetCount, dummy.Nodes, FlagList(data.FlagList, flag));
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }
                break;
            case ESandboxObjectiveType.DestroyTimedFleets:
                helper = DestroyTimed(destroyTimedFleets2ID, destroyTimedFleets2DescID, destroyTimedFleets3ID, destroyTimedFleets3DescID, destroyTimedTimerIDs, objData, data, diff);
                CreateFleetAllNodes(helper, dummy.Nodes, data.FlagList);
                for (int i = 0; i < helper; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }
                break;
            case ESandboxObjectiveType.DestroyChase:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                Destroy1(destroyChaseID, destroyChaseDescID, objData);

                intHelper3.Clear();
                intHelper3.Add(UnityRandom.Range(0, data.AdditionalInt.Count));
                DestroyChase(destroyChaseSpeeds[(int)data.Difficulty], intHelper3, data, diff);
                break;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                TargetiveObjective(EObjectiveType.Destroy, destroyChase2ID, destroyChase2DescID, objData, 0, 1);

                intHelper3.Clear();

                int dublet = RandomUtils.GetRandom(data.TripletList);
                for (int i = 0; i < 32; i++)
                {
                    if ((dublet & (1 << i)) != 0)
                    {
                        intHelper3.Add(i);
                    }
                }
                Assert.IsTrue(intHelper3.Count == 2);
                DestroyChase(destroyChaseMultipleSpeeds[(int)data.Difficulty], intHelper3, data, diff);
                break;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
                intHelper.Clear();
                intHelper.AddRange(GetTripletIndices(RandomUtils.GetRandom(data.TripletList), 10));
                Assert.IsTrue(intHelper.Count == 3);
                objData.Active = false;
                objData.Effects.Clear();
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        foreach (var newObjData2 in ObjectivesSetText(2, retrieveTimedPlanes2ID, retrieveTimedPlanes2DescID, objData))
                        {
                            newObjData2.Type = EObjectiveType.FinishCustomMission;
                            newObjData2.Active = false;
                        }
                        objData = dummy.Objectives[2];
                        objData.Active = true;
                        objData.Effects.Add(ActivateEffect(true, 1));
                        if (data.Difficulty == EMissionDifficulty.Medium)
                        {
                            fleetCount = 2;
                        }
                        RemoveFromTriplet(intHelper, data.AdditionalInt);
                        break;
                    case EMissionDifficulty.Hard:
                        foreach (var newObjData2 in ObjectivesSetText(3, retrieveTimedPlanes3ID, retrieveTimedPlanes3DescID, objData))
                        {
                            newObjData2.Type = EObjectiveType.FinishCustomMission;
                            newObjData2.Active = false;
                        }
                        objData = dummy.Objectives[2];
                        objData.Active = true;
                        objData.Effects.Add(ActivateEffect(true, 3));
                        objData = dummy.Objectives[3];
                        objData.Effects.Add(ActivateEffect(true, 1));
                        fleetCount = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }

                objData = dummy.Objectives[1];
                objData.Effects.Add(FinishEffect(true, 0));
                objData.Effects.Add(winEffect);

                objData = new ObjectiveData();
                objData.Type = EObjectiveType.Instant;
                objData.Active = true;
                objData.Effects = new List<ObjectiveEffectData>();
                int hours = retrieveTimedPlanesHours[(int)data.Difficulty];
                foreach (int index in intHelper)
                {
                    var planes = RandomUtils.GetRandom(retrievePlanesOptions);
                    objData.Effects.Add(CustomMissionEffect(index, hours, planes.Bombers, planes.Fighters, planes.Torpedoes));
                }
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                TimeFailObjective(ref objData, hours * 180, EMissionLoseCause.SandboxTimeout);

                if (fleetCount > 0)
                {
                    setHelper.Clear();
                    foreach (int index in intHelper)
                    {
                        setHelper.Add(index);
                    }
                    CreateFleet(fleetCount, dummy.Nodes, GetRetrievePlanesFleetNodes(dummy.Nodes, setHelper));
                    for (int i = 0; i < fleetCount; i++)
                    {
                        GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                    }
                }
                break;
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
                SpawnRetrieveTimedSurvivorsUnknownArea(data, ref objData, false, retrieveSurvivorsID, retrieveSurvivorsDescID);
                Assert.IsTrue(objData.Type == EObjectiveType.Instant);

                helper = retrieveSurvivorsTicks[(int)data.Difficulty];
                objData.Effects.Add(TimerEffect(true, helper, retrieveSurvivorsTimerIDs[0], retrieveSurvivorsTimerIDs[1], retrieveSurvivorsTimerIDs[2]));
                objData.Effects.Add(ShowSurvivorsEffect());

                int objID = dummy.Objectives.Count;
                objData = new ObjectiveData();
                TimeFailObjective(ref objData, helper, EMissionLoseCause.SandboxSurvivorsDrowned);
                objData.Effects.Add(ActivateEffect(false, dummy.Objectives.Count));

                objData = new ObjectiveData();
                Objective(ref objData, EObjectiveType.Time, helper + 1);
                objData.Active = true;
                objData.Effects = new List<ObjectiveEffectData>() { winEffect };
                dummy.Objectives.Add(objData);

                objData = dummy.Objectives[0];
                objData.Effects.Clear();
                objData.Effects.Add(ActivateEffect(false, objID));

                var objData2 = new ObjectiveData();
                RescueSurvivorsObjective(ref objData2, objData.Count + 1);
                objData2.Active = true;
                objData2.Effects = new List<ObjectiveEffectData>() { winEffect };
                dummy.Objectives.Add(objData2);
                break;
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                SpawnRetrieveTimedSurvivorsUnknownArea(data, ref objData, true, retrieveSurvivorsUnknownID, retrieveSurvivorsUnknownDescID);
                break;
            case ESandboxObjectiveType.DefendBase:
                int outpost = CreateOutpost(RandomUtils.GetRandom(possibleOutposts2).Duplicate(), data.Outposts, diff);
                dummy.EnemyUnits[0].IsAlly = true;
                GetNameFromBasket(dummy.EnemyUnits[0], friendlyOutpostNamesBucket, friendlyOutpostNames);

                helper = defendOutpostTicks[(int)data.Difficulty];
                TimeObjective(ref objData, helper, defendOutpostRepelID, defendOutpostRepelDescID);
                objData.Params = new string[] { (helper / 180).ToString() };
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                DummyObjective(ref objData, defendOutpostSurviveID, defendOutpostSurviveDescID);
                objData.ObjectiveTargetIDs.Add(0);
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                TargetiveObjective(ref objData, EObjectiveType.Destroy, EObjectiveTarget.Number, 1, 0);
                objData.LoseType = EMissionLoseCause.SandboxOutpostDestroyed;
                objData.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
                objData.Active = true;
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                InstantObjective(ref objData, ShowAllyEffect(0), TimerEffect(true, helper, defendOutpostTimerIDs[0], defendOutpostTimerIDs[1], defendOutpostTimerIDs[2]));
                dummy.Objectives.Add(objData);

                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        fleetCount = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        fleetCount = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }

                CreateFleet(fleetCount, dummy.Nodes, FlagList(data.FlagList, outpost), LongFlagList(data.LongFlagList, outpost));
                for (int i = 1; i < dummy.EnemyUnits.Count; i++)
                {
                    dummy.EnemyUnits[i].CanChase = false;
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }
                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.DefendMultipleBases:
                intHelper4.Clear();
                intHelper4.AddRange(GetFlagTripletIndices(RandomUtils.GetRandom(data.TripletList)));
                Assert.IsTrue(intHelper4.Count == 2);
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        possibles = possibleOutposts3;
                        fleetCount = 3;
                        break;
                    case EMissionDifficulty.Medium:
                        possibles = possibleOutposts3;
                        fleetCount = 4;
                        break;
                    case EMissionDifficulty.Hard:
                        possibles = possibleOutposts2;
                        fleetCount = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                foreach (int index in intHelper4)
                {
                    CreateOutpost(RandomUtils.GetRandom(possibles), data.Outposts[index], diff);
                }
                foreach (var allyOutpost in dummy.EnemyUnits)
                {
                    allyOutpost.IsAlly = true;
                    GetNameFromBasket(allyOutpost, friendlyOutpostNamesBucket, friendlyOutpostNames);
                }
                CreateFleet(fleetCount, dummy.Nodes, FlagListMultipleOutposts(data.FlagList, intHelper4), FlagListMultipleOutposts(data.LongFlagList, intHelper4));
                for (int i = 2; i < dummy.EnemyUnits.Count; i++)
                {
                    dummy.EnemyUnits[i].CanChase = false;
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }

                helper = defendOutpostsTicks[(int)data.Difficulty];
                TimeObjective(ref objData, helper, defendOutpostsRepelID, defendOutpostsRepelDescID);
                objData.Params = new string[] { (helper / 180).ToString() };
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                DummyObjective(ref objData, defendOutpostsSurviveID, defendOutpostsSurviveDescID);
                objData.ObjectiveTargetIDs.Add(0);
                objData.ObjectiveTargetIDs.Add(1);
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                TargetiveObjective(ref objData, EObjectiveType.Destroy, EObjectiveTarget.Number, 1, 0, 1);
                objData.LoseType = EMissionLoseCause.SandboxOutpostDestroyed;
                objData.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
                objData.Active = true;
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                InstantObjective(ref objData, ShowAllyEffect(0, 1), TimerEffect(true, helper, defendOutpostsTimerIDs[0], defendOutpostsTimerIDs[1], defendOutpostsTimerIDs[2]));
                dummy.Objectives.Add(objData);

                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.DefendFleet:
                DefendFleetObjectives(data, ref objData, possibleFleets3, possibleFleets1,
                    defendFleetDestroyEnemy7ID, defendFleetDestroyEnemy7DescID, defendFleetProtectID, defendFleetProtectDescID, false, out fleetCount, out possibles);

                GetFillDefendFleetNodes(data);
                CreateFleet(fleetCount, dummy.Nodes, setHelper, intHelper3);
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }

                Assert.IsTrue(dummy.EnemyUnits.Count == fleetCount);
                CreateFleet(RandomUtils.GetRandom(possibles).Duplicate(), dummy.Nodes, intHelper4[0], intHelper4[0], intHelper4[1]);
                var friendlyFleet = dummy.EnemyUnits[fleetCount];
                friendlyFleet.IsAlly = true;
                GetNameFromBasket(friendlyFleet, friendlyFleetNamesBucket, friendlyFleetNames);

                objData.Effects.Add(ShowAllyEffect(fleetCount + data.AdditionalFleets));

                moveToBack.Add(fleetCount);
                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.DefendMultipleFleets:
                DefendFleetObjectives(data, ref objData, possibleFleets2, possibleFleets1,
                    defendFleetDestroyEnemy7ID, defendFleetDestroyEnemy7DescID, defendFleetsProtectID, defendFleetsProtectDescID, true, out fleetCount, out possibles);
                intHelper4.Clear();
                intHelper4.AddRange(GetFlagTripletIndices(RandomUtils.GetRandom(data.TripletList)));
                Assert.IsTrue(intHelper4.Count == 2);

                intHelper.Clear();
                intHelper.AddRange(FlagList(data.FlagList, intHelper4[0]));
                intHelper4.Add(RandomUtils.GetRandom(intHelper));
                intHelper.Clear();
                intHelper.AddRange(FlagList(data.FlagList, intHelper4[1]));
                intHelper.Remove(intHelper4[1]);
                intHelper4.Add(RandomUtils.GetRandom(intHelper));

                setHelper.Clear();
                intHelper3.Clear();
                for (int i = 0; i < dummy.Nodes.Count; i++)
                {
                    if (!intHelper4.Contains(i))
                    {
                        setHelper.Add(i);
                        intHelper3.Add(i);
                    }
                }
                CreateFleet(fleetCount, dummy.Nodes, setHelper, intHelper3);
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }

                Assert.IsTrue(dummy.EnemyUnits.Count == fleetCount);
                intHelper4[0] = data.AdditionalInt[intHelper4[0]];
                CreateFleet(RandomUtils.GetRandom(possibles).Duplicate(), dummy.Nodes, intHelper4[0], intHelper4[0], intHelper4[2]);

                var fleet = dummy.EnemyUnits[fleetCount];
                fleet.IsAlly = true;
                GetNameFromBasket(fleet, friendlyFleetNamesBucket, friendlyFleetNames);

                intHelper4[1] = data.AdditionalInt[intHelper4[1]];
                CreateFleet(RandomUtils.GetRandom(possibles).Duplicate(), dummy.Nodes, intHelper4[1], intHelper4[1], intHelper4[3]);
                fleet = dummy.EnemyUnits[fleetCount + 1];
                fleet.IsAlly = true;
                GetNameFromBasket(fleet, friendlyFleetNamesBucket, friendlyFleetNames);

                helper = fleetCount + data.AdditionalFleets;
                objData.Effects.Add(ShowAllyEffect(helper, helper + 1));

                moveToBack.Add(fleetCount);
                moveToBack.Add(fleetCount + 1);

                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.DefendAllyTimed:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                ticks = timeDefendFleetTicks[(int)data.Difficulty];
                GetFillDefendFleetNodes(data);

                TimeObjective(ref objData, ticks, timeDefendFleetRepelID, timeDefendFleetRepelDescID);
                objData.Params = new string[] { (ticks / 180).ToString() };
                dummy.Objectives.Add(objData);

                SpawnDefendAlly(data, timeDefendFleetProtectAllyID, timeDefendFleetProtectAllyDescID, possibleFleets2, possibleFleets1, possibleFleets1, 3,
                    TimerEffect(true, ticks, timeDefendFleetTimerID, timeDefendFleetTimerTooltipID, timeDefendFleetTimerTooltipDescID), DetectPlayerEffect());
                break;
            case ESandboxObjectiveType.EscortRetreat:
                SpawnEscort(data, ref objData, escortRetreatEscortFleetID, escortRetreatEscortFleetDescID, escortRetreatProtectAllyID, escortRetreatProtectAllyDescID,
                    possibleFleets2, possibleFleets2, possibleFleets1, 3);
                break;
            case ESandboxObjectiveType.EscortAttack:
                SpawnEscort(data, ref objData, escortAttackEscortFleetID, escortAttackEscortFleetDescID, escortAttackProtectAllyID, escortAttackProtectAllyDescID,
                    possibleFleets2, possibleFleets2, possibleFleets1, 3);
                break;
            case ESandboxObjectiveType.EscortInvasion:
                SpawnEscort(data, ref objData, escortInvasionEscortFleetID, escortInvasionEscortFleetDescID, escortInvasionProtectAllyID, escortInvasionProtectAllyDescID,
                    possibleFleets2, possibleFleets1, possibleFleets1, 2);
                break;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                Destroy1(destroyUndetectedDestroyID, destroyUndetectedDestroyDescID, objData);

                objData = new ObjectiveData();
                StayHiddenObjective(ref objData, destroyUndetectedStayUndetectedID, destroyUndetectedStayUndetectedDescID);
                dummy.Objectives.Add(objData);
                objData = new ObjectiveData();
                FailObjectiveTarget(ref objData, EMissionLoseCause.SandboxDetected, 1);
                dummy.Objectives.Add(objData);

                objData = new ObjectiveData();
                InstantObjective(ref objData, RevealEffect(0));
                dummy.Objectives.Add(objData);

                intHelper.Clear();
                for (int i = 0; i < dummy.Nodes.Count; i++)
                {
                    intHelper.Add(i);
                }
                CreateFleet(oneshotFleetBucket.Get().Duplicate(), dummy.Nodes, data.AdditionalInt[(int)data.Difficulty], intHelper);
                GetNameFromBasket(dummy.EnemyUnits[0], enemyFleetNamesBucket, enemyFleetNames);

                undetected = true;
                break;
            case ESandboxObjectiveType.DefendBaseUndetected:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                SpawnDefendUndetected(data, ref objData, defendUndetectedTicks, defendUndetectedDefendTime3ID, defendUndetectedDefendTime3DescID, defendUndetectedStayUndetectedID,
                    defendUndetectedStayUndetectedDescID, defendUndetectedProtectOutpostID, defendUndetectedProtectOutpostDescID, defendUndetectedTimerID, defendUndetectedTimerTooltipID,
                    defendUndetectedTimerTooltipDescID, EMissionLoseCause.SandboxOutpostDestroyed);

                possibles = data.Difficulty == EMissionDifficulty.Hard ? possibleOutposts1 : possibleOutposts2;

                helper = RandomUtils.GetRandom(data.AdditionalInt);
                CreateOutpost(RandomUtils.GetRandom(possibles).Duplicate(), data.Outposts[helper], diff);
                var friendlyOutpost = dummy.EnemyUnits[0];
                friendlyOutpost.IsAlly = true;
                GetNameFromBasket(friendlyOutpost, friendlyOutpostNamesBucket, friendlyOutpostNames);
                CreateFleet(2, dummy.Nodes, FlagList(data.FlagList, helper));
                for (int i = 0; i < 2; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i + 1], enemyFleetNamesBucket, enemyFleetNames);
                }

                undetected = true;
                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.DefendFleetUndetected:
                Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
                SpawnDefendUndetected(data, ref objData, defendUndetectedFleetTicks, defendUndetectedFleetDefendTime3ID, defendUndetectedFleetDefendTime3DescID, defendUndetectedFleetStayUndetectedID,
                    defendUndetectedFleetStayUndetectedDescID, defendUndetectedFleetProtectFleetID, defendUndetectedFleetProtectFleetDescID, defendUndetectedFleetTimerID, defendUndetectedFleetTimerTooltipID,
                    defendUndetectedFleetTimerTooltipDescID, EMissionLoseCause.SandboxAllyDestroyed);

                intHelper.Clear();
                intHelper.Add(RandomUtils.GetRandom(data.AdditionalInt));
                intHelper2.Clear();
                intHelper2.AddRange(FlagList(data.FlagList, data.AdditionalInt.IndexOf(intHelper[0])));
                intHelper.Add(RandomUtils.GetRandom(intHelper2));
                CreateFleet(RandomUtils.GetRandom(possibleFleets2).Duplicate(), dummy.Nodes, intHelper[0], intHelper);
                var fleet2 = dummy.EnemyUnits[0];
                fleet2.IsAlly = true;
                GetNameFromBasket(fleet2, friendlyFleetNamesBucket, friendlyFleetNames);

                setHelper.Clear();
                foreach (int index in data.Undetected)
                {
                    setHelper.Add(index);
                }
                setHelper.Remove(intHelper[0]);
                setHelper.Remove(intHelper[1]);

                CreateFleet(2, dummy.Nodes, setHelper);
                for (int i = 0; i < 2; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i + 1], enemyFleetNamesBucket, enemyFleetNames);
                }

                undetected = true;
                dummy.AllyAttacks = true;
                break;
            case ESandboxObjectiveType.Scout:
                fleetCount = ScoutMap(data, ref objData, scoutDiscover3ID, scoutDiscover3DescID, scoutIdentify3ID, scoutIdentify3DescID,
                    scoutDiscover4ID, scoutDiscover4DescID, scoutIdentify4ID, scoutIdentify4DescID, winEffect);
                StayUndetected(scoutStayUndetectedID, scoutStayUndetectedDescID);

                CreateFleetAllNodes(fleetCount, dummy.Nodes, data.AdditionalInt);
                foreach (var enemy in dummy.EnemyUnits)
                {
                    GetNameFromBasket(enemy, enemyFleetNamesBucket, enemyFleetNames);
                    enemy.OverrideChanceToReveal = 0f;
                }

                undetected = true;
                break;
            case ESandboxObjectiveType.ScoutBase:
                fleetCount = ScoutMap(data, ref objData, scoutOutpostDiscover3ID, scoutOutpostDiscover3DescID, scoutOutpostIdentify3ID, scoutOutpostIdentify3DescID,
                    scoutOutpostDiscover4ID, scoutOutpostDiscover4DescID, scoutOutpostIdentify4ID, scoutOutpostIdentify4DescID, winEffect);
                StayUndetected(scoutOutpostStayUndetectedID, scoutOutpostStayUndetectedDescID);

                intHelper4.Clear();
                intHelper4.AddRange(data.AdditionalInt);
                for (int i = 0; i < fleetCount; i++)
                {
                    CreateOutpost(data.Outposts[RandomUtils.GetRemoveRandom(intHelper4)], diff);
                }
                foreach (var enemy in dummy.EnemyUnits)
                {
                    enemy.OverrideChanceToReveal = 0f;
                    GetNameFromBasket(enemy, enemyOutpostNamesBucket, enemyOutpostNames);
                }

                undetected = true;
                break;
            case ESandboxObjectiveType.Patrol:
                fleetCount = ScoutMap(data, ref objData, patrolDiscover3ID, patrolDiscover3DescID, patrolIdentify3ID, patrolIdentify3DescID,
                    patrolDiscover4ID, patrolDiscover4DescID, patrolIdentify4ID, patrolIdentify4DescID, winEffect);

                dummy.AdditionalObjectsToSpawnMin += (int)data.Difficulty + 2;

                CreateFleetAllNodes(fleetCount, dummy.Nodes, data.AdditionalInt);
                foreach (var enemy in dummy.EnemyUnits)
                {
                    GetNameFromBasket(enemy, enemyFleetNamesBucket, enemyFleetNames);
                }
                break;
            case ESandboxObjectiveType.PatrolBase:
                fleetCount = ScoutMap(data, ref objData, patrolOutpostDiscover3ID, patrolOutpostDiscover3DescID, patrolOutpostIdentify3ID, patrolOutpostIdentify3DescID,
                    patrolOutpostDiscover4ID, patrolOutpostDiscover4DescID, patrolOutpostIdentify4ID, patrolOutpostIdentify4DescID, winEffect);

                dummy.AdditionalObjectsToSpawnMin += (int)data.Difficulty + 2;

                intHelper4.Clear();
                intHelper4.AddRange(data.AdditionalInt);
                for (int i = 0; i < fleetCount; i++)
                {
                    CreateOutpost(data.Outposts[RandomUtils.GetRemoveRandom(intHelper4)], diff);
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyOutpostNamesBucket, enemyOutpostNames);
                }
                break;
            case ESandboxObjectiveType.PatrolUndetected:
                UndetectedPatrolMap(data, ref objData, out helper, out fleetCount, patrolUndetectedDiscover3ID, patrolUndetectedDiscover3DescID,
                    patrolUndetectedDiscover4ID, patrolUndetectedDiscover4DescID, patrolUndetectedDiscover5ID, patrolUndetectedDiscover5DescID,
                    patrolUndetectedStayUndetectedID, patrolUndetectedStayUndetectedDescID);

                SpawnNeutrals(data, helper);
                foreach (var enemy in dummy.EnemyUnits)
                {
                    data.AdditionalInt.Remove(enemy.Taken);
                }
                CreateFleetAllNodes(fleetCount, dummy.Nodes, data.AdditionalInt);
                for (int i = 0; i < fleetCount; i++)
                {
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
                }

                data.Undetected = data.AdditionalInt;
                undetected = true;
                break;
            case ESandboxObjectiveType.UndetectedPatrolBase:
                UndetectedPatrolMap(data, ref objData, out helper, out fleetCount, patrolUndetectedOutpostDiscover3ID, patrolUndetectedOutpostDiscover3DescID,
                    patrolUndetectedOutpostDiscover4ID, patrolUndetectedOutpostDiscover4DescID, patrolUndetectedOutpostDiscover5ID, patrolUndetectedOutpostDiscover5DescID,
                    patrolUndetectedStayUndetectedID, patrolUndetectedStayUndetectedDescID);

                SpawnNeutrals(data, helper);

                intHelper4.Clear();
                intHelper4.AddRange(data.AdditionalInt);
                for (int i = 0; i < fleetCount; i++)
                {
                    CreateOutpost(data.Outposts[RandomUtils.GetRemoveRandom(intHelper4)], diff);
                    GetNameFromBasket(dummy.EnemyUnits[i], enemyOutpostNamesBucket, enemyOutpostNames);
                }

                undetected = true;
                break;
            default:
                Assert.IsTrue(false);
                return null;
        }

        Outit(data);
        //additional fleets
        if (data.AdditionalFleets > 0)
        {
            setHelper.Clear();
            intHelper3.Clear();
            if (undetected)
            {
                foreach (int index in data.Undetected)
                {
                    setHelper.Add(index);
                }
            }
            else
            {
                for (int i = 0; i < dummy.Nodes.Count; i++)
                {
                    setHelper.Add(i);
                }
            }
            foreach (var enemy in dummy.EnemyUnits)
            {
                setHelper.Remove(enemy.Taken);
            }
            int count = dummy.EnemyUnits.Count;
            CreateFleetAllNodes(data.AdditionalFleets, dummy.Nodes, setHelper);
            for (int i = 0; i < data.AdditionalFleets; i++)
            {
                GetNameFromBasket(dummy.EnemyUnits[count + i], enemyFleetNamesBucket, enemyFleetNames);
            }
        }

        int value = -1;
        for (int i = 0; i < moveToBack.Count; i++)
        {
            Assert.IsTrue(moveToBack[i] > value);
            int index = moveToBack[i] - i;
            value = index;
            dummy.EnemyUnits.Add(dummy.EnemyUnits[index]);
            dummy.EnemyUnits.RemoveAt(index);
        }
        moveToBack.Clear();

        return dummy;
    }

    public SOTacticMap SpawnEnemyFleet(MapSpawnData data, ref int ship, ref string name)
    {
        Init(data);

        Assert.IsTrue(data.Difficulty < EMissionDifficulty.VeryHard);
        Assert.IsTrue(data.Type == ESandboxObjectiveType.EnemyFleetInstance);
        Assert.IsTrue(data.AdditionalFleets == 0);

        var objData = MainObjective(out _);

        Destroy1(redWatersDestroyEnemyID, redWatersDestroyEnemyDescID, objData);

        if (ship == -1)
        {
            CreateFleetAllNodes(1, dummy.Nodes, data.AdditionalInt);

            var enemy = dummy.EnemyUnits[0];
            ship = enemy.SaveIndex;
            name = enemy.NameID;
        }
        else
        {
            CreateFleet(fleetBucket.FromIndex(ship).Duplicate(), dummy.Nodes, RandomUtils.GetRandom(data.AdditionalInt), data.AdditionalInt);
            dummy.EnemyUnits[0].NameID = name;
        }

        objData = new ObjectiveData();
        InstantObjective(ref objData, DetectPlayerEffect(), RevealEffect(0));
        dummy.Objectives.Add(objData);

        Outit(data);

        return dummy;
    }

    public void Save(ref SandboxSpawnMapData saveData)
    {
        var data = mapSpawnData;
        saveData.MapSpawnData = data;
        saveData.AdditionalEnemies = data.AdditionalFleets;
        if (saveData.Names == null)
        {
            saveData.Names = new List<string>();
        }
        saveData.Names.Clear();
        if (saveData.Positions == null)
        {
            saveData.Positions = new List<MyVector2>();
        }
        saveData.Positions.Clear();
        if (saveData.Routes == null)
        {
            saveData.Routes = new List<ListInt>();
        }
        saveData.Routes.Clear();
        if (saveData.Blocks == null)
        {
            saveData.Blocks = new List<int>();
        }
        saveData.Blocks.Clear();
        if (saveData.Custom == null)
        {
            saveData.Custom = new List<int>();
        }
        saveData.Custom.Clear();

        if (saveData.CloudsBucket == null)
        {
            saveData.CloudsBucket = new List<int>();
        }
        saveData.CloudsBucket.Clear();
        foreach (var cloud in bucket)
        {
            saveData.CloudsBucket.Add(cloudsPrefabs.IndexOf(cloud));
        }
        saveData.CloudsPrefab = cloudsPrefabs.IndexOf(dummy.CloudsPrefab);
        saveData.CloudsDirection = dummy.CloudDirection;

        if (saveData.FleetBucket == null)
        {
            saveData.FleetBucket = new List<ListInt>();
        }
        fleetBucket.Save(saveData.FleetBucket);
        if (saveData.OneshotFleetBucket == null)
        {
            saveData.OneshotFleetBucket = new List<ListInt>();
        }
        oneshotFleetBucket.Save(saveData.OneshotFleetBucket);
        if (saveData.OutpostBucket == null)
        {
            saveData.OutpostBucket = new List<ListInt>();
        }
        outpostBucket.Save(saveData.OutpostBucket);

        Save(ref saveData.EnemyFleetNamesBucket, enemyFleetNames, enemyFleetNamesBucket);
        Save(ref saveData.EnemyOutpostNamesBucket, enemyOutpostNames, enemyOutpostNamesBucket);
        Save(ref saveData.FriendlyFleetNamesBucket, friendlyFleetNames, friendlyFleetNamesBucket);
        Save(ref saveData.FriendlyOutpostNamesBucket, friendlyOutpostNames, friendlyOutpostNamesBucket);

        foreach (var enemy in dummy.EnemyUnits)
        {
            saveData.Names.Add(enemy.NameID);
            saveData.Positions.Add(enemy.Position);
            saveData.Routes.Add(new ListInt());
            saveData.Blocks.Add(enemy.SaveIndex);
        }
        saveData.ObjectiveFleets = -1;
        switch (data.Type)
        {
            case ESandboxObjectiveType.DestroyBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 0;
                        break;
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 1;
                        break;
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i + 1);
                }
                break;
            case ESandboxObjectiveType.DestroyMultipleBases:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 2;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i + saveData.ObjectiveFleets);
                }
                break;
            case ESandboxObjectiveType.SeekAndDestroy:
                SetRoute(saveData.Routes, 0);
                break;
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 2;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.DestroyTimedFleets:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 2;
                        break;
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 3;
                        break;
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.DestroyChase:
                SetRouteFromPatrols(saveData.Routes, 1);
                break;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                SetRouteFromPatrols(saveData.Routes, 2);
                break;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 0;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }

                foreach (var effect in dummy.Objectives[dummy.Objectives.Count - 2].Effects)
                {
                    saveData.Custom.Add(effect.RetrievePosition);
                    saveData.Custom.Add(effect.BombersNeeded);
                    saveData.Custom.Add(effect.FightersNeeded);
                    saveData.Custom.Add(effect.TorpedoesNeeded);
                }
                break;
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 0;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i + 4);
                }
                break;
            case ESandboxObjectiveType.DefendBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i + 1);
                }
                break;
            case ESandboxObjectiveType.DefendMultipleBases:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 3;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i + 2);
                }
                break;
            case ESandboxObjectiveType.DefendFleet:
                SetRoutes(data, ref saveData, 1);
                break;
            case ESandboxObjectiveType.DefendMultipleFleets:
                SetRoutes(data, ref saveData, 2);
                break;
            case ESandboxObjectiveType.DefendAllyTimed:
            case ESandboxObjectiveType.EscortRetreat:
            case ESandboxObjectiveType.EscortAttack:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 3;
                        break;
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.EscortInvasion:
                for (int i = 0; i < 3; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                SetRoute(saveData.Routes, 0);
                break;
            case ESandboxObjectiveType.DefendBaseUndetected:
                SetRoute(saveData.Routes, 1);
                SetRoute(saveData.Routes, 2);
                break;
            case ESandboxObjectiveType.DefendFleetUndetected:
                for (int i = 0; i < 3; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.Scout:
            case ESandboxObjectiveType.Patrol:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 3;
                        break;
                    case EMissionDifficulty.Hard:
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                break;
            case ESandboxObjectiveType.ScoutBase:
            case ESandboxObjectiveType.PatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 3;
                        break;
                    case EMissionDifficulty.Hard:
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.PatrolUndetected:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 1;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        saveData.ObjectiveFleets = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    SetRoute(saveData.Routes, i);
                }
                saveData.Custom.AddRange(dummy.Objectives[dummy.Objectives.Count - 1].Effects[0].Targets);
                break;
            case ESandboxObjectiveType.UndetectedPatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        saveData.ObjectiveFleets = 1;
                        break;
                    case EMissionDifficulty.Hard:
                    case EMissionDifficulty.Medium:
                        saveData.ObjectiveFleets = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                saveData.Custom.AddRange(dummy.Objectives[dummy.Objectives.Count - 1].Effects[0].Targets);
                break;
            case ESandboxObjectiveType.EnemyFleetInstance:
                SetRoute(saveData.Routes, 0);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        int count = saveData.Routes.Count - 1;
        for (int i = 0; i < data.AdditionalFleets; i++)
        {
            saveData.Routes[count - i].List.AddRange(dummy.EnemyUnits[count - i].RandomNodes);
        }
    }

    public SOTacticMap Load(ref SandboxSpawnMapData saveData)
    {
        var data = saveData.MapSpawnData;
        data.AdditionalFleets = saveData.AdditionalEnemies;
        data.EnemiesCount = GetEnemiesCount(data);

        int dump = 0;
        string dump2 = null;
        var map = data.Type == ESandboxObjectiveType.EnemyFleetInstance ? SpawnEnemyFleet(data, ref dump, ref dump2) : Spawn(data);

        bucket.Clear();
        foreach (int index in saveData.CloudsBucket)
        {
            bucket.Add(cloudsPrefabs[index]);
        }
        dummy.CloudsPrefab = cloudsPrefabs[saveData.CloudsPrefab];
        dummy.CloudDirection = saveData.CloudsDirection;

        fleetBucket.Load(saveData.FleetBucket);
        oneshotFleetBucket.Load(saveData.OneshotFleetBucket);
        outpostBucket.Load(saveData.OutpostBucket);

        Load(saveData.EnemyFleetNamesBucket, enemyFleetNames, enemyFleetNamesBucket);
        Load(saveData.EnemyOutpostNamesBucket, enemyOutpostNames, enemyOutpostNamesBucket);
        Load(saveData.FriendlyFleetNamesBucket, friendlyFleetNames, friendlyFleetNamesBucket);
        Load(saveData.FriendlyOutpostNamesBucket, friendlyOutpostNames, friendlyOutpostNamesBucket);

        for (int i = 0; i < saveData.Positions.Count; i++)
        {
            if (dummy.EnemyUnits.Count <= i)
            {
                break;
            }
            var enemy = dummy.EnemyUnits[i];
            enemy.Position = saveData.Positions[i];
            enemy.NameID = saveData.Names[i];
            if (enemy.RandomNodes != null && enemy.RandomNodes.Count > 0)
            {
                enemy.Taken = dummy.Nodes.IndexOf(enemy.Position);
            }
            else
            {
                enemy.Taken = -1;
            }
        }

        int helper;
        List<EnemyUnitData> possibles;
        switch (data.Type)
        {
            case ESandboxObjectiveType.DestroyBase:
                GetBlocks(saveData.Blocks, 0, outpostBucket);
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + 1);
                    GetBlocks(saveData.Blocks, i + 1, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DestroyMultipleBases:
                int count2 = data.Difficulty == EMissionDifficulty.Easy ? 2 : 3;
                for (int i = 0; i < count2; i++)
                {
                    GetBlocks(saveData.Blocks, i, outpostBucket);
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + count2);
                    GetBlocks(saveData.Blocks, i + count2, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.SeekAndDestroy:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, fleetBucket);
                break;
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
            case ESandboxObjectiveType.DestroyTimedFleets:
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DestroyChase:
                GetRouteToPatrols(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, fleetBucket);
                break;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                GetRouteToPatrols(saveData.Routes, 0);
                GetRouteToPatrols(saveData.Routes, 1);
                GetBlocks(saveData.Blocks, 0, fleetBucket);
                GetBlocks(saveData.Blocks, 1, fleetBucket);
                break;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                var effects = dummy.Objectives[dummy.Objectives.Count - 2].Effects;
                for (int i = 0; i < effects.Count; i++)
                {
                    effects[i].RetrievePosition = saveData.Custom[i];
                    effects[i].BombersNeeded = saveData.Custom[i + 1];
                    effects[i].FightersNeeded = saveData.Custom[i + 2];
                    effects[i].TorpedoesNeeded = saveData.Custom[i + 3];
                }
                break;
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                helper = 0;
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        helper = 3;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        helper = 4;
                        break;
                }
                for (int i = 0; i < helper; i++)
                {
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + helper);
                    GetBlocks(saveData.Blocks, i + helper, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DefendBase:
                GetBlocks(saveData.Blocks, 0, possibleOutposts1);
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + 1);
                    GetBlocks(saveData.Blocks, i + 1, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DefendMultipleBases:
                possibles = data.Difficulty == EMissionDifficulty.Hard ? possibleOutposts2 : possibleOutposts3;
                GetBlocks(saveData.Blocks, 0, possibles);
                GetBlocks(saveData.Blocks, 1, possibles);
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + 2);
                    GetBlocks(saveData.Blocks, i + 2, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DefendFleet:
                possibles = data.Difficulty == EMissionDifficulty.Hard ? possibleFleets1 : possibleFleets3;
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                GetRoute(saveData.Routes, saveData.ObjectiveFleets);
                GetBlocks(saveData.Blocks, saveData.ObjectiveFleets, possibles);
                break;
            case ESandboxObjectiveType.DefendMultipleFleets:
                possibles = data.Difficulty == EMissionDifficulty.Hard ? possibleFleets1 : possibleFleets2;
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                GetRoute(saveData.Routes, saveData.ObjectiveFleets);
                GetBlocks(saveData.Blocks, saveData.ObjectiveFleets, possibles);
                GetRoute(saveData.Routes, saveData.ObjectiveFleets + 1);
                GetBlocks(saveData.Blocks, saveData.ObjectiveFleets + 1, possibles);
                break;
            case ESandboxObjectiveType.DefendAllyTimed:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, data.Difficulty == EMissionDifficulty.Easy ? possibleFleets2 : possibleFleets1);
                for (int i = 1; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.EscortRetreat:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, data.Difficulty == EMissionDifficulty.Hard ? possibleFleets1 : possibleFleets2);
                for (int i = 1; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.EscortAttack:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, data.Difficulty == EMissionDifficulty.Hard ? possibleFleets1 : possibleFleets2);
                for (int i = 1; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.EscortInvasion:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, data.Difficulty == EMissionDifficulty.Easy ? possibleFleets2 : possibleFleets1);
                for (int i = 1; i < 3; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, oneshotFleetBucket);
                break;
            case ESandboxObjectiveType.DefendBaseUndetected:
                GetBlocks(saveData.Blocks, 0, data.Difficulty == EMissionDifficulty.Hard ? possibleOutposts1 : possibleOutposts2);
                GetRoute(saveData.Routes, 1);
                GetRoute(saveData.Routes, 2);
                GetBlocks(saveData.Blocks, 1, fleetBucket);
                GetBlocks(saveData.Blocks, 2, fleetBucket);
                break;
            case ESandboxObjectiveType.DefendFleetUndetected:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, possibleFleets2);
                for (int i = 1; i < 3; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.Scout:
            case ESandboxObjectiveType.Patrol:
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i);
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                break;
            case ESandboxObjectiveType.ScoutBase:
            case ESandboxObjectiveType.PatrolBase:
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetBlocks(saveData.Blocks, i, outpostBucket);
                }
                break;
            case ESandboxObjectiveType.PatrolUndetected:
                helper = 0;
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        helper = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        helper = 3;
                        break;
                }
                for (int i = 0; i < helper; i++)
                {
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetRoute(saveData.Routes, i + helper);
                    GetBlocks(saveData.Blocks, i + helper, fleetBucket);
                }

                SetupEffectTargets(saveData.Custom);
                break;
            case ESandboxObjectiveType.UndetectedPatrolBase:
                helper = 0;
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        helper = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        helper = 3;
                        break;
                }
                for (int i = 0; i < helper; i++)
                {
                    GetBlocks(saveData.Blocks, i, fleetBucket);
                }
                for (int i = 0; i < saveData.ObjectiveFleets; i++)
                {
                    GetBlocks(saveData.Blocks, i + helper, outpostBucket);
                }
                SetupEffectTargets(saveData.Custom);
                break;
            case ESandboxObjectiveType.EnemyFleetInstance:
                GetRoute(saveData.Routes, 0);
                GetBlocks(saveData.Blocks, 0, fleetBucket);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }

        int count = saveData.Routes.Count - 1;
        for (int i = 0; i < data.AdditionalFleets; i++)
        {
            GetRoute(saveData.Routes, count - i);
            GetBlocks(saveData.Blocks, count - i, fleetBucket);
        }

        return map;
    }

    private Vector2 Init(MapSpawnData data)
    {
        mapSpawnData = data;
        dummy.Nodes.Clear();

        Vector2 diff = data.PlayerPos;
        diff.y += (-dummy.PlayerPosition.y / 540f) * TacticalMapCreator.TacticMapHeight * 0.5f;

        foreach (var node in data.Nodes)
        {
            dummy.Nodes.Add((node - diff) * TacticalMapCreator.WorldToTacticScale);
        }

        //clouds
        if (bucket.Count == 0)
        {
            bucket.AddRange(cloudsPrefabs);
        }
        dummy.CloudsPrefab = RandomUtils.GetRemoveRandom(bucket);
        dummy.CloudDirection = UnityRandom.insideUnitCircle;

        dummy.AdditionalObjectsToSpawnMin = (int)GetEnemiesCount(data) + 1;

        dummy.Objectives.Clear();
        dummy.EnemyUnits.Clear();

        return diff;
    }

    private ObjectiveData MainObjective(out ObjectiveEffectData winEffect)
    {
        var result = new ObjectiveData();

        result.Active = true;
        result.Visible = true;

        winEffect = new ObjectiveEffectData() { EffectType = EObjectiveEffect.Win };
        result.Effects = new List<ObjectiveEffectData>() { winEffect };

        return result;
    }

    private void Outit(MapSpawnData data)
    {
        for (int i = 0; i < dummy.Objectives.Count; i++)
        {
            Assert.IsTrue(dummy.Objectives.LastIndexOf(dummy.Objectives[i]) == i);
            Assert.IsNotNull(dummy.Objectives[i].Effects);
        }
        foreach (var enemy in dummy.EnemyUnits)
        {
            fleetBucket.Check(enemy);
            oneshotFleetBucket.Check(enemy);
            outpostBucket.Check(enemy);
        }
        dummy.Difficulty = data.Difficulty;
        dummy.EnemiesCount = data.EnemiesCount;

        dummy.AdditionalObjectsToSpawnMax = dummy.AdditionalObjectsToSpawnMin;
    }

    private int FlagCount(List<int> flags, int flag)
    {
        int result = 0;
        foreach (int value in flags)
        {
            if ((value & flag) != 0)
            {
                result++;
            }
        }
        return result;
    }

    private MapSpawnData CheckSeekAndDestroy(List<float> sqrDistances, Vector2 mapPos, int fleetCount, MapSpawnData data)
    {
        intHelper.Clear();
        for (int i = 0; i < sqrDistances.Count; i++)
        {
            intHelper.Add(0);
        }
        intHelper.Add(1 << 0);
        intHelper.Add(1 << 1);
        intHelper.Add(1 << 2);

        int count = sqrDistances.Count;
        if (count == 2)
        {
            intHelper[count] |= intHelper[count + 1];
            intHelper[count + 1] = intHelper[count + 2];
        }
        else
        {
            Assert.IsTrue(count == 3);
        }

        data.FlagList = new List<int>();
        int found = 0;
        foreach (var node in data.Nodes)
        {
            float dist = Vector2.SqrMagnitude(node - mapPos);
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                if (dist < sqrDistances[i])
                {
                    value = intHelper[count + i];
                    if (++intHelper[i] == fleetCount)
                    {
                        found++;
                    }
                    break;
                }
            }
            data.FlagList.Add(value);
        }
        Assert.IsFalse(found > count);
        return found == count ? data : null;
    }

    private IEnumerable<int> GetNodesWithin(IEnumerable<Vector2> nodes, Vector2 pos, float sqrDist)
    {
        int i = 0;
        foreach (var node in nodes)
        {
            if (Vector2.SqrMagnitude(node - pos) < sqrDist)
            {
                yield return i;
            }
            i++;
        }
    }

    private IEnumerable<int> GetNodesWithout(IEnumerable<Vector2> nodes, Vector2 pos, float sqrDist)
    {
        int i = 0;
        foreach (var node in nodes)
        {
            if (Vector2.SqrMagnitude(node - pos) > sqrDist)
            {
                yield return i;
            }
            i++;
        }
    }

    private void GetChaseNodes(MapSpawnData data, IEnumerable<Vector2> edgeNodes, Vector2 mapPos, float maxSpawnDistance, float minEdgeDistance)
    {
        intHelper.Clear();
        intHelper.AddRange(GetNodesWithin(ToVector2(data.Nodes), mapPos, maxSpawnDistance * maxSpawnDistance * tacticToWorldSqr));
        data.Edges = new List<MyVector2>(ToMyVector2(edgeNodes));
        float minEdgeSqrDistance = minEdgeDistance * minEdgeDistance;
        minEdgeSqrDistance *= tacticToWorldSqr;

        data.FlagList = new List<int>();
        for (int i = 0; i < data.Edges.Count; i++)
        {
            data.FlagList.Add(0);
        }

        data.AdditionalInt = new List<int>();
        foreach (int index in intHelper)
        {
            bool added = false;
            int flag = 1 << data.AdditionalInt.Count;
            foreach (int index2 in GetNodesWithout(ToVector2(data.Edges), data.Nodes[index], minEdgeSqrDistance))
            {
                added = true;
                data.FlagList[index2] |= flag;
            }
            if (added)
            {
                data.AdditionalInt.Add(index);
            }
        }
    }

    private bool CheckDistanceWithin(List<MyVector2> nodes, int first, int second, float min, float max)
    {
        float dist = Vector2.SqrMagnitude((Vector2)nodes[first] - nodes[second]);
        return dist >= min && dist < max;
    }

    private EEnemiesCount GetEnemiesCount(MapSpawnData data)
    {
        int count = 0;
        switch (data.Type)
        {
            case ESandboxObjectiveType.DestroyBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 1;
                        break;
                    case EMissionDifficulty.Medium:
                        count = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DestroyMultipleBases:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 3;
                        break;
                    case EMissionDifficulty.Medium:
                        count = 5;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 6;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.SeekAndDestroy:
                count = 1;
                break;
            case ESandboxObjectiveType.SeekAndDestroyMultiple:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 2;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DestroyTimedFleets:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 2;
                        break;
                    case EMissionDifficulty.Medium:
                        count = 3;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DestroyChase:
                count = 1;
                break;
            case ESandboxObjectiveType.DestroyChaseMultiple:
                count = 2;
                break;
            case ESandboxObjectiveType.RetrieveTimedPlanes:
            case ESandboxObjectiveType.RetrieveTimedSurvivors:
            case ESandboxObjectiveType.RetrieveSurvivorsUnknown:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 0;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DefendBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        count = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DefendMultipleBases:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 3;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DefendFleet:
            case ESandboxObjectiveType.DefendMultipleFleets:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 1;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.DefendAllyTimed:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        count = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.EscortRetreat:
            case ESandboxObjectiveType.EscortAttack:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                    case EMissionDifficulty.Medium:
                        count = 2;
                        break;
                    case EMissionDifficulty.Hard:
                        count = 3;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.EscortInvasion:
                count = 2;
                break;
            case ESandboxObjectiveType.DestroyFleetUndetected:
                count = 1;
                break;
            case ESandboxObjectiveType.DefendBaseUndetected:
            case ESandboxObjectiveType.DefendFleetUndetected:
                count = 2;
                break;
            case ESandboxObjectiveType.Scout:
            case ESandboxObjectiveType.ScoutBase:
            case ESandboxObjectiveType.Patrol:
            case ESandboxObjectiveType.PatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 3;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 4;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.PatrolUndetected:
            case ESandboxObjectiveType.UndetectedPatrolBase:
                switch (data.Difficulty)
                {
                    case EMissionDifficulty.Easy:
                        count = 1;
                        break;
                    case EMissionDifficulty.Medium:
                    case EMissionDifficulty.Hard:
                        count = 2;
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
                break;
            case ESandboxObjectiveType.EnemyFleetInstance:
                return EEnemiesCount.Weak;
            default:
                Assert.IsTrue(false);
                break;
        }
        count += data.AdditionalFleets;
        if (count < 2)
        {
            return EEnemiesCount.Weak;
        }
        if (count < 4)
        {
            return EEnemiesCount.MediumLow;
        }
        if (count < 6)
        {
            return EEnemiesCount.MediumHigh;
        }
        return EEnemiesCount.Strong;
    }

    private MapSpawnData CanDefendFleet(MapSpawnData result, Vector2 mapPos, int nodeCount, float spawnMaxDistance, float nodeMinDistance)
    {
        if (result.Nodes.Count < nodeCount)
        {
            return null;
        }

        result.FlagList = new List<int>();
        for (int i = 0; i < result.Nodes.Count; i++)
        {
            result.FlagList.Add(0);
        }

        result.AdditionalInt = new List<int>();
        int flag = 1;
        foreach (int index in GetNodesWithin(ToVector2(result.Nodes), mapPos, spawnMaxDistance))
        {
            bool ok = false;
            foreach (int index2 in GetNodesWithout(ToVector2(result.Nodes), result.Nodes[index], nodeMinDistance))
            {
                ok = true;
                result.FlagList[index2] |= flag;
            }
            if (ok)
            {
                Assert.IsTrue(result.AdditionalInt.Count < 32);
                result.AdditionalInt.Add(index);
                flag = 1 << result.AdditionalInt.Count;
            }
        }
        return result.AdditionalInt.Count > 0 ? result : null;
    }

    private MapSpawnData CheckAddStartNodes(MapSpawnData result, List<int> startNodes, List<List<float>> nodeDistances, List<float> sqrDistances, List<MyVector2> nodes, int offset, int minFleetNodes)
    {
        result.AdditionalInt = new List<int>();
        for (int i = 0; i < sqrDistances.Count; i++)
        {
            int index = -1;
            int index2 = -1;
            float dist = float.PositiveInfinity;
            for (int j = 0; j < startNodes.Count; j++)
            {
                int k = j * 3 + offset;
                for (int l = offset; l < nodeDistances[k].Count; l++)
                {
                    float newDist = Mathf.Abs(nodeDistances[k][l] * nodeDistances[k][l] - sqrDistances[i]);
                    if (newDist < dist)
                    {
                        dist = newDist;
                        index = startNodes[j];
                        index2 = l;
                    }
                }
            }
            if (index == -1)
            {
                return null;
            }

            result.AdditionalInt.Add(index);
            if (nodes == null)
            {
                result.AdditionalInt.Add(index2);
            }
            else
            {
                result.AdditionalInt.Add(result.Nodes.Count);
                result.Nodes.Add(nodes[index2]);
            }
        }
        float airstrikeSqrRange = AirstrikeSqrRange() * tacticToWorldSqr;
        result.FlagList = new List<int>();
        for (int i = 0; i < result.Nodes.Count; i++)
        {
            result.FlagList.Add(0);
        }
        for (int i = 0; i < sqrDistances.Count; i++)
        {
            int count = 0;
            int index = result.AdditionalInt[i * 2];
            int flag = 1 << i;
            foreach (int index2 in GetNodesWithin(ToVector2(result.Nodes), result.Nodes[index], airstrikeSqrRange))
            {
                if (index2 == index)
                {
                    continue;
                }
                result.FlagList[index2] |= flag;
                count++;
            }
            if (count < minFleetNodes)
            {
                return null;
            }
        }

        return result;
    }

    private MapSpawnData CanScout(MapSpawnData result, int nodeCount, int farNodeCount, Vector2 mapPos, float minDist)
    {
        if (result.Nodes.Count < nodeCount)
        {
            return null;
        }
        result.AdditionalInt = new List<int>(GetNodesWithout(ToVector2(result.Nodes), mapPos, minDist));
        return result.AdditionalInt.Count > farNodeCount ? result : null;
    }

    private MapSpawnData CanScoutOutpost(MapSpawnData result, IEnumerable<Vector2> outpostNodes, Vector2 mapPos, float minDist)
    {
        if (result.Nodes.Count < 5)
        {
            return null;
        }
        result.Outposts = new List<MyVector2>(ToMyVector2(outpostNodes));
        result.AdditionalInt = new List<int>(GetNodesWithout(ToVector2(result.Outposts), mapPos, minDist));
        return result.AdditionalInt.Count > 3 ? result : null;
    }

    private int DestroyTimed(List<string> objText2, List<string> objText2Desc, List<string> objText3, List<string> objText3Desc, List<string> timerIDs, ObjectiveData objData, MapSpawnData data, Vector2 diff)
    {
        int count = 0;
        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
            case EMissionDifficulty.Hard:
                count = 2;
                TargetiveObjective(EObjectiveType.Destroy, objText2, objText2Desc, objData, 0, 1);
                objData = new ObjectiveData();
                InstantObjective(ref objData, RevealEffect(0, 1));
                break;
            case EMissionDifficulty.Medium:
                count = 3;
                TargetiveObjective(EObjectiveType.Destroy, objText3, objText3Desc, objData, 0, 1, 2);
                objData = new ObjectiveData();
                InstantObjective(ref objData, RevealEffect(0, 1, 2));
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        int ticks = destroyTimeTicks[(int)data.Difficulty];
        objData.Effects.Add(TimerEffect(true, ticks, timerIDs[0], timerIDs[1], timerIDs[2]));
        dummy.Objectives.Add(objData);
        objData = new ObjectiveData();
        TimeFailObjective(ref objData, ticks, EMissionLoseCause.SandboxTimeout);

        return count;
    }

    private void DestroyChase(float speed, IEnumerable<int> fleets, MapSpawnData data, Vector2 diff)
    {
        int count = dummy.Nodes.Count;

        foreach (var node in data.Edges)
        {
            dummy.Nodes.Add((node - diff) * TacticalMapCreator.WorldToTacticScale);
        }
        intHelper2.Clear();
        setHelper.Clear();
        foreach (int fleetIndex in fleets)
        {
            var enemy = fleetBucket.Get().Duplicate();
            FillParams(enemy);
            GetNameFromBasket(enemy, enemyFleetNamesBucket, enemyFleetNames);

            enemy.Speed = speed;
            enemy.CanChase = false;
            enemy.OverrideChanceToReveal = 0f;
            enemy.Taken = data.AdditionalInt[fleetIndex];
            enemy.Position = dummy.Nodes[enemy.Taken];

            intHelper.Clear();
            int flag = 1 << fleetIndex;
            for (int i = 0; i < data.FlagList.Count; i++)
            {
                int value = count + i;
                if (!setHelper.Contains(value) && (data.FlagList[i] & flag) != 0)
                {
                    intHelper.Add(value);
                }
            }

            int edge = RandomUtils.GetRandom(intHelper);
            setHelper.Add(edge);
            enemy.Patrols = new List<PatrolData>() { new PatrolData() { Poses = new List<int>() { edge } } };
            enemy.MaxOffset = 5f;

            intHelper2.Add(dummy.EnemyUnits.Count);
            dummy.EnemyUnits.Add(enemy);
        }

        var objData = new ObjectiveData();
        InstantObjective(ref objData, RevealEffect(intHelper2));
        dummy.Objectives.Add(objData);

        foreach (int index in intHelper2)
        {
            objData = new ObjectiveData();
            ReachObjective(ref objData, index);
            objData.LoseType = EMissionLoseCause.SandboxEnemyFled;
            objData.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
            objData.Active = true;
            dummy.Objectives.Add(objData);
        }
    }

    private void DefendFleetObjectives(MapSpawnData mapData, ref ObjectiveData objData, List<EnemyUnitData> list1, List<EnemyUnitData> list2, List<string> title7,
        List<string> desc7, string title3, string desc3, bool multipleAllies, out int fleetCount, out List<EnemyUnitData> possibleUnits)
    {
        switch (mapData.Difficulty)
        {
            case EMissionDifficulty.Easy:
                fleetCount = 1;
                possibleUnits = list1;
                break;
            case EMissionDifficulty.Medium:
                fleetCount = 2;
                possibleUnits = list1;

                break;
            case EMissionDifficulty.Hard:
                fleetCount = 2;
                possibleUnits = list2;

                break;
            default:
                fleetCount = 0;
                possibleUnits = null;
                Assert.IsTrue(false);
                break;
        }
        int count = fleetCount + mapData.AdditionalFleets;
        if (count == 1)
        {
            Destroy1(title7[0], desc7[0], objData);
            objData.Params = new string[] { "0", "1" };
        }
        else
        {
            intHelper4.Clear();
            for (int i = 0; i < count; i++)
            {
                intHelper4.Add(i);
            }
            TargetiveObjective(EObjectiveType.Destroy, title7, desc7, objData, true, intHelper4);
        }

        objData = new ObjectiveData();
        objData.Title = title3;
        objData.Description = desc3;
        objData.Effects = new List<ObjectiveEffectData>();

        TargetiveObjective(ref objData, EObjectiveType.Destroy, EObjectiveTarget.Number, 1, count);
        if (multipleAllies)
        {
            objData.Targets.Add(count + 1);
            objData.ObjectiveTargetIDs.Add(count + 1);
        }
        objData.NotType = true;
        objData.Visible = true;
        objData.Active = true;
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        FailObjectiveTarget(ref objData, EMissionLoseCause.SandboxAllyDestroyed, dummy.Objectives.Count - 1);
        objData.NotType = false;
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        InstantObjective(ref objData, DetectPlayerEffect());
        dummy.Objectives.Add(objData);
    }

    private void AddOptionalFleetsToObjectives(MapSpawnData data, int startIndex, int enemyCount, int fleetCount)
    {
        intHelper.Clear();
        for (int i = 0; i < data.AdditionalFleets; i++)
        {
            intHelper.Add(enemyCount + i);
        }
        dummy.Objectives[startIndex].TargetType = EObjectiveTarget.Number;
        dummy.Objectives[startIndex].Count = fleetCount;
        dummy.Objectives[startIndex].Targets.AddRange(intHelper);
        dummy.Objectives[startIndex].ObjectiveTargetIDs.AddRange(intHelper);

        for (int i = 1; i <= fleetCount; i++)
        {
            dummy.Objectives[startIndex + i].Targets.AddRange(intHelper);
            dummy.Objectives[startIndex + i].ObjectiveTargetIDs.AddRange(intHelper);
        }
    }

    private void SpawnDefendAlly(MapSpawnData data, string title, string desc, List<EnemyUnitData> list1, List<EnemyUnitData> list2, List<EnemyUnitData> list3,
        int fleetCount3, params ObjectiveEffectData[] effects)
    {
        var objData = new ObjectiveData();
        Destroy1(title, desc, objData);
        objData.Effects = new List<ObjectiveEffectData>();
        objData.Visible = true;
        objData.Active = true;
        objData.NotType = true;

        objData = new ObjectiveData();
        FailObjectiveTarget(ref objData, EMissionLoseCause.SandboxAllyDestroyed, dummy.Objectives.Count - 1);
        objData.NotType = false;
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        InstantObjective(ref objData, effects);
        objData.Effects.Add(ShowAllyEffect(0));
        dummy.Objectives.Add(objData);

        int fleetCount;
        List<EnemyUnitData> possibles;
        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
                fleetCount = 2;
                possibles = list1;
                break;
            case EMissionDifficulty.Medium:
                fleetCount = 2;
                possibles = list2;
                break;
            case EMissionDifficulty.Hard:
                fleetCount = fleetCount3;
                possibles = list3;
                break;
            default:
                fleetCount = 0;
                possibles = null;
                Assert.IsTrue(false);
                break;
        }

        CreateFleet(RandomUtils.GetRandom(possibles).Duplicate(), dummy.Nodes, intHelper4[0], intHelper4[1]);

        var fleet = dummy.EnemyUnits[0];
        fleet.IsAlly = true;
        fleet.MaxOffset = 5f;
        GetNameFromBasket(fleet, friendlyFleetNamesBucket, friendlyFleetNames);

        Assert.IsFalse(setHelper.Count < fleetCount, fleetCount.ToString());
        CreateFleet(fleetCount, dummy.Nodes, setHelper, intHelper3);
        for (int i = 0; i < fleetCount; i++)
        {
            GetNameFromBasket(dummy.EnemyUnits[i + 1], enemyFleetNamesBucket, enemyFleetNames);
        }
        dummy.AllyAttacks = true;
    }

    private void SpawnEscort(MapSpawnData data, ref ObjectiveData objData, string title1, string desc1, string title2, string desc2, List<EnemyUnitData> list1,
        List<EnemyUnitData> list2, List<EnemyUnitData> list3, int fleetCount3)
    {
        int index = (int)data.Difficulty * 2;
        intHelper4.Clear();
        intHelper4.Add(data.AdditionalInt[index]);
        intHelper4.Add(data.AdditionalInt[index + 1]);

        setHelper.Clear();
        foreach (int index2 in FlagList(data.FlagList, (int)data.Difficulty))
        {
            if (!intHelper4.Contains(index2))
            {
                setHelper.Add(index2);
            }
        }
        Assert.IsFalse(setHelper.Count == 0);
        intHelper3.Clear();
        for (int i = 0; i < dummy.Nodes.Count; i++)
        {
            if (!intHelper4.Contains(i))
            {
                intHelper3.Add(i);
            }
        }

        ReachObjective(ref objData, 0, intHelper4[1]);
        objData.Title = title2;
        objData.Description = desc2;
        objData.Active = true;

        SpawnDefendAlly(data, title1, desc1, list1, list2, list3, fleetCount3, DetectPlayerEffect());

        objData.ObjectiveTargetIDs.Add(-1);
        objData.ObjectiveTargetVectors.Add(dummy.Nodes[intHelper4[1]]);
        dummy.Objectives.Add(objData);
    }

    private void SpawnDefendUndetected(MapSpawnData data, ref ObjectiveData objData, List<int> ticks, List<string> timeObj, List<string> timeObjDesc,
        string hiddenObj, string hiddenObjDesc, string protectObj, string protectObjDesc, string timer, string timerTooltip, string timerTooltipDesc, EMissionLoseCause destroyedCause)
    {
        int difficulty = (int)data.Difficulty;
        int ticksCount = ticks[difficulty];

        TimeObjective(ref objData, ticksCount, timeObj[difficulty], timeObjDesc[difficulty]);
        objData.Params = new string[] { (ticksCount / 180).ToString() };
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        StayHiddenObjective(ref objData, hiddenObj, hiddenObjDesc);
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        Destroy1(protectObj, protectObjDesc, objData);
        objData.Effects = new List<ObjectiveEffectData>();
        objData.NotType = true;
        objData.Active = true;
        objData.Visible = true;

        objData = new ObjectiveData();
        FailObjectiveTarget(ref objData, EMissionLoseCause.SandboxDetected, 1);
        dummy.Objectives.Add(objData);
        objData = new ObjectiveData();
        FailObjectiveTarget(ref objData, destroyedCause, 2);
        objData.NotType = false;
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        InstantObjective(ref objData, TimerEffect(true, ticks[difficulty], timer, timerTooltip, timerTooltipDesc), ShowAllyEffect(0));
        dummy.Objectives.Add(objData);
    }

    private void GetFillDefendFleetNodes(MapSpawnData data)
    {
        intHelper4.Clear();
        intHelper4.Add(UnityRandom.Range(0, data.AdditionalInt.Count));

        intHelper.Clear();
        intHelper.AddRange(FlagList(data.FlagList, intHelper4[0]));

        intHelper4[0] = data.AdditionalInt[intHelper4[0]];
        intHelper4.Add(RandomUtils.GetRandom(intHelper));

        setHelper.Clear();
        intHelper3.Clear();
        for (int i = 0; i < dummy.Nodes.Count; i++)
        {
            if (!intHelper4.Contains(i))
            {
                setHelper.Add(i);
                intHelper3.Add(i);
            }
        }
    }

    private int ScoutMap(MapSpawnData data, ref ObjectiveData objData, List<string> discover3, List<string> discover3Desc, List<string> identify3, List<string> identify3Desc,
        List<string> discover4, List<string> discover4Desc, List<string> identify4, List<string> identify4Desc, ObjectiveEffectData winEffect)
    {
        int result;
        Assert.IsTrue(objData.Effects.Count == 1);
        objData.Effects.Clear();

        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
                result = 3;
                TargetiveObjective(EObjectiveType.Reveal, discover3, discover3Desc, objData, 0, 1, 2);

                objData = new ObjectiveData();
                objData.Visible = true;
                TargetiveObjective(EObjectiveType.RevealBlocks, identify3, identify3Desc, objData, false, 0, 1, 2);
                break;
            case EMissionDifficulty.Medium:
            case EMissionDifficulty.Hard:
                result = 4;
                TargetiveObjective(EObjectiveType.Reveal, discover4, discover4Desc, objData, 0, 1, 2, 3);

                objData = new ObjectiveData();
                objData.Visible = true;
                TargetiveObjective(EObjectiveType.RevealBlocks, identify4, identify4Desc, objData, false, 0, 1, 2, 3);
                break;
            default:
                Assert.IsTrue(false);
                result = 0;
                break;
        }
        objData.Effects.Add(winEffect);
        Assert.IsTrue(dummy.Objectives[result + 1] == objData);
        for (int i = 0; i < result; i++)
        {
            dummy.Objectives[i].ObjectiveTargetIDs.Clear();
        }
        AddOptionalFleetsToObjectives(data, 0, result, result);
        AddOptionalFleetsToObjectives(data, result + 1, result, result);
        return result;
    }

    private void UndetectedPatrolMap(MapSpawnData data, ref ObjectiveData objData, out int uos, out int fleetCount, List<string> discover3, List<string> discover3Desc,
        List<string> discover4, List<string> discover4Desc, List<string> discover5, List<string> discover5Desc, string undetected, string undetectedDesc)
    {
        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
                Objective(EObjectiveType.RevealUO, 3, discover3, discover3Desc, objData);
                fleetCount = 1;
                uos = 2;
                break;
            case EMissionDifficulty.Medium:
                Objective(EObjectiveType.RevealUO, 4, discover4, discover4Desc, objData);
                fleetCount = 2;
                uos = 2;
                break;
            case EMissionDifficulty.Hard:
                Objective(EObjectiveType.RevealUO, 5, discover5, discover5Desc, objData);
                fleetCount = 2;
                uos = 3;
                break;
            default:
                fleetCount = 0;
                uos = 0;
                Assert.IsTrue(false);
                break;
        }
        int count = fleetCount + uos;
        objData = dummy.Objectives[0];
        objData.Targets = new List<int>();
        for (int i = 0; i < count; i++)
        {
            objData.Targets.Add(0);
        }
        objData.UOObjectives = true;

        StayUndetected(undetected, undetectedDesc);
    }

    private void SpawnNeutrals(MapSpawnData data, int count)
    {
        var objData = new ObjectiveData();

        intHelper4.Clear();
        intHelper4.AddRange(data.TripletList);

        intHelper.Clear();
        for (int i = 0; i < count; i++)
        {
            intHelper.Add(RandomUtils.GetRemoveRandom(intHelper4));
        }
        CreateFleet(count, dummy.Nodes, data.TripletList);
        intHelper.Clear();
        for (int i = 0; i < count; i++)
        {
            int index = dummy.EnemyUnits.Count - i - 1;
            intHelper.Add(index);
            dummy.EnemyUnits[index].IsDisabled = true;
        }

        InstantObjective(ref objData, SpawnNeutralEffect(intHelper));
        dummy.Objectives.Add(objData);
    }

    private void StayUndetected(string undetected, string undetectedDesc)
    {
        var objData = new ObjectiveData();
        StayHiddenObjective(ref objData, undetected, undetectedDesc);
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        FailObjectiveTarget(ref objData, EMissionLoseCause.SandboxDetected, dummy.Objectives.Count - 1);
        dummy.Objectives.Add(objData);
    }

    private void RemoveFromTriplet(List<int> triplet, List<int> notInList)
    {
        int index = UnityRandom.Range(0, triplet.Count);
        if (notInList.Contains(triplet[index]))
        {
            int value = UnityRandom.value > 0.5f ? 2 : 0;
            index = index == value ? 1 : value;
        }
        triplet.RemoveAt(index);
    }

    private IEnumerable<int> FlagList(List<int> flags, int flag)
    {
        flag = 1 << flag;
        for (int i = 0; i < flags.Count; i++)
        {
            if ((flags[i] & flag) != 0)
            {
                yield return i;
            }
        }
    }

    private IEnumerable<int> LongFlagList(List<long> flags, int value)
    {
        long flag = 1L << value;
        for (int i = 0; i < flags.Count; i++)
        {
            if ((flags[i] & flag) != 0L)
            {
                yield return i;
            }
        }
    }

    private IEnumerable<int> FlagListMultipleOutposts(List<int> flags, List<int> outposts)
    {
        int value = 0;
        for (int i = 0; i < outposts.Count; i++)
        {
            value = 1 << outposts[i];
        }

        for (int i = 0; i < flags.Count; i++)
        {
            if ((flags[i] & value) != 0)
            {
                yield return i;
            }
        }
    }

    private IEnumerable<int> FlagListMultipleOutposts(List<long> flags, List<int> outposts)
    {
        long value = 0;
        for (int i = 0; i < outposts.Count; i++)
        {
            value = 1L << outposts[i];
        }

        for (int i = 0; i < flags.Count; i++)
        {
            if ((flags[i] & value) != 0L)
            {
                yield return i;
            }
        }
    }

    private IEnumerable<int> GetFlagTripletIndices(int triplet)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((triplet & (1 << i)) != 0)
            {
                yield return i;
            }
        }
    }

    private IEnumerable<int> GetTripletIndices(int triplet, int offset)
    {
        int left = offset;
        while (left <= 32)
        {
            yield return BinUtils.GetBits(triplet, left - 1, left - offset);
            left += offset;
        }
    }

    private IEnumerable<int> GetRetrievePlanesFleetNodes(List<Vector2> nodes, HashSet<int> retrievePlanesCenters)
    {
        float dist = retrievePlanesMaxPatrolDistance * retrievePlanesMaxPatrolDistance;
        for (int i = 0; i < dummy.Nodes.Count; i++)
        {
            foreach (int index in retrievePlanesCenters)
            {
                foreach (int index2 in GetNodesWithout(nodes, nodes[index], dist))
                {
                    yield return index2;
                }
            }
        }
    }

    private int CreateOutpost(List<MyVector2> outpostNodes, Vector2 diff)
    {
        int result = UnityRandom.Range(0, outpostNodes.Count);

        CreateOutpost(outpostNodes[result], diff);

        return result;
    }

    private int CreateOutpost(EnemyUnitData outpost, List<MyVector2> outpostNodes, Vector2 diff)
    {
        int result = UnityRandom.Range(0, outpostNodes.Count);

        CreateOutpost(outpost, outpostNodes[result], diff);

        return result;
    }

    private void CreateOutpost(Vector2 pos, Vector2 diff)
    {
        CreateOutpost(outpostBucket.Get().Duplicate(), pos, diff);
    }

    private void CreateOutpost(EnemyUnitData outpost, Vector2 pos, Vector2 diff)
    {
        FillParams(outpost);
        outpost.Position = (pos - diff) * TacticalMapCreator.WorldToTacticScale;
        dummy.EnemyUnits.Add(outpost);
    }

    private void CreateFleet(int count, List<Vector2> nodes, IEnumerable<int> possibleNodes)
    {
        intHelper3.Clear();
        intHelper3.AddRange(possibleNodes);
        CreateFleet(count, nodes, intHelper3, intHelper3);
    }

    private void CreateFleetAllNodes(int count, List<Vector2> nodes, IEnumerable<int> possibleNodes)
    {
        intHelper3.Clear();
        for (int i = 0; i < nodes.Count; i++)
        {
            intHelper3.Add(i);
        }
        CreateFleet(count, nodes, possibleNodes, intHelper3);
    }

    private void CreateFleet(int count, List<Vector2> nodes, IEnumerable<int> possibleNodes, IEnumerable<int> possibleRoutes)
    {
        if (count < 1)
        {
            return;
        }
        intHelper.Clear();
        intHelper.AddRange(possibleNodes);
        intHelper2.Clear();
        intHelper2.AddRange(possibleRoutes);

        for (int i = 0; i < count; i++)
        {
            var enemy = fleetBucket.Get().Duplicate();
            FillParams(enemy);

            enemy.Taken = RandomUtils.GetRemoveRandom(intHelper);
            enemy.Position = nodes[enemy.Taken];
            enemy.RandomNodes = new List<int>(intHelper2);

            dummy.EnemyUnits.Add(enemy);
        }
    }

    private void CreateFleet(EnemyUnitData enemy, List<Vector2> nodes, int node, IEnumerable<int> possibleRoutes)
    {
        FillParams(enemy);

        enemy.Taken = node;
        enemy.Position = nodes[enemy.Taken];
        enemy.RandomNodes = new List<int>(possibleRoutes);

        dummy.EnemyUnits.Add(enemy);
    }

    private void CreateFleet(EnemyUnitData enemy, List<Vector2> nodes, int node, params int[] possibleRoutes)
    {
        CreateFleet(enemy, nodes, node, (IEnumerable<int>)possibleRoutes);
    }

    private void FillParams(EnemyUnitData data)
    {
        data.AttackRange = attackRange;
        data.DetectRange = detectRange;
        data.RevealRange = revealRange;
        data.MaxOffset = maxOffset;
        data.Speed = speed;
    }

    private void Objective(ref ObjectiveData data, EObjectiveType type, int count)
    {
        data.Type = type;
        data.Count = count;
    }

    private void Objective(ref ObjectiveData data, EObjectiveType type, EObjectiveTarget target, int count)
    {
        Objective(ref data, type, count);
        data.TargetType = target;
    }

    private void TargetiveObjective(ref ObjectiveData data, EObjectiveType type, EObjectiveTarget target, int count, params int[] targets)
    {
        TargetiveObjective(ref data, type, target, count, (ICollection<int>)targets);
    }

    private void TargetiveObjective(ref ObjectiveData data, EObjectiveType type, EObjectiveTarget target, int count, ICollection<int> targets)
    {
        Objective(ref data, type, target, count);
        data.Targets = new List<int>(targets);
        data.ObjectiveTargetIDs.AddRange(targets);
    }

    private void RescueSurvivorsObjective(ref ObjectiveData data, int count)
    {
        Objective(ref data, EObjectiveType.RescueSurvivors, EObjectiveTarget.Number, count);
    }

    private void TimeObjective(ref ObjectiveData data, int count, string title, string desc)
    {
        Objective(ref data, EObjectiveType.Time, count);

        data.Title = title;
        data.Description = desc;
    }

    private void InstantObjective(ref ObjectiveData data, params ObjectiveEffectData[] effects)
    {
        data.Type = EObjectiveType.Instant;
        data.Active = true;
        data.Effects = new List<ObjectiveEffectData>(effects);
    }

    private void ReachObjective(ref ObjectiveData data, int enemyID)
    {
        var nodes = dummy.EnemyUnits[enemyID].Patrols[0].Poses;
        ReachObjective(ref data, enemyID, nodes[nodes.Count - 1]);
    }

    private void ReachObjective(ref ObjectiveData data, int enemyID, int node)
    {
        TargetiveObjective(ref data, EObjectiveType.Reach, EObjectiveTarget.All, 1, node);
        data.ObjectiveTargetIDs.Clear();
        data.SecondaryTarget = enemyID;
    }

    private void DummyObjective(ref ObjectiveData data, string title, string desc)
    {
        data.Title = title;
        data.Description = desc;
        data.Effects = new List<ObjectiveEffectData>();
        data.Visible = true;
    }

    private void StayHiddenObjective(ref ObjectiveData data, string title, string desc)
    {
        DummyObjective(ref data, title, desc);
        data.Type = EObjectiveType.StayHidden;
        data.Active = true;
    }

    private void FailObjectiveTarget(ref ObjectiveData data, EMissionLoseCause loseCause, params int[] targets)
    {
        TargetiveObjective(ref data, EObjectiveType.CompleteObjective, EObjectiveTarget.Number, 1, targets);
        data.ObjectiveTargetIDs.Clear();
        data.NotType = true;
        data.LoseType = loseCause;
        data.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
        data.Active = true;
    }

    private void TimeFailObjective(ref ObjectiveData data, int time, EMissionLoseCause loseCause)
    {
        Objective(ref data, EObjectiveType.Time, time);
        data.Active = true;
        data.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
        data.LoseType = loseCause;
        dummy.Objectives.Add(data);
    }

    private ObjectiveEffectData Effect(EObjectiveEffect effect, bool active = true)
    {
        var result = new ObjectiveEffectData();
        result.EffectType = effect;
        result.NotEffect = !active;
        return result;
    }

    private ObjectiveEffectData Effect(EObjectiveEffect effect, params int[] targets)
    {
        return Effect(effect, (IEnumerable<int>)targets);
    }

    private ObjectiveEffectData Effect(EObjectiveEffect effect, IEnumerable<int> targets)
    {
        var result = Effect(effect);
        result.Targets = new List<int>(targets);
        return result;
    }

    private ObjectiveEffectData Effect(EObjectiveEffect effect, bool active, params int[] targets)
    {
        var result = Effect(effect, (IEnumerable<int>)targets);
        result.NotEffect = !active;
        return result;
    }

    private ObjectiveEffectData SetObjectiveTextEffect(int id, string text, string desc, int paramA, int paramB)
    {
        var result = Effect(EObjectiveEffect.SetObjectiveText, id);
        result.TimerTooltipTitleID = text;
        result.TimerTooltipDescID = desc;
        result.ParamA = paramA + 1;
        result.ParamB = paramB + 1;
        return result;
    }

    private ObjectiveEffectData RevealEffect(IEnumerable<int> targets)
    {
        return Effect(EObjectiveEffect.Reveal, targets);
    }

    private ObjectiveEffectData RevealEffect(params int[] targets)
    {
        return Effect(EObjectiveEffect.Reveal, targets);
    }

    private ObjectiveEffectData LoseEffect()
    {
        return Effect(EObjectiveEffect.Win, false);
    }

    private ObjectiveEffectData ActivateEffect(bool activate, params int[] objs)
    {
        return Effect(EObjectiveEffect.ActivateObjectives, activate, objs);
    }

    private ObjectiveEffectData FinishEffect(bool activate, params int[] objs)
    {
        return Effect(EObjectiveEffect.SucceedObjectives, activate, objs);
    }

    private ObjectiveEffectData TimerEffect(bool show, int ticks, string text, string tooltipTitle, string tooltipDesc)
    {
        var result = Effect(EObjectiveEffect.ShowTimer, show);
        result.Minutes = ticks;
        result.TimerID = text;
        result.TimerTooltipTitleID = tooltipTitle;
        result.TimerTooltipDescID = tooltipDesc;
        return result;
    }

    private ObjectiveEffectData CustomMissionEffect(int nodeID, int hours, int bombers, int fighters, int torpedoes)
    {
        var result = Effect(EObjectiveEffect.CustomMissionRetrieval);
        result.RetrievePosition = nodeID;
        result.HoursToRetrieve = hours;
        result.BombersNeeded = bombers;
        result.FightersNeeded = fighters;
        result.TorpedoesNeeded = torpedoes;
        return result;
    }

    private ObjectiveEffectData SpawnSurvivorsEffect(List<int> targets)
    {
        return Effect(EObjectiveEffect.SpawnSurvivors, targets);
    }

    private ObjectiveEffectData ShowRescueRangeEffect()
    {
        return Effect(EObjectiveEffect.ShowRescueRange);
    }

    private ObjectiveEffectData SpawnRescueShipEffect()
    {
        return Effect(EObjectiveEffect.SpawnRescueShip);
    }

    private ObjectiveEffectData ShowSurvivorsEffect()
    {
        return Effect(EObjectiveEffect.ShowSurvivors);
    }

    private ObjectiveEffectData ShowAllyEffect(params int[] targets)
    {
        return Effect(EObjectiveEffect.ShowAlly, targets);
    }

    private ObjectiveEffectData DetectPlayerEffect()
    {
        return Effect(EObjectiveEffect.DetectPlayer);
    }

    private ObjectiveEffectData SpawnNeutralEffect(IEnumerable<int> targets)
    {
        return Effect(EObjectiveEffect.SpawnNeutral, targets);
    }

    private IEnumerable<ObjectiveData> ObjectivesSetText(int count, List<string> titleIDs, List<string> descIDs, ObjectiveData data, bool nocheck = false)
    {
        data.Title = titleIDs[0];
        data.Description = descIDs[0];
        data.Params = new string[] { "0", count.ToString() };
        if (data.Effects == null)
        {
            data.Effects = new List<ObjectiveEffectData>();
        }

        int objID = dummy.Objectives.Count;
        Assert.IsTrue(nocheck || objID == 0);
        data.Effects.Insert(0, SetObjectiveTextEffect(objID, titleIDs[count], descIDs[count], count, count));

        dummy.Objectives.Add(data);
        yield return data;

        for (int i = 1; i <= count; i++)
        {
            data = new ObjectiveData();
            data.Effects = new List<ObjectiveEffectData>() { SetObjectiveTextEffect(objID, titleIDs[i], descIDs[i], i, count) };
            dummy.Objectives.Add(data);
            yield return data;
        }
    }

    private void Destroy1(string titleID, string descID, ObjectiveData data)
    {
        data.Title = titleID;
        data.Description = descID;
        TargetiveObjective(ref data, EObjectiveType.Destroy, EObjectiveTarget.All, 0, 0);
        dummy.Objectives.Add(data);
    }

    private void TargetiveObjective(EObjectiveType type, List<string> titleIDs, List<string> descIDs, ObjectiveData data, bool check, params int[] targets)
    {
        TargetiveObjective(type, titleIDs, descIDs, data, check, (ICollection<int>)targets);
    }

    private void TargetiveObjective(EObjectiveType type, List<string> titleIDs, List<string> descIDs, ObjectiveData data, bool check, ICollection<int> targets)
    {
        var target = EObjectiveTarget.All;
        int count2 = 0;
        foreach (var obj in ObjectivesSetText(targets.Count, titleIDs, descIDs, data, !check))
        {
            data = obj;
            TargetiveObjective(ref data, type, target, count2, targets);
            data.Active = true;

            target = EObjectiveTarget.Number;
            count2++;
        }
    }

    private void TargetiveObjective(EObjectiveType type, List<string> titleIDs, List<string> descIDs, ObjectiveData data, params int[] targets)
    {
        TargetiveObjective(type, titleIDs, descIDs, data, true, targets);
    }

    private void Objective(EObjectiveType type, int count, List<string> titleIDs, List<string> descIDs, ObjectiveData data)
    {
        var target = EObjectiveTarget.All;
        int count2 = 0;
        foreach (var obj in ObjectivesSetText(count, titleIDs, descIDs, data))
        {
            data = obj;
            Objective(ref data, type, target, count2);
            data.Active = true;

            target = EObjectiveTarget.Number;
            count2++;
        }
    }

    private void SpawnRetrieveTimedSurvivorsUnknownArea(MapSpawnData data, ref ObjectiveData objData, bool moreSurvivors, List<string> titles, List<string> descs)
    {
        int fleetCount = 0;

        int triplet = UnityRandom.Range(0, data.TripletList.Count);
        intHelper.Clear();
        intHelper.AddRange(GetTripletIndices(data.TripletList[triplet], 8));
        Assert.IsTrue(intHelper.Count == 4);

        int value = 2;
        if (moreSurvivors)
        {
            value++;
        }
        int count = 0;
        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
                foreach (var objData2 in ObjectivesSetText(value, titles, descs, objData))
                {
                    var newObjData = objData2;
                    RescueSurvivorsObjective(ref newObjData, count++);
                    newObjData.Active = true;
                }
                intHelper.RemoveAt(UnityRandom.Range(1, intHelper.Count));
                break;
            case EMissionDifficulty.Medium:
            case EMissionDifficulty.Hard:
                foreach (var objData2 in ObjectivesSetText(value + 1, titles, descs, objData))
                {
                    var newObjData = objData2;
                    RescueSurvivorsObjective(ref newObjData, count++);
                    newObjData.Active = true;
                }
                fleetCount = 2;
                break;
            default:
                Assert.IsTrue(false);
                break;
        }

        dummy.Objectives[0].Count = count - 1;
        if (moreSurvivors)
        {
            count--;
        }

        intHelper2.Clear();
        for (int i = 0; i < count; i++)
        {
            intHelper2.Add(i);
        }

        objData = new ObjectiveData();
        objData.Type = EObjectiveType.CustomEscortDestroyed;
        objData.Active = true;
        objData.LoseType = EMissionLoseCause.SandboxDestroyedRescueShip;
        objData.Effects = new List<ObjectiveEffectData>() { LoseEffect() };
        dummy.Objectives.Add(objData);

        objData = new ObjectiveData();
        InstantObjective(ref objData, SpawnRescueShipEffect(), SpawnSurvivorsEffect(intHelper2), ShowRescueRangeEffect());
        dummy.Objectives.Add(objData);

        CreateFleet(count, dummy.Nodes, intHelper);
        foreach (var fleet in dummy.EnemyUnits)
        {
            fleet.IsDisabled = true;
        }

        CreateFleet(fleetCount, dummy.Nodes, LongFlagList(data.LongFlagList, triplet));
        for (int i = 0; i < fleetCount; i++)
        {
            GetNameFromBasket(dummy.EnemyUnits[i], enemyFleetNamesBucket, enemyFleetNames);
        }
    }

    private void SetRoutes(MapSpawnData data, ref SandboxSpawnMapData saveData, int count)
    {
        switch (data.Difficulty)
        {
            case EMissionDifficulty.Easy:
                saveData.ObjectiveFleets = 1;
                break;
            case EMissionDifficulty.Medium:
            case EMissionDifficulty.Hard:
                saveData.ObjectiveFleets = 2;
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        for (int i = 0; i < saveData.ObjectiveFleets + count; i++)
        {
            SetRoute(saveData.Routes, i);
        }
    }

    private void SetRoute(List<ListInt> routes, int index)
    {
        routes[index].List.AddRange(dummy.EnemyUnits[index].RandomNodes);
    }

    private void SetRouteFromPatrols(List<ListInt> routes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            routes[i].List.Add(dummy.EnemyUnits[i].Patrols[0].Poses[0]);
        }
    }

    private void GetRoute(List<ListInt> routes, int index)
    {
        var nodes = dummy.EnemyUnits[index].RandomNodes;
        var list = routes[index].List;
        nodes.Clear();
        nodes.AddRange(list);
    }

    private void GetRouteToPatrols(List<ListInt> routes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            dummy.EnemyUnits[i].Patrols[0].Poses[0] = routes[i].List[0];
        }
    }

    private void SetBlocksIndex(List<EnemyUnitData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].SaveIndex = i;
        }
    }

    private void GetNameFromBasket(EnemyUnitData data, List<string> basket, List<string> list)
    {
        if (basket.Count == 0)
        {
            basket.AddRange(list);
        }
        data.NameID = RandomUtils.GetRemoveRandom(basket);
    }

    private void GetBlocks(List<int> list, int index, UnitBucketData bucket)
    {
        var enemy = dummy.EnemyUnits[index];
        enemy.BuildingBlocks.Clear();

        var savedEnemy = bucket.FromIndex(list[index]);
        enemy.BuildingBlocks.AddRange(savedEnemy.BuildingBlocks);
        enemy.SaveIndex = savedEnemy.SaveIndex;
    }

    private void GetBlocks(List<int> list, int index, List<EnemyUnitData> unitList)
    {
        var enemy = dummy.EnemyUnits[index];
        enemy.BuildingBlocks.Clear();
        enemy.BuildingBlocks.AddRange(unitList[list[index]].BuildingBlocks);
    }

    private void SetupEffectTargets(List<int> data)
    {
        var targets = dummy.Objectives[dummy.Objectives.Count - 1].Effects[0].Targets;
        targets.Clear();
        targets.AddRange(data);
    }

    private float AirstrikeSqrRange()
    {
        var result = tacticManager.GetAirstrikeMaxRange() * 1.2f;
        result *= result;
        return result;
    }

    private IEnumerable<Vector2> ToVector2(IEnumerable<MyVector2> list)
    {
        foreach (var vec in list)
        {
            yield return vec;
        }
    }

    private IEnumerable<MyVector2> ToMyVector2(IEnumerable<Vector2> list)
    {
        foreach (var vec in list)
        {
            yield return vec;
        }
    }

    private void Save(ref List<int> save, List<string> list, List<string> bucket)
    {
        if (save == null)
        {
            save = new List<int>();
        }
        save.Clear();

        for (int i = 0; i < bucket.Count; i++)
        {
            save.Add(list.IndexOf(bucket[i]));
        }
    }

    private void Load(List<int> save, List<string> list, List<string> bucket)
    {
        bucket.Clear();
        for (int i = 0; i < save.Count; i++)
        {
            bucket.Add(list[save[i]]);
        }
    }
}
