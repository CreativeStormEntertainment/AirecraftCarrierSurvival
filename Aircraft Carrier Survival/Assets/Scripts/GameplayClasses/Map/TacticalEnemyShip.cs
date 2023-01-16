using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using UnityRandom = UnityEngine.Random;

public class TacticalEnemyShip : TacticalObject, ITickable
{
    public bool BaseCanAttack => canAttack;
    public bool CanAttack => canAttack && playerAttackTicks <= 0;

    public PatrolData CurrentPatrol => patrols[patrolIndex];

    public bool AlreadyRetreats => alreadyRetreats;

    public int BonusManeuversDefence
    {
        get;
        set;
    }

    public bool Reconed
    {
        get;
        set;
    }

    public bool Spotted
    {
        get;
        set;
    }

    public bool UpdateRealtime
    {
        get;
        set;
    }

    public bool Ignore
    {
        get;
        set;
    }

    public bool Dead
    {
        get;
        private set;
    }

    public int Id
    {
        get;
        private set;
    }

    public string LocalizedName
    {
        get;
        private set;
    }

    public List<EnemyManeuverInstanceData> Blocks
    {
        get;
        private set;
    }

    public int PatrolNodeIndex
    {
        get;
        private set;
    }

    public bool FinishedPatrol
    {
        get;
        private set;
    }

    public float AttackRangeSqr
    {
        get;
        private set;
    }
    public float AttackRange
    {
        get;
        private set;
    }
    public float DetectRange
    {
        get;
        private set;
    }

    public bool IsDisabled
    {
        get;
        private set;
    }

    public float OverrideChanceToReveal
    {
        get;
        private set;
    }

    public bool CanReceiveCAP
    {
        get;
        private set;
    }

    public bool NotByPlayer
    {
        get;
        private set;
    }

    public bool NotTargetable
    {
        get;
        private set;
    }

    [SerializeField]
    private RectTransform shipImage = null;
    [SerializeField]
    private CartRandom attackChanceRandom = null;
    [SerializeField]
    private float chaseRange = 20f;

    private int patrolIndex = 0;
    private TacticManager tacticManager = null;
    private EnemyAttacksManager enemyAttacksManager = null;
    private List<PathNode> openList;
    private HashSet<PathNode> closeList;
    private List<PathNode> path;
    private int pathIndex = 0;
    private HashSet<int> patrolsAvailable;

    private List<PatrolData> patrols;
    private List<PathNode> patrolsNodes;

    private float patrolNodeOffset = 0;

    private bool canChase = false;

    private Vector2 prevPos = Vector2.zero;
    private Vector2 pos = Vector2.zero;
    private Vector2 moveDir = Vector2.zero;

    private float speed = 0f;

    private bool chaseMode;

    private bool firstPositionSetup = true;

    private bool canRetreat;
    private bool alreadyRetreats;

    private float revealRange;

    private List<NodeData> nodesRetreat;

    private bool searchAndDestroy;
    private TacticalEnemyShip destroyTarget;
    private List<TacticalEnemyShip> potentialTargets;

    private float detectRangeSqr;

    private float missedTimes;
    private int tickTime;
    private int attackTimer;

    private bool active;

    private bool canAttack;
    private int playerAttackTicks;
    private int attackCooldownTicks;

    private Dictionary<EMisidentifiedType, List<EnemyManeuverData>> misidentifieds;

    private List<PathNode> nodesHelper;
    private float patrolNodeOffsetSqr;
    private Vector2 scale;
    private Vector2Int counts;

    private HashSet<int> originalRandomNodes;
    private HashSet<int> randomNodes;
    private List<int> helper;
    private int previousRandomNode;

    private bool denied;
    private bool ignore;
    private bool ignore2;

    private int chaseIndex;
    private HashSet<int> chaseHelper;
    private List<int> chaseListHelper;
    private Vector2 chasePlayerPos;

    private Vector2? distractTarget;

    private bool alternativePathfinding;
    private bool fromChase;

    private void OnDestroy()
    {
        tacticManager = TacticManager.Instance;
        if (tacticManager != null)
        {
            tacticManager.NodesChanged -= OnNodesChanged;
            tacticManager.RandomNodeChosen -= OnRandomNodeChosen;
            tacticManager.PotentialRandomNodeChosen -= OnPotentialRandomNodeChosen;
            tacticManager.RandomNodeDenied -= OnRandomNodeDenied;

            if (tacticManager.Carrier != null)
            {
                tacticManager.Carrier.ChaseNodesChanged -= OnChaseNodesChanged;
            }
        }
        enemyAttacksManager = EnemyAttacksManager.Instance;
        if (enemyAttacksManager != null)
        {
            enemyAttacksManager.DetectedChanged -= OnDetectedChanged;
        }
    }

    public void BasicSetup(int id, ETacticalObjectSide side, List<PatrolData> patrols, float patrolOffset, float speed, Vector2 position, bool notTargetable, bool fleetOverride)
    {
        chaseIndex = -1;
        if (side == ETacticalObjectSide.Enemy)
        {
            tacticManager.Carrier.ChaseNodesChanged += OnChaseNodesChanged;
        }
        chaseHelper = new HashSet<int>();
        chaseListHelper = new List<int>();

        active = true;
        openList = new List<PathNode>();
        closeList = new HashSet<PathNode>();

        RectTransform = shipImage;

        ChangeSide(side);

        Id = id;
        enemyAttacksManager = EnemyAttacksManager.Instance;
        this.patrols = patrols;
        this.speed = speed;

        NotTargetable = notTargetable;

        potentialTargets = new List<TacticalEnemyShip>();

        //#instead of everytime ifcheck, create list at the beginning
        foreach (var patrol in patrols)
        {
            patrol.Init();
        }

        patrolNodeOffset = patrolOffset;

        Blocks = new List<EnemyManeuverInstanceData>();

        SetEnemyPosition(position);

        if (patrols.Count == 0 && !fleetOverride)
        {
            ChangeType(ETacticalObjectType.Outpost);
        }
        else
        {
            tacticManager.NodesChanged += OnNodesChanged;
            ChangeType(ETacticalObjectType.StrikeGroup);

            if (!fleetOverride)
            {
                patrolsAvailable = new HashSet<int>();
                //#there was always 1. patrol at start
                NewPatrol();
            }

            if (GameSceneManager.Instance.IsLoading)
            {
                path = new List<PathNode>();
            }
            else
            {
                //SetPatrolNodes(patrolNodes);
                path = FindPath(fleetOverride, false);
                DrawPath();
                if (path.Count == 0)
                {
                    Debug.LogError("Empty path");
                }
            }
        }
        if (!IsDisabled)
        {
            EnableShip();
        }

        nodesRetreat = new List<NodeData>();
        for (int i = 0; i < 4; i++)
        {
            nodesRetreat.Add(new NodeData());
        }
    }

    public void Setup(int id, EnemyUnitData enemy, List<EnemyManeuverData> buildingBlocks, Dictionary<EMisidentifiedType, List<EnemyManeuverData>> misidentifieds,
        Dictionary<EMisidentifiedType, HashSet<EnemyManeuverData>> misidentifiedsDict, List<int> instantDeadBlocks, bool weaker)
    {
        alternativePathfinding = enemy.AlternativePathfinding;
        tacticManager = TacticManager.Instance;
        bool sandboxFleet = enemy.RandomNodes != null && enemy.RandomNodes.Count > 0;
        if (sandboxFleet)
        {
            originalRandomNodes = new HashSet<int>();
            randomNodes = new HashSet<int>();
            helper = new List<int>();
            foreach (int index in enemy.RandomNodes)
            {
                originalRandomNodes.Add(index);
                randomNodes.Add(index);
            }
            previousRandomNode = -1;
            tacticManager.RandomNodeChosen += OnRandomNodeChosen;
            tacticManager.PotentialRandomNodeChosen += OnPotentialRandomNodeChosen;

            if (enemy.Patrols.Count == 0)
            {
                PatrolNodeIndex = -1;
            }
        }

        IsDisabled = enemy.IsDisabled;
        revealRange = enemy.RevealRange;

        BasicSetup(id, enemy.IsAlly ? ETacticalObjectSide.Friendly : ETacticalObjectSide.Enemy, enemy.Patrols, enemy.MaxOffset, enemy.Speed, enemy.Position, enemy.NotTargetable, sandboxFleet);

        GreaterInvisibility = enemy.IsDetectableMissionOnly;
        HadGreaterInvisibility = GreaterInvisibility;
        Invisible = GreaterInvisibility || enemy.IsHidden;

        DetectRange = enemy.DetectRange;
        detectRangeSqr = enemy.DetectRange * enemy.DetectRange;

        OverrideChanceToReveal = enemy.OverrideChanceToReveal;
        canChase = enemy.CanChase;
        canRetreat = enemy.CanRetreat;

        CanReceiveCAP = enemy.CanReceiveCAP;

        var locMan = LocalizationManager.Instance;
        if (!string.IsNullOrEmpty(enemy.NameID))
        {
            LocalizedName = locMan.GetText(enemy.NameID);
        }
        int count = buildingBlocks.Count;
        if (weaker && count > 1)
        {
            count--;
        }
        this.misidentifieds = misidentifieds;
        for (int i = 0; i < count; i++)
        {
            var data = buildingBlocks[i];
            var block = new EnemyManeuverInstanceData(data);

            if (data.MisidentifiedType == EMisidentifiedType.Unique)
            {
                block.Alternative = block.Data;
            }
            else
            {
                var pool = misidentifiedsDict[data.MisidentifiedType];
                if (!pool.Remove(data))
                {
                    Debug.LogError("" + id + " " + enemy.Name + " " + enemy.NameID + " " + data.name +  " - wtf");
                }
                if (pool.Count == 0)
                {
                    block.Alternative = block.Data;
                    Debug.LogError(data.Name + " - wtf2");
                }
                else
                {
                    block.Alternative = RandomUtils.GetRandom(pool);
                }
                pool.Add(data);
            }
            block.Visible = enemy.IsAlly;
            Blocks.Add(block);
            data.LocalizedName = locMan.GetText(data.Name);
            data.LocalizedDescription = locMan.GetText(data.Description);
            block.Alternative.LocalizedName = locMan.GetText(block.Alternative.Name);
            block.Alternative.LocalizedDescription = locMan.GetText(block.Alternative.Description);
        }
        if (enemy.IsAlly)
        {
            var bonusBlock = Type == ETacticalObjectType.StrikeGroup ? tacticManager.BonusAllyStrikeGroupBlock : tacticManager.BonusAllyOutpostBlock;
            bonusBlock.LocalizedName = locMan.GetText(bonusBlock.Name);
            bonusBlock.LocalizedDescription = locMan.GetText(bonusBlock.Description);
            for (int i = 0; i < tacticManager.BonusAllyBlocks; i++)
            {
                var block = new EnemyManeuverInstanceData(bonusBlock);
                block.Visible = true;
                Blocks.Add(block);
            }
        }

        if (instantDeadBlocks != null)
        {
            foreach (int index in instantDeadBlocks)
            {
                if (index > -1 && index < Blocks.Count)
                {
                    Blocks[index].Dead = true;
                }
            }
        }

        if (enemy.IsAlly || enemy.AttackRange < .1f)
        {
            canAttack = false;
            AttackRangeSqr = 0f;
            AttackRange = 0f;
        }
        else
        {
            canAttack = true;
            AttackRangeSqr = enemy.AttackRange * enemy.AttackRange;
            AttackRange = enemy.AttackRange;
        }
        if (Side == ETacticalObjectSide.Neutral)
        {
            Retreat();
        }
    }

    public void SetupFromEnemy(int id, ETacticalObjectSide side, TacticalEnemyShip fromEnemy)
    {
        tacticManager = TacticManager.Instance;
        BasicSetup(id, side, fromEnemy.patrols, fromEnemy.patrolNodeOffset, fromEnemy.speed, fromEnemy.pos, false, false);
    }

    public void SetSearchAndDestroy(bool set, int attackTime, TacticalEnemyShip target = null)
    {
        attackChanceRandom.Init();
        searchAndDestroy = set;
        destroyTarget = null;
        //todo on false some changes to patrol?

        tickTime = 0;
        attackTimer = attackTime;

        if (set && target != null && !target.Dead && !target.IsDisabled)
        {
            destroyTarget = target;
            if (destroyTarget.destroyTarget == null && !target.alreadyRetreats)
            {
                destroyTarget.SetSearchAndDestroy(true, attackTime, this);
            }
        }
    }

    public void LoadData(EnemyShipData data)
    {
        if (data.Dead)
        {
            NotByPlayer = data.NotByPlayer;
            MakeDead();
        }
        else
        {
            Reconed = data.Reconed;
            Spotted = data.Spotted;
            Special = data.Special;

            SetEnemyPosition(data.ObjectData.Position);
            shipImage.rotation = Quaternion.Euler(0f, 0f, data.ObjectData.Direction.X);
            canChase = data.CanChase;
            if (IsDisabled && !data.Disabled)
            {
                EnableShip();
            }
            if (!data.GreaterInvisibility)
            {
                GreaterInvisibility = false;
            }
            if (!data.Invisible)
            {
                Invisible = false;
            }
            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                if (data.BlocksData.Count <= i)
                {
                    block.Visible = true;
                    var type = misidentifieds[block.Data.MisidentifiedType];

                    Debug.LogError($"{Id} - block {i} is MAJOR broken :( - check enemy block, should be different");
                    int index = type.IndexOf(block.Data);
                    if (index != -1)
                    {
                        type.RemoveAt(index);
                    }
                    block.Alternative = type.Count > 0 ? RandomUtils.GetRandom(type) : block.Data;
                    if (index != -1)
                    {
                        type.Insert(index, block.Data);
                    }
                    if (block.Data == block.Alternative)
                    {
                        block.Visible = true;
                    }
                    continue;
                }
                var blockData = data.BlocksData[i];
                block.Visible = blockData.Revealed;
                block.CurrentDurability = Mathf.Min(block.Data.Durability, blockData.Durability);
                block.Dead = block.CurrentDurability < 1;
                if (blockData.Alternative == -2)
                {
                    block.Alternative = null;
                }
                else if (blockData.Alternative == -1)
                {
                    block.Alternative = block.Data;
                }
                else
                {
                    var type = misidentifieds[block.Data.MisidentifiedType];
                    if (type.Count > blockData.Alternative)
                    {
                        block.Alternative = type[blockData.Alternative];
                    }
                    else
                    {
                        Debug.LogError($"{Id} - block {i} has broken alternative - check enemy block, should be different");
                        int index = type.IndexOf(block.Data);
                        if (index != -1)
                        {
                            type.RemoveAt(index);
                        }
                        block.Alternative = type.Count > 0 ? RandomUtils.GetRandom(type) : block.Data;
                        if (index != -1)
                        {
                            type.Insert(index, block.Data);
                        }
                        if (block.Data == block.Alternative)
                        {
                            block.Visible = true;
                        }
                    }
                }
            }

            Visible = data.ObjectData.Revealed;
            UpdateRealtime = data.UpdateRealtime;

            attackChanceRandom.LoadData(ref data.AttackChanceData);

            if (data.SearchAndDestroy)
            {
                SetSearchAndDestroy(true, data.AttackTimer);
                destroyTarget = data.TargetID < 0 ? null : tacticManager.GetShip(data.TargetID);
            }

            playerAttackTicks = data.PowerfulTicks;
            attackCooldownTicks = data.CooldownTicks;

            FinishedPatrol = data.FinishedPatrol;
            if (data.IsRetreating)
            {
                Retreat();
            }
            else
            {
                if (originalRandomNodes != null && originalRandomNodes.Count != 0)
                {
                    previousRandomNode = -1;
                    randomNodes.Clear();
                    if (data.StartNode == -1)
                    {
                        foreach (int node in data.RandomNodes)
                        {
                            randomNodes.Add(node);
                        }
                    }
                    else
                    {
                        randomNodes.Add(data.StartNode);
                    }

                    Assert.IsFalse(ignore);
                    ignore = true;
                    pathIndex = 0;
                    path = FindPath(true, false);
                    if (path.Count == 0)
                    {
                        Debug.LogError("Empty path");
                    }
                    Assert.IsTrue(ignore);
                    ignore = false;

                    randomNodes.Clear();
                    foreach (int node in data.RandomNodes)
                    {
                        randomNodes.Add(node);
                    }
                    randomNodes.Remove(previousRandomNode);
                }
                else if (patrols.Count != 0)
                {
                    patrolIndex = data.CurrentPatrol;
                    PatrolNodeIndex = data.PatrolNodeIndex;

                    pathIndex = 0;

                    if (data.HasDistract)
                    {
                        distractTarget = data.DistractPosition;
                    }

                    path = FindPath(false, data.FromChase);
                    DrawPath();
                    if (path.Count == 0)
                    {
                        Debug.LogError("Empty path");
                    }
                }
            }
        }
    }

    public EnemyShipData SaveData()
    {
        var result = new EnemyShipData();
        result.BlocksData = new List<EnemyBlockSaveData>();

        if (Dead)
        {
            result.Dead = true;
            result.NotByPlayer = NotByPlayer;
        }
        else
        {
            result.Dead = false;
            result.ObjectData.Position = shipImage.anchoredPosition;
            result.ObjectData.Direction.X = shipImage.rotation.eulerAngles.z;
            result.Reconed = Reconed;
            result.Spotted = Spotted;
            result.Special = Special;

            if (distractTarget.HasValue)
            {
                result.HasDistract = true;
                result.DistractPosition = distractTarget.Value;
            }
            else
            {
                result.HasDistract = false;
            }

            foreach (var block in Blocks)
            {
                var data = new EnemyBlockSaveData();
                data.Revealed = block.Visible;
                if (block.Dead)
                {
                    data.Durability = 0;
                }
                else
                {
                    data.Durability = block.CurrentDurability;
                }
                if (block.Alternative == null)
                {
                    data.Alternative = -2;
                }
                else if (block.Alternative == block.Data)
                {
                    data.Alternative = -1;
                }
                else
                {
                    data.Alternative = misidentifieds[block.Data.MisidentifiedType].IndexOf(block.Alternative);
                }
                result.BlocksData.Add(data);
            }

            result.CanChase = canChase;
            result.Disabled = IsDisabled;
            result.Invisible = Invisible;

            result.SearchAndDestroy = searchAndDestroy;
            result.AttackTimer = attackTimer;
            result.TargetID = destroyTarget == null ? -1 : destroyTarget.Id;

            result.GreaterInvisibility = GreaterInvisibility;
            result.IsRetreating = alreadyRetreats;
            result.UpdateRealtime = UpdateRealtime;
            result.FinishedPatrol = FinishedPatrol;

            result.PowerfulTicks = playerAttackTicks;
            result.CooldownTicks = attackCooldownTicks;

            attackChanceRandom.SaveData(ref result.AttackChanceData);

            if (originalRandomNodes != null && originalRandomNodes.Count != 0)
            {
                result.RandomNodes = new List<int>(randomNodes);
                result.StartNode = previousRandomNode;
            }
            else if (!alreadyRetreats)
            {
                result.CurrentPatrol = patrolIndex;
                result.PatrolNodeIndex = PatrolNodeIndex;
                result.FromChase = fromChase;
            }
        }

        return result;
    }

    public void EnableShip()
    {
        IsDisabled = false;
        if (Side == ETacticalObjectSide.Friendly)
        {
            RevealRange();
        }
        TimeManager.Instance.AddTickable(this);
    }

    public void CheckIsDead(bool notByPlayer)
    {
        bool allDestroyed = true;
        bool carrierDead = true;
        foreach (var block in Blocks)
        {
            if (!block.Dead)
            {
                allDestroyed = false;
                if (block.Data.ShipType == EEnemyShipType.Carrier || block.Data.ShipType == EEnemyShipType.Airport)
                {
                    carrierDead = false;
                }
            }
        }
        if (allDestroyed)
        {
            NotByPlayer = notByPlayer;
            MakeDead();
        }
        else
        {
            if (carrierDead)
            {
                canAttack = false;
                if (canRetreat && !alreadyRetreats)
                {
                    Retreat();
                }
            }
            tacticManager.Markers.UpdateAttackRange(this);
        }
    }

    public void SetChase(bool canChase)
    {
        this.canChase = canChase;
        if (!canChase && chaseMode)
        {
            OnDetectedChanged(false);
        }
    }

    public void Die(bool notByPlayer)
    {
        NotByPlayer = notByPlayer;

        MapUnitMaskManager.Instance.RemoveUnitObject(shipImage.gameObject);

        active = false;
        Dead = true;
        canAttack = false;
        TimeManager.Instance.RemoveTickable(this);
    }

    public int GetDeadBlocksCount()
    {
        int deads = 0;
        foreach (var block in Blocks)
        {
            deads += block.Dead ? 1 : 0;
        }
        return deads;
    }

    public void Teleport(Vector2 position)
    {
        pos = position;
        shipImage.anchoredPosition = pos;

        if (!GameSceneManager.Instance.IsLoading && chaseMode)
        {
            path = FindPath(false, false);
            DrawPath();
            if (path.Count == 0)
            {
                Debug.LogError("Empty path");
            }
        }
    }

    public void PowerfulAttack()
    {
        if (attackCooldownTicks <= 0)
        {
            int ticksForHour = TimeManager.Instance.TicksForHour;
            var param = Parameters.Instance;

            playerAttackTicks = param.AttackHinderedHours * ticksForHour;
            attackCooldownTicks = param.AttackCooldownHours * ticksForHour;
        }
    }

    public void Distract(Vector2 pos)
    {
        if (alreadyRetreats || Dead)
        {
            distractTarget = null;
            return;
        }
        distractTarget = pos;

        path = FindPath(false, false);
        DrawPath();
        if (path.Count == 0)
        {
            Debug.LogError("Empty path");
        }
    }

    public void Tick()
    {
        if (!active || GameSceneManager.Instance.IsLoading)
        {
            return;
        }
        switch (Side)
        {
            case ETacticalObjectSide.Friendly:
                tacticManager.NewRevealArea(shipImage.anchoredPosition, revealRange * revealRange, false, EMissionOrderType.None);
                break;
            case ETacticalObjectSide.Enemy:
                var enemyAttacksMan = EnemyAttacksManager.Instance;
                if (!enemyAttacksManager.IsDetected && Vector3.SqrMagnitude(tacticManager.Carrier.Position - shipImage.anchoredPosition) < detectRangeSqr)
                {
                    enemyAttacksMan.Detect();
                }
                if (playerAttackTicks > 0)
                {
                    playerAttackTicks--;
                }
                if (attackCooldownTicks > 0)
                {
                    attackCooldownTicks--;
                }
                break;
        }

        if (searchAndDestroy)
        {
            if (destroyTarget != null && destroyTarget.Dead)
            {
                destroyTarget = null;
            }

            if (destroyTarget == null)
            {
                FindTarget();
                if (destroyTarget == null)
                {
                    SetSearchAndDestroy(false, 0);
                    return;
                }
            }

            float sqrDist = Vector2.SqrMagnitude(pos - destroyTarget.shipImage.anchoredPosition);
            if (sqrDist > 3025f)
            {
                pos = Vector2.MoveTowards(pos, destroyTarget.shipImage.anchoredPosition, speed);
                shipImage.anchoredPosition = pos;
            }

            sqrDist = Vector2.SqrMagnitude(pos - destroyTarget.shipImage.anchoredPosition);
            if (sqrDist < 3600f)
            {
                tacticManager.FireSearchAndDestroyReady();
                if (TimeManager.Instance.IsDay && ++tickTime >= attackTimer && attackChanceRandom.Check())
                {
                    tickTime = 0;
                    var param = Parameters.Instance;
                    float blocksCount = 0f;
                    foreach (var block in Blocks)
                    {
                        if (!block.Dead)
                        {
                            blocksCount++;
                        }
                    }
                    float chance = param.BalancedForcesParameterX * blocksCount + missedTimes * param.BalancedForcesMissedBonus;
                    Assert.IsFalse(destroyTarget.Dead || destroyTarget.IsDisabled);
                    if (UnityRandom.value <= chance)
                    {
                        missedTimes = 0f;
                        foreach (var block in destroyTarget.Blocks)
                        {
                            if (!block.Dead)
                            {
                                block.Dead = true;
                                tacticManager.FireBlockDestroyed(block.Data, false);
                                break;
                            }
                        }
                        destroyTarget.CheckIsDead(true);
                    }
                    else
                    {
                        missedTimes++;
                    }
                }

                if (sqrDist > 0.1f)
                {
                    LookAt(destroyTarget.shipImage.anchoredPosition);
                }
            }
            else
            {
                attackChanceRandom.Init();
                LookAt(destroyTarget.shipImage.anchoredPosition);
            }
        }
        else
        {
            bool sandbox = originalRandomNodes != null && originalRandomNodes.Count != 0;
            if (patrols.Count == 0 && !sandbox)
            {
                return;
            }

            if (pathIndex < 0 || pathIndex >= path.Count)
            {
                if (alreadyRetreats &&
                    ((Mathf.Abs(shipImage.anchoredPosition.x - (((shipImage.anchoredPosition.x < 0f) ? -1f : 1f) * 950f)) > 1f) ||
                     (Mathf.Abs(shipImage.anchoredPosition.y - (((shipImage.anchoredPosition.y < 0f) ? -1f : 1f) * 520f)) > 1f)))
                {
                    Debug.Log("Retreated enemy destroyed");
                    tacticManager.DestroyObject(Id, true);
                    return;
                }
                pathIndex = 0;
                //#wrong condition
                if (!chaseMode && !distractTarget.HasValue)
                {
                    if (PatrolNodeIndex != -1)
                    {
                        tacticManager.FireDestinationReached(Id, CurrentPatrol.Poses[PatrolNodeIndex]);
                    }
                    else if (sandbox)
                    {
                        tacticManager.FireDestinationReached(Id, previousRandomNode);
                    }
                }
                if ((!enemyAttacksManager.IsDetected || !canChase) && !sandbox)
                {
                    //When enemy patroling
                    if (distractTarget.HasValue)
                    {
                        distractTarget = null;
                    }
                    else
                    {
                        PatrolNodeIndex++;
                    }
                    if (PatrolNodeIndex >= patrols[patrolIndex].SNodePoses.Count)
                    {
                        FinishedPatrol = true;
                        PatrolNodeIndex = 0;
                        NewPatrol();
                    }
                }

                path = FindPath(sandbox, chaseMode);
                DrawPath();
                if (path.Count == 0)
                {
                    Debug.LogError("Empty path");
                }
            }

#if ALLOW_CHEATS
            float newSpeed = (Side == ETacticalObjectSide.Friendly && TacticManager.Instance.FastFriends) ? 10f : 1f * speed;
            pos = Vector2.MoveTowards(pos, path[pathIndex].Position, newSpeed);
#else
            pos = Vector2.MoveTowards(pos, path[pathIndex].Position, speed);
#endif
            if ((pos - path[pathIndex].Position).sqrMagnitude < 0.1f)
            {
                pathIndex++;
            }
            shipImage.anchoredPosition = pos;
            LookAt();
            if (alreadyRetreats)
            {
                return;
            }

            //#wrong checks as we discussed
            if (Side == ETacticalObjectSide.Enemy && !chaseMode && enemyAttacksManager.IsDetected && canChase)
            {
                //#you forgot about breaking chasing, when player detection changes to undetected
                enemyAttacksManager.DetectedChanged -= OnDetectedChanged;
                enemyAttacksManager.DetectedChanged += OnDetectedChanged;

                path = FindPath(false, false);
                DrawPath();
                if (path.Count == 0)
                {
                    Debug.LogError("Empty path");
                }
            }
        }

        if (firstPositionSetup)
        {
            InstantUpdate = true;
            firstPositionSetup = false;
        }
    }

    private void MakeDead()
    {
        var tacMan = TacticManager.Instance;
        tacMan.DestroyObject(Id, NotByPlayer);
    }

    private void Retreat()
    {
        Assert.IsFalse(GameSceneManager.Instance.IsLoading);

        alreadyRetreats = true;
        SetSearchAndDestroy(false, 0);

        pathIndex = 0;
        path = FindPath(false, false);
        DrawPath();
        if (path.Count == 0)
        {
            Debug.LogError("Empty path");
        }
    }

    private void DrawPath()
    {
        if (tacticManager.EnemyPathVisible)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Debug.DrawLine(transform.TransformPoint(path[i - 1].Position), transform.TransformPoint(path[i].Position), Color.green, 10);
            }
        }
    }

    private List<PathNode> FindPath(bool sandbox, bool fromChase)
    {
        this.fromChase = false;
        int prev = previousRandomNode;

        chaseMode = enemyAttacksManager.IsDetected && canChase;
        if (!chaseMode)
        {
            this.fromChase = fromChase;
            if (chaseIndex != -1)
            {
                ClearChaseIndices();
            }
            chaseIndex = -1;
        }
        var endNode = FindEndNode(sandbox, out bool alternativePath);
        return FindPath(endNode, sandbox, alternativePath, prev);
    }

    private List<PathNode> FindPath(PathNode endNode, bool sandbox = false, bool alternativePath = false, int prev = -1)
    {
        var result = FindPath(endNode, sandbox, prev, alternativePath, false);
        if (result == null)
        {
            Debug.LogError($"Cannot find node, island problem, {Id} - {tacticManager.MapNodes.Find(pos).Position} -> {endNode.Position}");
            result = FindPath(endNode, sandbox, prev, alternativePath, true);
        }

        return result;
    }

    private List<PathNode> FindPath(PathNode endNode, bool sandbox, int prev, bool alternativePath, bool ignoreLand)
    {
        var startNode = tacticManager.MapNodes.Find(pos);
        if (alternativePath)
        {
            return new List<PathNode>() { startNode, endNode };
        }

        if (!alreadyRetreats && endNode.IsOnLand)
        {
            var error = endNode.Position.ToString() +  " - Cannot find end node on sea - ";
            if (enemyAttacksManager.IsDetected && canChase)
            {
                error += "near player";
            }
            else if (sandbox)
            {
                error += $"enemy id: {Id}; prev node: {prev}; current: {previousRandomNode}";
            }
            else
            {
                error += "enemy id: " + Id + "; patrolIndex: " + patrolIndex + "; nodeIndex - " + PatrolNodeIndex;
            }
            Debug.LogError(error);
        }

        //#QoL, hashSet is better suited if you want to use mainly Contains, 
        //dont use new if you don't have to, why unnecessary garbage?
        openList.Clear();
        openList.Add(startNode);
        closeList.Clear();

        //#QoL
        foreach (var pathNode in tacticManager.MapNodes.GetNodes())
        {
            pathNode.GCost = int.MaxValue;
            pathNode.CalculateFCost();
            pathNode.CameFromNode = null;
        }

        startNode.GCost = 0;
        startNode.HCost = TacticalMapGrid.CalculateRemainingDistance(startNode.MapSNode, endNode.MapSNode);
        startNode.CalculateFCost();

        bool checkLand = !ignoreLand && !startNode.IsOnLand && !endNode.IsOnLand;

        while (openList.Count > 0)
        {
            var currentNode = TacticalMapGrid.GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                //PrintTime("FindPath", false);
                prevPos = pos;
                return TacticalMapGrid.CalculatePathHelper(endNode);
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach (var nNode in currentNode.GetNeighbourList(tacticManager.MapNodes))
            {
                if (closeList.Contains(nNode))
                {
                    continue;
                }

                if (checkLand && nNode.IsOnLand)
                {
                    closeList.Add(nNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + TacticalMapGrid.CalculateRemainingDistance(currentNode.MapSNode, nNode.MapSNode);
                if (tentativeGCost < nNode.GCost)
                {
                    nNode.CameFromNode = currentNode;
                    nNode.GCost = tentativeGCost;
                    nNode.HCost = TacticalMapGrid.CalculateRemainingDistance(nNode.MapSNode, endNode.MapSNode);
                    nNode.CalculateFCost();

                    if (!openList.Contains(nNode))
                    {
                        openList.Add(nNode);
                    }
                }
            }
        }

        //PrintTime("FindPath", false);
        return null;
    }

    private PathNode FindEndNode(bool sandbox, out bool alternativePath)
    {
        alternativePath = false;
        int oldNode = previousRandomNode;
        previousRandomNode = -1;

        //dont assign value if you don't use it
        PathNode result;
        if (alreadyRetreats)
        {
            result = FindEdgeNodeWithOffset();
        }
        else if (enemyAttacksManager.IsDetected && canChase)
        {
            var player = tacticManager.Carrier;
            if (player.ChaseNodes.Count == 0)
            {
                if (TacticManager.Instance.MapNodes == null)
                {
                    Debug.LogError("FATAL ERROR BUUUU");
                    throw new System.Exception();
                }
                player.FillChaseNodes();
                if (player.ChaseNodes.Count == 0)
                {
                    Debug.LogError("FATAL ERROR 2 BUUUU");
                    throw new System.Exception();
                }
            }
            if (chaseIndex != -1)
            {
                chaseIndex = ClearChaseIndices();
            }
            bool add = player.FreeChaseNodes.Remove(chaseIndex);
            chaseHelper.Clear();

            int mode = -1;

            var pos = RectTransform.anchoredPosition;
            float maxDist = Vector2.SqrMagnitude(pos - player.Position);
            for (int i = 0; i < player.ChaseNodes.Count; i++)
            {
                if (Vector2.SqrMagnitude(pos - player.ChaseNodes[i]) < maxDist)
                {
                    mode = 0;
                    chaseHelper.Add(i);
                }
            }

            if (player.FreeChaseNodes.Count != 0)
            {
                if (chaseHelper.Count == 0)
                {
                    foreach (int index in player.FreeChaseNodes)
                    {
                        mode = 1;
                        chaseHelper.Add(index);
                    }
                }
                else
                {
                    foreach (int index in chaseHelper)
                    {
                        if (!player.FreeChaseNodes.Contains(index))
                        {
                            chaseListHelper.Add(index);
                        }
                    }
                    if (chaseListHelper.Count < chaseHelper.Count)
                    {
                        foreach (int index in chaseListHelper)
                        {
                            chaseHelper.Remove(index);
                        }
                    }
                    else
                    {
                        chaseHelper.Clear();
                    }
                    chaseListHelper.Clear();
                }
            }
            bool clearCached = chaseHelper.Count != 0;
            if (!clearCached)
            {
                mode += 10;
                for (int i = 0; i < player.ChaseNodes.Count; i++)
                {
                    chaseHelper.Add(i);
                }
            }
            int oldIndex = chaseIndex;
            chaseIndex = RandomUtils.GetRandom(chaseHelper);
            if (add)
            {
                player.FreeChaseNodes.Add(oldIndex);
            }
            if (chaseIndex < 0 || chaseIndex >= player.ChaseNodes.Count)
            {
                Debug.LogError($"Broken chase index, mode: {mode}, chase index: {chaseIndex}, chase nodes count: {player.ChaseNodes.Count}");
            }
            pos = player.ChaseNodes[chaseIndex];

            chaseIndex = player.TranslateIndex(chaseIndex, false);
            if (clearCached)
            {
                foreach (int index in chaseHelper)
                {
                    player.FreeChaseNodes.Remove(index);
                }
            }
            else
            {
                OccupyChaseNodes();
            }

            result = FindWithSmallOffset(pos);
        }
        else if (sandbox)
        {
            int node = 0;
            if (ignore)
            {
                denied = false;
                node = RandomUtils.GetRandom(randomNodes);
            }
            else
            {
                int tries = 1000;

                Assert.IsFalse(ignore2);
                ignore2 = true;
                denied = true;
                while (denied)
                {
                    if (--tries < 0 || randomNodes.Count == 0)
                    {
                        break;
                    }
                    denied = false;
                    node = RandomUtils.GetRandom(randomNodes);

                    helper.Add(node);
                    randomNodes.Remove(node);

                    tacticManager.RandomNodeDenied += OnRandomNodeDenied;
                    tacticManager.FirePotentialRandomNodeChosen(node, oldNode);
                    tacticManager.RandomNodeDenied -= OnRandomNodeDenied;
                }
                Assert.IsTrue(ignore2);
                ignore2 = false;
                foreach (int index in helper)
                {
                    randomNodes.Add(index);
                }
                helper.Clear();
            }

            if (denied)
            {
                denied = false;
                Debug.LogError("sandbox enemy: " + Id + ", cannot find free node");
            }
            else
            {
                tacticManager.FireRandomNodeChosen(node, true);
            }
            previousRandomNode = node;
            result = FindWithOffset(tacticManager.MapNodes.Find(node).Position);
        }
        else if (distractTarget.HasValue)
        {
            result = FindWithOffset(distractTarget.Value);
        }
        else
        {
            patrolIndex = Mathf.Max(0, patrolIndex);
            PatrolNodeIndex = Mathf.Max(0, PatrolNodeIndex);
            var pos = patrols[patrolIndex].SNodePoses[PatrolNodeIndex].Position;
            alternativePath = !fromChase && alternativePathfinding;
            result = alternativePath ? tacticManager.MapNodes.Find(pos) : FindWithOffset(pos);
        }
        if (originalRandomNodes != null && originalRandomNodes.Count != 0 && oldNode != -1)
        {
            tacticManager.FireRandomNodeChosen(oldNode, false);
        }
        return result;
    }

    private PathNode FindEdgeNodeWithOffset()
    {
        var pos = shipImage.anchoredPosition;

        var up = new Vector2(0f, 540f);
        nodesRetreat[0].Setup(pos, up);
        nodesRetreat[1].Setup(pos, new Vector2(0f, -540f));
        nodesRetreat[2].Setup(pos, new Vector2(960f, 0f));
        nodesRetreat[3].Setup(pos, new Vector2(-960f, 0f));
        nodesRetreat.Sort(nodesRetreat[0]);
        int tries = 0;
        var nodes = tacticManager.MapNodes;
        while (++tries < 10000)
        {
            var result = nodes.Find(nodesRetreat[0].Next());
            if (result.IsOnLand)
            {
                nodesRetreat.Sort(nodesRetreat[0]);
            }
            else
            {
                return result;
            }
        }
        Debug.LogError("Cannot find end node.");
        return nodes.Find(nodesRetreat[0].Setup(pos, up).Next());
    }

    private PathNode FindWithSmallOffset(Vector2 pos)
    {
        float oldOffset = patrolNodeOffset;
        patrolNodeOffset = 5f;
        var result = FindWithOffset(pos);
        patrolNodeOffset = oldOffset;
        return result;
    }

    private PathNode FindWithOffset(Vector2 position)
    {
        if (patrolNodeOffset <= 1f)
        {
            var result = tacticManager.MapNodes.Find(position);
            if (result.IsOnLand)
            {
                Debug.LogError("Enemy with 0 patrol offset on land");
            }
            return result;
        }

        if (nodesHelper == null) 
        {
            nodesHelper = new List<PathNode>();
            patrolNodeOffsetSqr = patrolNodeOffset * patrolNodeOffset;
            scale = Vector2.one / tacticManager.MapNodes.Scale;
            counts = new Vector2Int(Mathf.RoundToInt(patrolNodeOffset * scale.x), Mathf.RoundToInt(patrolNodeOffset * scale.y));
        }
        nodesHelper.Clear();
        var intPosition = tacticManager.MapNodes.GetNodeInt(position);
        intPosition -= counts;
        int startY = intPosition.y;
        foreach (var node in GetNodes(counts, intPosition, startY))
        {
            if (!node.IsOnLand)
            {
                nodesHelper.Add(node);
            }
        }

        if (nodesHelper.Count == 0)
        {
            Debug.LogError($"Cannot find node near {position} on sea(offset: {patrolNodeOffset})");

            nodesHelper.AddRange(GetNodes(counts, intPosition, startY));
        }
        return RandomUtils.GetRandom(nodesHelper);
    }

    private IEnumerable<PathNode> GetNodes(Vector2Int counts, Vector2Int intPosition, int startY)
    {
        for (int i = -counts.x; i <= counts.x; i++, intPosition.x++)
        {
            if (intPosition.x < 0)
            {
                continue;
            }
            if (intPosition.x >= tacticManager.MapNodes.ResX)
            {
                break;
            }
            intPosition.y = startY;
            for (int j = -counts.y; j <= counts.y; j++, intPosition.y++)
            {
                if (intPosition.y < 0)
                {
                    continue;
                }
                if (intPosition.y >= tacticManager.MapNodes.ResY)
                {
                    break;
                }
                if (Vector2.SqrMagnitude(new Vector2(i * scale.x, j * scale.y)) < patrolNodeOffsetSqr)
                {
                    yield return tacticManager.MapNodes.GetPathNode(intPosition);
                }
            }
        }
    }

    private void SetEnemyPosition(Vector2 position)
    {
        shipImage.anchoredPosition = position;
        pos = position;
    }

    private void LookAt()
    {
        if (pathIndex < path.Count)
        {
            LookAt(path[Visible ? pathIndex : path.Count - 1].Position);
        }
    }

    private void LookAt(Vector2 dest)
    {
        shipImage.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, dest - pos));
    }

    private void NewPatrol()
    {
        if (patrolsAvailable.Count == 0)
        {
            for (int i = 0; i < patrols.Count; i++)
            {
                patrolsAvailable.Add(i);
            }
        }
        patrolIndex = RandomUtils.GetRandom(patrolsAvailable);
        patrolsAvailable.Remove(patrolIndex);
    }

    private void OnDetectedChanged(bool detected)
    {
        if (chaseMode && !detected)
        {
            chaseMode = false;
            path.Clear();
            pathIndex = 0;

            //#go to patrol node which wasn't reached
            if (originalRandomNodes == null || originalRandomNodes.Count == 0)
            {
                PatrolNodeIndex--;
            }
            enemyAttacksManager.DetectedChanged -= OnDetectedChanged;
        }
    }

    private void RevealRange()
    {
        MapUnitMaskManager.Instance.AddUnitObject(shipImage.gameObject, (int)revealRange);
    }

    private void FindTarget()
    {
        potentialTargets.Clear();
        foreach (var enemy in tacticManager.GetAllShips())
        {
            if (enemy.searchAndDestroy && !enemy.IsDisabled && !enemy.Dead && Side != enemy.Side && enemy.destroyTarget == null)
            {
                Assert.IsFalse(Side == ETacticalObjectSide.Neutral);
                Assert.IsFalse(enemy.Side == ETacticalObjectSide.Neutral);
                potentialTargets.Add(enemy);
            }
        }
        if (potentialTargets.Count > 0)
        {
            destroyTarget = FindTargetInner();
            destroyTarget.destroyTarget = this;
            return;
        }
        foreach (var enemy in tacticManager.GetAllShips())
        {
            if (enemy.searchAndDestroy && !enemy.IsDisabled && !enemy.Dead && Side != enemy.Side)
            {
                potentialTargets.Add(enemy);
            }
        }
        if (potentialTargets.Count > 0)
        {
            destroyTarget = FindTargetInner();
        }
    }

    private TacticalEnemyShip FindTargetInner()
    {
        TacticalEnemyShip result = null;
        float dist = float.PositiveInfinity;
        foreach (var target in potentialTargets)
        {
            float newDist = Vector2.SqrMagnitude(target.shipImage.anchoredPosition - shipImage.anchoredPosition);
            if (newDist < dist)
            {
                dist = newDist;
                result = target;
            }
        }
        return result;
    }

    private void OnRandomNodeChosen(int nodeID, bool taken)
    {
        if (originalRandomNodes.Contains(nodeID))
        {
            if (taken)
            {
                randomNodes.Remove(nodeID);
            }
            else
            {
                randomNodes.Add(nodeID);
            }
        }
    }

    private void OnPotentialRandomNodeChosen(int nodeID, int prevNodeID)
    {
        if (!ignore2 && originalRandomNodes.Contains(nodeID) && randomNodes.Count == 1 && randomNodes.Contains(nodeID) && !originalRandomNodes.Contains(prevNodeID))
        {
            tacticManager.FireRandomNodeDenied();
        }
    }

    private void OnRandomNodeDenied()
    {
        tacticManager.RandomNodeDenied -= OnRandomNodeDenied;
        denied = true;
    }

    private void OccupyChaseNodes()
    {
        var player = tacticManager.Carrier;
        int index = player.TranslateIndex(chaseIndex, true);
        if (index == -1)
        {
            return;
        }

        var pos = player.ChaseNodes[index];
        float sqr = chaseRange * chaseRange;
        for (int i = 0; i < player.ChaseNodes.Count; i++)
        {
            if (Vector2.SqrMagnitude(pos - player.ChaseNodes[i]) < sqr)
            {
                player.FreeChaseNodes.Remove(i);
            }
        }
    }

    private int ClearChaseIndices()
    {
        var player = tacticManager.Carrier;
        int result = player.TranslateIndex(chaseIndex, true);
        if (result > -1)
        {
            var pos = player.ChaseNodes[result];
            float sqr = chaseRange * chaseRange;
            for (int i = 0; i < player.ChaseNodes.Count; i++)
            {
                if (Vector2.SqrMagnitude(pos - player.ChaseNodes[i]) < sqr)
                {
                    player.FreeChaseNodes.Add(i);
                }
            }
        }
        return result;
    }

    private void OnChaseNodesChanged()
    {
        if (chaseMode && !GameSceneManager.Instance.IsLoading && path.Count > 0)
        {
            var player = tacticManager.Carrier;
            if (chaseIndex == -1)
            {
                if (Vector2.SqrMagnitude(chasePlayerPos - player.Position) < 2500f)
                {
                    return;
                }
            }

            if (Vector2.SqrMagnitude(pos - path[path.Count - 1].Position) > 400f)
            {
                chaseIndex = -1;
                chasePlayerPos = player.Position;
                return;
            }

            pathIndex = 0;
            int prev = chaseIndex;
            if (chaseIndex != -1)
            {
                chaseIndex = player.TranslateIndex(chaseIndex, true);
            }
            if (chaseIndex == -1)
            {
                path = FindPath(false, false);
            }
            else
            {
                if (player.ChaseNodes.Count <= chaseIndex || chaseIndex < 0)
                {
                    Debug.Log($"fatal chase index: {chaseIndex}; prev chase index: {prev}; current chase nodes count: {player.ChaseNodes.Count}");
                }
                path = FindPath(FindWithSmallOffset(player.ChaseNodes[chaseIndex]));
                chaseIndex = player.TranslateIndex(chaseIndex, false);
            }
            DrawPath();
            if (path.Count == 0)
            {
                Debug.LogError("Empty path");
            }
        }
    }

    private void OnNodesChanged()
    {
        if (alreadyRetreats)
        {
            pathIndex = 0;
            path = FindPath(false, false);
            DrawPath();
            if (path.Count == 0)
            {
                Debug.LogError("Empty path");
            }
            return;
        }
        if (searchAndDestroy || chaseMode || (originalRandomNodes != null && originalRandomNodes.Count != 0))
        {
            return;
        }
        path = FindPath(false, chaseMode);
        DrawPath();
        if (path.Count == 0)
        {
            Debug.LogError("Empty path");
        }
    }
}
