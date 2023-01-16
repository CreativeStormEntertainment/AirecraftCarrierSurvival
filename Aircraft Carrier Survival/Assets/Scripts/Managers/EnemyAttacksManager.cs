using GambitUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.UI;

using UnityRandom = UnityEngine.Random;

public class EnemyAttacksManager : MonoBehaviour, IPopupPanel, ITickable
{
    public event Action<EAttackResult> AttackFinished = delegate { };
    public event Action<bool> AttackAnimationStateChanged = delegate { };
    public event Action<bool> DetectedChanged = delegate { };
    public event Action EnemyAttackSucceed = delegate { };
    public event Action EnemyAttacked = delegate { };
    public event Action AttackOnUs = delegate { };
    public event Action KindaDetected = delegate { };

    public static EnemyAttacksManager Instance = null;

    private static readonly int FireAnim = Animator.StringToHash("Fire");

    public EWindowType Type => EWindowType.Other;

    public List<EnemyAttackTimerCarrier> ScriptedAttackScenarios => scriptedAttackScenarios;

    public bool SubmarinesBlocked
    {
        get;
        set;
    }

    public bool FriendlyCAPIsMidway
    {
        get;
        set;
    }

    public bool DisableAttacksOnAlly
    {
        get;
        set;
    }

    public bool IsDetected
    {
        get;
        private set;
    }

    public bool AlreadyDetected
    {
        get;
        private set;
    }

    public float RadarHighlightTime = 4f;

    //[SerializeField]
    //private Text pointsText = null;

    [SerializeField]
    private List<Transform> pointsParents = null;

    [SerializeField]
    private List<EnemyAttackTimerCarrier> scriptedAttackScenarios = null;

    [SerializeField]
    private Text defencePointsText = null;

    [SerializeField]
    private int mediumAttackThreshold = 4;

    [SerializeField]
    private int heavyAttackThreshold = 8;

    [SerializeField]
    private List<AnimAttacksPrefabData> attacksAnims = null;

    [SerializeField]
    private List<EnemyAttackAnimationData> enemyAttackAnimationsStrikeGroup = null;

    [SerializeField]
    private List<StartReconAnimData> reconAnims = null;

    [SerializeField]
    private StartReconAnimData submarineAnim = null;

    [SerializeField]
    private int maxAttacksInProgress = 4;

    [SerializeField]
    private List<AAAnimationList> aaAnims = null;

    [SerializeField]
    private Text escortPointsText = null;

    [SerializeField]
    private float undetectionHoursAfterClouds = 2f;

    [SerializeField]
    private DefenceTooltip defenceTooltip = null;

    [SerializeField]
    private float aaTimeToFire = 175f;
    [SerializeField]
    private float crewInTime = .8f;
    [SerializeField]
    private float crewOutTime = .9f;
    [SerializeField]
    private float aaFireTime = 18f;
    [SerializeField]
    private float randomTime = 3f;
    [SerializeField]
    private float timeForSound = 1.5f;
    [SerializeField]
    private float timeToFinishSound = 30f;

    [SerializeField]
    private AASounds aaSounds = null;
    [SerializeField]
    private Transform aaSoundPosOutsideCamera = null;

    [SerializeField]
    private List<GameObject> fullHDBars = null;

    [Header("Generated attacks")]
    [SerializeField]
    private SandboxAdmiralLevels levelsData = null;
    [SerializeField]
    private int startHour = 6;
    [SerializeField]
    private int endHour = 6;
    [SerializeField]
    private int minutesStep = 13;
    [SerializeField]
    private float carrierTargetChance = .75f;
    [SerializeField]
    private float kamikazeChance = .15f;

    [SerializeField]
    private List<DifficultyAttackData> difficultyAttackDatas = null;
    [SerializeField]
    private List<SandboxAttackData> sandboxAttacks = null;

    private List<List<Image>> defencePointsIcons = null;

    private int crewManagerDefencePoints;
    private int defenceRoomDefencePoints = 0;
    private int pilotsDefencePoints;
    private int capDefencePoints;
    private int strikeGroupDefencePoints;
    private int strikeGroupActiveDefencePoints;
    private int islandBuffDefencePoints;
    private int islandBuffEscortPoints;
    private int sectionDebuff;
    private int strikeGroupExtraDefencePoints;
    private int buffBuffPoints;
    private int defencePoints;

    private int crewManagerEscortPoints;
    private int strikeGroupEscortPoints;
    private int capEscortPoints;
    private int strikeGroupExtraEscortPoints;
    private int escortPoints;

    private bool redirectAttack;
    private bool strikeGroupInvulnerable;

    private List<EnemyAttackTimerCarrier> enemyAttacksOnUs;
    private List<EnemyAttackTimerFriend> enemyAttacksOnFriend;
    private EnemySubmarineAttackTimer enemyAttacksSubmarine;
    private EnemyReconAttackTimer enemyAttacksRecon;

    private List<Dictionary<EEnemyAttackPower, List<AnimAttackData>>> animations;
    private List<Dictionary<EEnemyAttackPower, List<AnimAttackData>>> strikeGroupAnimations;
    private HashSet<EnemyAttackData> attacksInProgress;

    private HashSet<EnemyAttackOnUs24h> enemyAttacksOnUsSet;
    private HashSet<EnemyAttackFriend24h> enemyAttacksOnFriendSet;
    private HashSet<EnemySubmarineAttackTimer> enemyAttacksSubmarineSet;
    private HashSet<EnemyReconAttackTimer> enemyAttacksReconSet;

    private SOTacticMap currentMap;

    private bool wasInClouds;
    private int ticksToDetection = -1;

    private Dictionary<int, AttackAnimCameraData> activeRecons;
    private HashSet<int> freeAttacksGroups;
    private Dictionary<EEnemyAttackPower, HashSet<int>> freeAttacks;
    private HashSet<int> freeRecons;
    private HashSet<int> freeReconAnims;

    private ECameraView prevView;

    private bool playingAA;
    private bool pausedAA;

    private Transform aaSoundsTrans;
    private Transform aaSoundsCameraParent;
    private Vector3 aaSoundsPosition;

    private List<EnemyAttackData> savedData;

    private ETemporaryBuff currentBuff;

    private bool casualties;

    private HashSet<int> times;
    private EGameMode mode;
    private HashSet<TacticalEnemyShip> ships;
    private List<TacticalEnemyShip> currentShips;
    private HashSet<TacticalEnemyShip> allies;
    private List<bool> scoutSubHelper;

    private int globalAttacksPowerModifier;

    private int maxSandboxTimes;
    private int maxSandboxTimes2;

    private bool isLoading;

#if ALLOW_CHEATS
    private bool invisible;
#endif

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        int index = 0;
        defencePointsIcons = new List<List<Image>>();
        foreach (Transform parent in pointsParents)
        {
            defencePointsIcons.Add(new List<Image>());
            foreach (Transform defIcon in parent)
            {
                defencePointsIcons[index].Add(defIcon.GetComponent<Image>());
            }
            //defencePointsIcons[index].Reverse();
            foreach (Image i in defencePointsIcons[index])
            {
                i.enabled = false;
            }
            index++;
        }

        for (int i = 1; i < aaAnims.Count; i++)
        {
            aaAnims[i].Anims.AddRange(aaAnims[i - 1].Anims);
        }

        freeAttacksGroups = new HashSet<int>();
        freeAttacks = new Dictionary<EEnemyAttackPower, HashSet<int>>();
        animations = new List<Dictionary<EEnemyAttackPower, List<AnimAttackData>>>();
        SetupAnims(animations, attacksAnims, freeAttacksGroups, freeAttacks);

        enemyAttacksOnUsSet = new HashSet<EnemyAttackOnUs24h>();
        enemyAttacksOnFriendSet = new HashSet<EnemyAttackFriend24h>();
        enemyAttacksSubmarineSet = new HashSet<EnemySubmarineAttackTimer>();
        enemyAttacksReconSet = new HashSet<EnemyReconAttackTimer>();

        attacksInProgress = new HashSet<EnemyAttackData>();

        activeRecons = new Dictionary<int, AttackAnimCameraData>();
        freeRecons = new HashSet<int>();
        freeReconAnims = new HashSet<int>();
        for (int i = 0; i < reconAnims.Count; i++)
        {
            freeRecons.Add(i);
        }
        for (int i = 0; i < reconAnims[0].Directors.Count; i++)
        {
            freeReconAnims.Add(i);
        }

        aaSoundsTrans = aaSounds.transform;
        aaSoundsCameraParent = aaSoundsTrans.parent;
        aaSoundsPosition = aaSoundsTrans.localPosition;

        savedData = new List<EnemyAttackData>();

        sandboxAttacks.Sort((x, y) => Comparer<int>.Default.Compare((int)x.Difficulty, (int)y.Difficulty));
        ships = new HashSet<TacticalEnemyShip>();
        currentShips = new List<TacticalEnemyShip>();
        allies = new HashSet<TacticalEnemyShip>();
        scoutSubHelper = new List<bool>();

        int minutes = (endHour - startHour * 60);
        maxSandboxTimes = minutes / minutesStep;
        maxSandboxTimes2 = (minutes - 20) / minutesStep + 1;
    }

    private void Start()
    {
        //TacticManager.Instance.EnemyRouted += OnEnemyRouted;
        TimeManager.Instance.AddTickable(this);

        CameraManager.Instance.ViewChanged += OnViewChanged;
        WorldMap.Instance.Toggled += OnWorldMapToggled;
    }

    private void Update()
    {
        if (playingAA && pausedAA != (Time.timeScale == 0f))
        {
            pausedAA = !pausedAA;
            aaSounds.SetPause(pausedAA);
        }
    }

    public void LoadStart()
    {
        Assert.IsFalse(isLoading);
        isLoading = true;
    }

    public void LoadData(ref EnemyAttacksSaveData saveData)
    {
        Assert.IsTrue(isLoading);
        isLoading = false;

        DisableAttacksOnAlly = saveData.DisableAttacksOnAlly;

        IsDetected = saveData.Detected;
        AlreadyDetected = saveData.AlreadyDetected;
        ticksToDetection = saveData.DetectionTicks;
        wasInClouds = saveData.CloudsUndetected;

        if (mode == EGameMode.Sandbox)
        {
            enemyAttacksOnUs = new List<EnemyAttackTimerCarrier>();
            var timerOnUs = new EnemyAttackTimerCarrier();
            int i = 0;
            foreach (var pair in LoadDatas(saveData.SandboxAttacks, timerOnUs, i, 0))
            {
                pair.Key.Kamikaze = pair.Value.Target == 1;
                pair.Key.Target = pair.Value.Target < 2 ? EEnemyAttackTarget.Carrier : EEnemyAttackTarget.StrikeGroup;
                pair.Key.AttackPower = pair.Value.AttackPower;
                i++;
            }
            enemyAttacksOnUs.Add(timerOnUs);

            enemyAttacksOnFriend = new List<EnemyAttackTimerFriend>();
            var timerFriend = new EnemyAttackTimerFriend();
            foreach (var _ in LoadDatas(saveData.SandboxAttacks, timerFriend, i, 1))
            {
                i++;
            }
            enemyAttacksOnFriend.Add(timerFriend);

            enemyAttacksRecon = new EnemyReconAttackTimer();
            foreach (var _ in LoadDatas(saveData.SandboxAttacks, enemyAttacksRecon, i, 2))
            {
                i++;
            }

            enemyAttacksSubmarine = new EnemySubmarineAttackTimer();
            foreach (var _ in LoadDatas(saveData.SandboxAttacks, enemyAttacksSubmarine, i, 3))
                ;
        }
        else
        {
            int index = 0;
            Assert.IsTrue(saveData.ChosenPatterns.Count > 3, "b001");
            if (saveData.ChosenPatterns[index] > -1)
            {
                Assert.IsTrue(currentMap.EnemyAttacks.EnemiesAttackOnUs.Count > saveData.ChosenPatterns[index], "b002");
                enemyAttacksOnUs = currentMap.EnemyAttacks.EnemiesAttackOnUs[saveData.ChosenPatterns[index]].EnemyAttacks;
                foreach (var data in enemyAttacksOnUs)
                {
                    data.Setup();
                }
            }
            if (saveData.ChosenPatterns[++index] > -1 && !DisableAttacksOnAlly)
            {
                Assert.IsTrue(currentMap.EnemyAttacks.EnemiesAttacksOnFriends.Count > saveData.ChosenPatterns[index], "b003");
                enemyAttacksOnFriend = currentMap.EnemyAttacks.EnemiesAttacksOnFriends[saveData.ChosenPatterns[index]].EnemyAttacks;
                foreach (var data in enemyAttacksOnFriend)
                {
                    data.Setup();
                }
            }
            if (saveData.ChosenPatterns[++index] > -1)
            {
                Assert.IsTrue(currentMap.EnemyAttacks.EnemiesSubmarinesAttacks.Count > saveData.ChosenPatterns[index], "b004");
                enemyAttacksSubmarine = currentMap.EnemyAttacks.EnemiesSubmarinesAttacks[saveData.ChosenPatterns[index]];
                enemyAttacksSubmarine.Setup();
            }
            if (saveData.ChosenPatterns[++index] > -1)
            {
                Assert.IsTrue(currentMap.EnemyAttacks.EnemiesReconsAttacks.Count > saveData.ChosenPatterns[index], "b005");
                enemyAttacksRecon = currentMap.EnemyAttacks.EnemiesReconsAttacks[saveData.ChosenPatterns[index]];
                enemyAttacksRecon.Setup();
            }
        }

        savedData.Clear();
        var radarMan = RadarManager.Instance;
        foreach (var attackSaveData in saveData.Attacks)
        {
            var attackData = new EnemyAttackData(attackSaveData);
            var attackType = GetEnemyAttackPower(attackData.CalculatedAttackPower);
            var set = freeAttacks[attackType];
            int groupIndex = RandomUtils.GetRandom(freeAttacksGroups);

            Assert.IsTrue(animations.Count > groupIndex, "b006");
            Assert.IsTrue(animations[groupIndex][attackType].Count > attackSaveData.Anim, "b007");
            attackData.AnimData = new AttackAnimCameraData(animations[groupIndex][attackType][attackSaveData.Anim], attackSaveData.Anim);
            StartCoroutine(PlayAnim(freeAttacksGroups, set, groupIndex, attackSaveData.Anim, true, attackData, true, attackSaveData.Time, attackSaveData.AATime));

            if (attackSaveData.RadarTicks > -1)
            {
                radarMan.LoadAttack(attackSaveData.RadarTicks, attackData);
            }
            if (attackSaveData.AlreadyAttackedTime > 0d)
            {
                StartAttackDelayed(attackData, (float)(attackSaveData.AlreadyAttackedTime), true);
            }

            savedData.Add(attackData);
        }

        var tacticMan = TacticManager.Instance;
        foreach (var reconSaveData in saveData.ReconsAnims)
        {
            int dataIndex = RandomUtils.GetRandom(freeRecons);
            freeRecons.Remove(dataIndex);
            freeReconAnims.Remove(reconSaveData.Anim);
            var data = new AttackAnimCameraData(reconAnims, dataIndex, reconSaveData.Anim);
            data.DirectorGo.SetActive(true);
            data.TacticalMission = tacticMan.GetMission(EMissionOrderType.CounterHostileScouts, reconSaveData.Mission);
            activeRecons[dataIndex] = data;

            var enemy = radarMan.LoadRecon(dataIndex, reconSaveData.ZAngle);
            data.TacticalMission.SetRadarObject(enemy);
        }

        radarMan.LoadSubmarines(saveData.SubmarineAttacks);
    }

    public void SaveData(ref EnemyAttacksSaveData data)
    {
        data.DisableAttacksOnAlly = DisableAttacksOnAlly;
        data.AlreadyDetected = AlreadyDetected;
        data.CloudsUndetected = wasInClouds;
        data.Detected = IsDetected;
        data.DetectionTicks = ticksToDetection > 0 ? ticksToDetection : -1;

        if (mode == EGameMode.Sandbox)
        {
            if (data.SandboxAttacks == null)
            {
                data.SandboxAttacks = new List<SandboxAttacksSaveData>();
            }
            data.SandboxAttacks.Clear();
            if (enemyAttacksOnUs.Count == 0)
            {
                return;
            }

            foreach (var pair in SaveDatas(enemyAttacksOnUs[0].Datas, 0))
            {
                var save = pair.Value;
                if (pair.Key.Kamikaze)
                {
                    save.Target = 1;
                }
                else if (pair.Key.Target == EEnemyAttackTarget.Carrier)
                {
                    save.Target = 0;
                }
                else
                {
                    save.Target = 2;
                }
                save.AttackPower = pair.Key.AttackPower;
                data.SandboxAttacks.Add(save);
            }
            foreach (var pair in SaveDatas(enemyAttacksOnFriend[0].Datas, 1))
            {
                data.SandboxAttacks.Add(pair.Value);
            }
            foreach (var pair in SaveDatas(enemyAttacksRecon.Datas, 2))
            {
                data.SandboxAttacks.Add(pair.Value);
            }
            foreach (var pair in SaveDatas(enemyAttacksSubmarine.Datas, 3))
            {
                data.SandboxAttacks.Add(pair.Value);
            }
        }
        else
        {
            data.ChosenPatterns.Clear();

            var attackOnUs = currentMap.EnemyAttacks.EnemiesAttackOnUs;
            data.ChosenPatterns.Add((attackOnUs != null && attackOnUs.Count > 0 && enemyAttacksOnUs != null) ? attackOnUs.FindIndex((x) => x.EnemyAttacks == enemyAttacksOnUs) : -1);
            var attackOnFriends = currentMap.EnemyAttacks.EnemiesAttacksOnFriends;
            data.ChosenPatterns.Add((attackOnFriends != null && attackOnFriends.Count > 0 && attackOnFriends != null) ? attackOnFriends.FindIndex((x) => x.EnemyAttacks == enemyAttacksOnFriend) : -1);
            var attackSubs = currentMap.EnemyAttacks.EnemiesSubmarinesAttacks;
            data.ChosenPatterns.Add((attackSubs != null && attackSubs.Count > 0 && enemyAttacksSubmarine != null) ? attackSubs.IndexOf(enemyAttacksSubmarine) : -1);
            var attackRecons = currentMap.EnemyAttacks.EnemiesReconsAttacks;
            data.ChosenPatterns.Add((attackRecons != null && attackRecons.Count > 0 && enemyAttacksRecon != null) ? attackRecons.IndexOf(enemyAttacksRecon) : -1);
        }

        data.Attacks.Clear();
        var radarMan = RadarManager.Instance;
        foreach (var attackData in attacksInProgress)
        {
            var attackSaveData = new AttackSaveData();
            attackData.SaveData(ref attackSaveData);
            if (attackSaveData.AlreadyAttackedTime > 0d)
            {
                attackSaveData.AlreadyAttackedTime = 10d - (attackSaveData.Time - attackSaveData.AlreadyAttackedTime);
                attackSaveData.RadarTicks = -1;
            }
            else
            {
                attackSaveData.RadarTicks = (int)radarMan.GetEnemy(attackData).TickTimer;
            }
            data.Attacks.Add(attackSaveData);
        }

        data.ReconsAnims.Clear();
        var tacticMan = TacticManager.Instance;
        foreach (var pair in activeRecons)
        {
            var reconSaveData = new ReconSaveData();
            reconSaveData.Anim = pair.Value.AnimIndex;
            reconSaveData.ZAngle = radarMan.GetReconAngle(pair.Key);
            reconSaveData.Mission = tacticMan.IndexOf(pair.Value.TacticalMission);
            data.ReconsAnims.Add(reconSaveData);
        }

        radarMan.SaveSubmarines(data.SubmarineAttacks);
    }

    public void AttackCamera(EnemyAttackData data)
    {
        if (data.AnimData.Director.time >= 175d)
        {
            return;
        }

        SetSubmarineCamera(false);

        var hudMan = HudManager.Instance;
        hudMan.SetBlockSpeed(2);
        //submarineTransCamera.gameObject.SetActive(false);
        foreach (var attackData in attacksInProgress)
        {
            attackData.AnimData.CameraGo.SetActive(false);
        }
        foreach (var reconData in reconAnims)
        {
            reconData.Camera.SetActive(false);
        }
        hudMan.HideTacticMap();

        data.AnimData.CameraGo.SetActive(true);
        Assert.IsNotNull(data, "AttackCamera Data");
        Assert.IsNotNull(data.AnimData, "AttackCamera AnimData");
        Assert.IsNotNull(data.AnimData.Emitter, "AttackCamera Emitter");
        Assert.IsNotNull(data.AnimData.Director, "AttackCamera Director");

        if (!FMODStudio.IsPlaying(data.AnimData.Emitter.EventInstance))
        {
            data.AnimData.Emitter.Play();
        }
        FMODStudio.SetTimelinePosition(data.AnimData.Emitter.EventInstance, (int)(data.AnimData.Director.time * 1500d));

        ForceSwitchCamera();
        CameraManager.Instance.FixAttackCamera = true;
    }

    public void ReconCamera(int reconIndex)
    {
        SetSubmarineCamera(false);

        var hudMan = HudManager.Instance;
        hudMan.SetBlockSpeed(2);
        foreach (var attackData in attacksInProgress)
        {
            attackData.AnimData.CameraGo.SetActive(false);
        }
        //CameraManager.Instance.SwitchToPreviewAttack(reconTransCamera);
        foreach (var data in reconAnims)
        {
            data.Camera.SetActive(false);
        }
        hudMan.HideTacticMap();

        var reconData = activeRecons[reconIndex];
        reconData.CameraGo.SetActive(true);
        if (!FMODStudio.IsPlaying(reconData.Emitter.EventInstance))
        {
            reconData.Emitter.Play();
        }
        FMODStudio.SetTimelinePosition(reconData.Emitter.EventInstance, (int)(reconData.Director.time * 500d));

        ForceSwitchCamera();
        CameraManager.Instance.FixAttackCamera = false;
    }

    public void SubmarineCamera()
    {
        SetSubmarineCamera(true);

        var hudMan = HudManager.Instance;
        hudMan.SetBlockSpeed(2);
        foreach (var attackData in attacksInProgress)
        {
            attackData.AnimData.CameraGo.SetActive(false);
        }
        foreach (var data in reconAnims)
        {
            data.Camera.SetActive(false);
        }
        hudMan.HideTacticMap();

        float randTime = UnityRandom.Range(0f, (float)submarineAnim.Directors[0].duration);

        FMODStudio.SetTimelinePosition(submarineAnim.Emitters[0].EventInstance, (int)(randTime * 500f));
        submarineAnim.Directors[0].time = randTime;

        ForceSwitchCamera();
        CameraManager.Instance.FixAttackCamera = false;
    }

    public EEnemyAttackPower GetEnemyAttackPower(int attackPower)
    {
        if (attackPower < mediumAttackThreshold)
        {
            return EEnemyAttackPower.Small;
        }
        else if (attackPower < heavyAttackThreshold)
        {
            return EEnemyAttackPower.Medium;
        }
        else
        {
            return EEnemyAttackPower.Heavy;
        }
    }

    public void Setup(ETemporaryBuff buff, EGameMode mode)
    {
        currentBuff = buff;
        this.mode = mode;
        if (mode == EGameMode.Sandbox)
        {
            if (times == null)
            {
                times = new HashSet<int>();
            }
        }
    }

    //todo world map
    public void SetupEnemyAttacks(SOTacticMap map)
    {
        AlreadyDetected = false;
        ResetDetection();

        currentMap = map;

        casualties = map == null || map.Overrides == null || !map.Overrides.EnableNoAACasualties;

        globalAttacksPowerModifier = map.EnemyAttackPowerModifier;

        var param = Parameters.Instance.DifficultyParams;
        if (param.IgnoreGlobalAttacksModifier)
        {
            globalAttacksPowerModifier = 0;
        }
        globalAttacksPowerModifier += param.EnemyAirstrikePowerModifier;

        if (mode == EGameMode.Sandbox)
        {
            ships.Clear();
            allies.Clear();
            foreach (var ship in TacticManager.Instance.GetAllShips())
            {
                ships.Add(ship);
                if (ship.Side == ETacticalObjectSide.Friendly)
                {
                    allies.Add(ship);
                }
            }
        }

        var timeMan = TimeManager.Instance;
        timeMan.DateChanged -= OnDateChanged;
        timeMan.DateChanged += OnDateChanged;
        GetNextRandomAttacks(true);
    }

    public void GetNextRandomAttacks(bool init)
    {
        if (mode == EGameMode.Sandbox)
        {
            CreateSandboxAttacks(init);
            return;
        }

        var attackOnUs = currentMap.EnemyAttacks.EnemiesAttackOnUs;
        if (attackOnUs != null && attackOnUs.Count > 0)
        {
            enemyAttacksOnUs = ChooseRemoveRandom(enemyAttacksOnUsSet, attackOnUs, "enemy attacks on us").EnemyAttacks;
            foreach (var data in enemyAttacksOnUs)
            {
                data.Setup();
            }
        }

        var attackOnFriends = currentMap.EnemyAttacks.EnemiesAttacksOnFriends;
        if (attackOnFriends != null && attackOnFriends.Count > 0)
        {
            enemyAttacksOnFriend = ChooseRemoveRandom(enemyAttacksOnFriendSet, attackOnFriends, "enemy attacks on friends").EnemyAttacks;
            foreach (var data in enemyAttacksOnFriend)
            {
                data.Setup();
            }
        }

        var attackSubs = currentMap.EnemyAttacks.EnemiesSubmarinesAttacks;
        if (attackSubs != null && attackSubs.Count > 0)
        {
            enemyAttacksSubmarine = ChooseRemoveRandom(enemyAttacksSubmarineSet, attackSubs, "submarine attacks");
            enemyAttacksSubmarine.Setup();
        }

        var attackRecons = currentMap.EnemyAttacks.EnemiesReconsAttacks;
        if (attackRecons != null && attackRecons.Count > 0)
        {
            enemyAttacksRecon = ChooseRemoveRandom(enemyAttacksReconSet, attackRecons, "enemy recons");
            enemyAttacksRecon.Setup();
        }
    }

    public void CreateEnemyAttack(EnemyAttackData data, int enemyID, bool detected, bool inRange)
    {
        Assert.IsFalse(HudManager.Instance.InWorldMap);

        var tacMan = TacticManager.Instance;
        switch (data.Type)
        {
            case EEnemyAttackType.Raid:
                if (detected && inRange)
                {
                    if (attacksInProgress.Count >= maxAttacksInProgress)
                    {
                        return;
                    }
                    data.CalculatedAttackPower = data.AttackPower;
                    if (currentBuff == ETemporaryBuff.WeakerEnemiesAttacks)
                    {
                        data.CalculatedAttackPower--;
                    }
                    data.CalculatedAttackPower += globalAttacksPowerModifier;
                    data.CalculatedAttackPower = Mathf.Clamp(data.CalculatedAttackPower, 1, 999);
                    var attackType = GetEnemyAttackPower(data.CalculatedAttackPower);
                    data.CurrentTarget = CheckStrikeGroupTarget(data.CurrentTarget, redirectAttack);
                    var set = freeAttacks[attackType];
                    if (!GetAnim(freeAttacksGroups, set, out int groupIndex, out int animIndex))
                    {
                        return;
                    }
                    data.AnimData = new AttackAnimCameraData(animations[groupIndex][attackType][animIndex], animIndex);
                    StartCoroutine(PlayAnim(freeAttacksGroups, set, groupIndex, animIndex, true, data, true));
                    //GetAnim(data.AttackPower, (data.CurrentTarget == EEnemyAttackTarget.Carrier ? animations : strikeGroupAnimations), out var anims, out var anim);

                    var enemyPosition = tacMan.GetShip(enemyID).RectTransform.anchoredPosition;
                    data.Direction = enemyPosition - tacMan.Carrier.Rect.anchoredPosition;
                    data.Direction.Normalize();
                    RadarManager.Instance.SpawnEnemy(data);
                }
                break;
            case EEnemyAttackType.Scout:
                if (activeRecons.Count < reconAnims.Count)
                {
                    tacMan.AddNewMission(EMissionOrderType.CounterHostileScouts);
                }
                break;
            case EEnemyAttackType.Submarine:
                if (detected && !SubmarinesBlocked)
                {
                    tacMan.AddNewMission(EMissionOrderType.SubmarineHunt, data);
                }
                break;
        }
    }

    public void StartAttack(EnemyAttackData attackData)
    {
        Assert.IsFalse(HudManager.Instance.InWorldMap);
        Assert.IsTrue(attackData.AnimData.Director.state == PlayState.Playing);

        attackData.DirectorTimeAttack = attackData.AnimData.Director.time;
        StartCoroutine(StartAttackDelayed(attackData, 10f, false));
    }

    public void StartSubmarineAttack(EnemyAttackData attackData)
    {
        attackData.CurrentTarget = CheckStrikeGroupTarget(attackData.CurrentTarget, redirectAttack);
        if (attackData.CurrentTarget == EEnemyAttackTarget.Carrier)
        {
            AttackCarrier(1, 0, false);
        }
        else
        {
            AttackStrikeGroup(attackData.CurrentTarget, 1);
        }
    }

    public void CreateFriendlyAttack(EnemyAttackFriendData data, int enemyID)
    {
        var tacMan = TacticManager.Instance;
        var friend = tacMan.GetShip(data.FriendID);
        if (friend.Dead || friend.IsDisabled)
        {
            return;
        }
        var enemy = tacMan.GetShip(enemyID);
        if (enemy.CanAttack && tacMan.IsFriendInRange(friend, enemy))
        {
            tacMan.AddNewMission(FriendlyCAPIsMidway ? EMissionOrderType.FriendlyCAPMidway : EMissionOrderType.FriendlyFleetCAP, data);
            return;
        }
    }

    public void StartFriendlyAttack(EnemyAttackFriendData data, int power)
    {
        var tacMan = TacticManager.Instance;
        var friend = tacMan.GetShip(data.FriendID);
        if (power <= 0)
        {
            Assert.IsFalse(friend.Dead || friend.IsDisabled);
            foreach (var item in friend.Blocks)
            {
                if (!item.Dead)
                {
                    item.Dead = true;
                    tacMan.FireBlockDestroyed(item.Data, false);
                    break;
                }
            }

            friend.CheckIsDead(true);
        }
    }

    public void SetRedirectAttack(bool set)
    {
        redirectAttack = set;
    }

    public void SetStrikeGroupInvulnerable(bool invulnerable)
    {
        strikeGroupInvulnerable = invulnerable;
    }

    public void SetCrewManagerDefencePoints(int points)
    {
        crewManagerDefencePoints = points;
        UpdateDefencePoints();
    }

    public void SetDefenceRoomDefencePoints(int points)
    {
        defenceRoomDefencePoints = points;
        UpdateDefencePoints();
    }
    public void SetPilotsDefencePoints(int points)
    {
        pilotsDefencePoints = points;
        UpdateDefencePoints();
    }

    public void SetCAPDefencePoints(int points)
    {
        capDefencePoints = points;
        UpdateDefencePoints();
    }

    public void SetStrikeGroupDefencePoints(int points)
    {
        strikeGroupDefencePoints = points;
        UpdateDefencePoints();
    }

    public void SetIslandBuffDefencePoints(int points)
    {
        islandBuffDefencePoints += points;
        UpdateDefencePoints();
    }

    public void SetSectionDefencePoints(int points)
    {
        sectionDebuff = points;
        UpdateDefencePoints();
    }

    public void SetStrikeGroupExtraDefencePoints(int points)
    {
        strikeGroupExtraDefencePoints += points;
        UpdateDefencePoints();
    }

    public void SetCrewManagerEscortPoints(int points)
    {
        crewManagerEscortPoints = points;
        UpdateEscortPoints();
    }

    public void SetStrikeGroupEscortPoints(int points)
    {
        strikeGroupEscortPoints = points;
        UpdateEscortPoints();
    }

    public void SetIslandBuffEscortPoints(int points)
    {
        islandBuffEscortPoints += points;
        UpdateEscortPoints();
    }

    public void SetCapEscortPoints(int points)
    {
        capEscortPoints = points;
        UpdateEscortPoints();
    }

    public void SetStrikeGroupExtraEscortPoints(int points)
    {
        strikeGroupExtraEscortPoints += points;
        UpdateEscortPoints();
    }

    public void Detect()
    {
#if ALLOW_CHEATS
        if (invisible)
        {
            return;
        }
#endif
        if (IsDetected)
        {
            return;
        }
        if (wasInClouds)
        {
            AlreadyDetected = true;
            KindaDetected();
        }
        else
        {
            IsDetected = true;
            KindaDetected();
            DetectedChanged(true);
        }
    }

    public void Undetect(int hours = 2)
    {
        if (!IsDetected)
        {
            return;
        }

        ticksToDetection = hours * TimeManager.Instance.TicksForHour;
        IsDetected = false;
        DetectedChanged(false);
    }

    public RadarEnemy SpawnScout(TacticalMission mission)
    {
        var radarMan = RadarManager.Instance;

        int dataIndex = RandomUtils.GetRandom(freeRecons);
        freeRecons.Remove(dataIndex);
        int animIndex = RandomUtils.GetRandom(freeReconAnims);
        freeReconAnims.Remove(animIndex);
        var data = new AttackAnimCameraData(reconAnims, dataIndex, animIndex);
        data.DirectorGo.SetActive(true);
        data.TacticalMission = mission;
        activeRecons[dataIndex] = data;

        var enemy = radarMan.SpawnScout(dataIndex);
        return enemy;
    }

    public void SpawnSubmarine(EnemyAttackData data, TacticalMission mission)
    {
        var radarMan = RadarManager.Instance;
        if (!radarMan.HasSubmarine())
        {
            //reconAnim.gameObject.SetActive(false);
            //reconAnim.gameObject.SetActive(true);
        }
        radarMan.SpawnSubmarine(data, mission);
    }

    public void DespawnScout(RadarEnemy enemy)
    {
        var radarMan = RadarManager.Instance;
        radarMan.RemoveScout(enemy);
        var data = activeRecons[enemy.ReconIndex];
        if (data.CameraGo.activeSelf)
        {
            HudManager.Instance.UnsetBlockSpeed();
            data.CameraGo.SetActive(false);
            Hide();
        }
        data.DirectorGo.SetActive(false);
        reconAnims[data.DataIndex].Root.SetActive(false);

        freeRecons.Add(data.DataIndex);
        freeReconAnims.Add(data.AnimIndex);
        activeRecons.Remove(enemy.ReconIndex);
    }

    public void DespawnSubmarine(bool attackStrikeGroup)
    {
        var radarMan = RadarManager.Instance;
        radarMan.RemoveSubmarine(attackStrikeGroup);
        if (!radarMan.HasSubmarine() && submarineAnim.Camera.activeSelf)
        {
            SetSubmarineCamera(false);
            Hide();
        }
    }

    public void Hide()
    {
        CameraManager.Instance.SwitchMode(prevView);
    }

    public void Tick()
    {
        if (--ticksToDetection == 0)
        {
            Detect();
        }
        if (!HudManager.Instance.InWorldMap)
        {
            if (enemyAttacksOnUs == null)
            {
                Debug.LogError("enemyAttacks is Null.");
                return;
            }
            var tacMan = TacticManager.Instance;

            bool inClouds = tacMan.Carrier.CalculateShipVisibility();
            if (!wasInClouds && inClouds)
            {
                TacticalMapClouds.Instance.SetWeather(true);
                ResetDetection();
            }
            if (wasInClouds && !inClouds)
            {
                TacticalMapClouds.Instance.SetWeather(false);
                if (AlreadyDetected)
                {
                    IsDetected = true;
                    Undetect();
                }
            }
            wasInClouds = inClouds;

            if (mode == EGameMode.Sandbox)
            {
                currentShips.Clear();
                foreach (var ship in ships)
                {
                    if (ship.Side == ETacticalObjectSide.Enemy && !ship.Dead && !ship.IsDisabled && ship.CanAttack && tacMan.IsCarrierInRange(ship))
                    {
                        currentShips.Add(ship);
                    }
                }
                if (currentShips.Count > 0)
                {
                    enemyAttacksOnUs[0].Tick(RandomUtils.GetRandom(currentShips).Id, IsDetected, true, true);
                }
                else if (enemyAttacksOnUs.Count > 0)
                {
                    enemyAttacksOnUs[0].Tick(0, IsDetected, false, false);
                }

                if (enemyAttacksOnFriend != null && enemyAttacksOnFriend.Count > 0)
                {
                    var data = enemyAttacksOnFriend[0].Tick(0, true, true, false);
                    if (data != null)
                    {
                        currentShips.Clear();
                        foreach (var ally in allies)
                        {
                            if (ally.Side != ETacticalObjectSide.Friendly || ally.Dead || ally.IsDisabled)
                            {
                                continue;
                            }
                            foreach (var ship in ships)
                            {
                                if (ship.Side != ETacticalObjectSide.Enemy || ship.Dead || ship.IsDisabled || !ship.CanAttack || !tacMan.IsFriendInRange(ally, ship))
                                {
                                    continue;
                                }
                                currentShips.Add(ally);
                                break;
                            }
                        }
                        if (currentShips.Count > 0)
                        {
                            data.FriendID = RandomUtils.GetRandom(currentShips).Id;
                            currentShips.Clear();
                            var allyShip = tacMan.GetShip(data.FriendID);
                            foreach (var ship in ships)
                            {
                                if (ship.Side != ETacticalObjectSide.Enemy || ship.Dead || ship.IsDisabled || !ship.CanAttack || !tacMan.IsFriendInRange(allyShip, ship))
                                {
                                    continue;
                                }
                                currentShips.Add(ship);
                                break;
                            }
                            if (currentShips.Count > 0)
                            {
                                enemyAttacksOnFriend[0].Do(data, RandomUtils.GetRandom(currentShips).Id, IsDetected, true);
                            }
                        }
                        else
                        {
                            Debug.Log("No friend in range for attack");
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                foreach (var enemy in tacMan.GetAllShips())
                {
                    if (!enemy.Dead)
                    {
                        bool canAttack = enemy.CanAttack && !enemy.IsDisabled;
                        if (enemyAttacksOnUs.Count > i)
                        {
                            bool inRange = tacMan.IsCarrierInRange(enemy);
                            enemyAttacksOnUs[i].Tick(i, IsDetected, inRange, canAttack);
                        }
                        if (!DisableAttacksOnAlly && enemyAttacksOnFriend != null && enemyAttacksOnFriend.Count > i)
                        {
                            enemyAttacksOnFriend[i].Tick(i, true, true, canAttack);
                        }
                    }
                    i++;
                }
            }

            if (enemyAttacksSubmarine != null)
            {
                enemyAttacksSubmarine.Tick(-1, IsDetected, true, true);
            }

            if (enemyAttacksRecon != null)
            {
                enemyAttacksRecon.Tick(-1, IsDetected, true, true);
            }
        }
    }

    public RadarEnemyData GetEnemy(int index)
    {
        return RadarManager.Instance.GetEnemy(savedData[index]);
    }

    public int IndexOf(RadarEnemyData data)
    {
        int index = 0;
        var enemy = RadarManager.Instance.GetEnemy(data);
        foreach (var attack in attacksInProgress)
        {
            if (attack == enemy)
            {
                return index;
            }
            index++;
        }
        Assert.IsFalse(index == -1);
        return -1;
    }

    public void InstantAttack(int forceOverDefense, int absoluteForce)
    {
        AttackCarrier(Mathf.Max(forceOverDefense + globalAttacksPowerModifier, 1), absoluteForce, true);
    }

    public void MakeAttack(int attackPower, bool relative, bool strikeGroup, int hours, EEnemyAttackPower attackType = EEnemyAttackPower.Submarine, int attackAnim = -1)
    {
        if (relative)
        {
            attackPower += strikeGroup ? escortPoints : defencePoints;
        }
        var data = new EnemyAttackData(EEnemyAttackType.Raid);
        data.CalculatedAttackPower = Mathf.Max(attackPower + globalAttacksPowerModifier, 1);
        if (attackType == EEnemyAttackPower.Submarine)
        {
            attackType = GetEnemyAttackPower(data.CalculatedAttackPower);
        }
        data.CurrentTarget = CheckStrikeGroupTarget(strikeGroup ? EEnemyAttackTarget.StrikeGroup : EEnemyAttackTarget.Carrier, redirectAttack);
        var set = freeAttacks[attackType];
        if (!GetAnim(freeAttacksGroups, set, out int groupIndex, out int animIndex))
        {
            return;
        }
        if (attackAnim != -1)
        {
            animIndex = attackAnim;
        }
        data.AnimData = new AttackAnimCameraData(animations[groupIndex][attackType][animIndex], animIndex);

        int timeToAttack = hours * TimeManager.Instance.TicksForHour;
        StartCoroutine(PlayAnim(freeAttacksGroups, set, groupIndex, animIndex, true, data, true, 180d - timeToAttack / 3d));
        //GetAnim(data.AttackPower, (data.CurrentTarget == EEnemyAttackTarget.Carrier ? animations : strikeGroupAnimations), out var anims, out var anim);

        data.Direction = Vector2.up;
        RadarManager.Instance.SpawnEnemy(data, timeToAttack);
    }
#if ALLOW_CHEATS
    public void ToggleInvisible()
    {
        invisible = !invisible;
        Debug.Log(invisible ? "undetectable" : "detectable");
    }
#endif

    private void ResetDetection()
    {
        IsDetected = true;
        Undetect(-1);
    }

    private IEnumerator StartAttackDelayed(EnemyAttackData data, float time, bool load)
    {
        if (time > 0f)
        {
            yield return new WaitForSeconds(time);
        }

        data.CurrentTarget = CheckStrikeGroupTarget(data.CurrentTarget, false);
        int value = data.CalculatedAttackPower;
        if (data.CurrentTarget == EEnemyAttackTarget.Carrier)
        {
            value -= defencePoints;
            Debug.Log($"Enemy power: {data.CalculatedAttackPower}, our defence: {defencePoints}, result: {value}");
            AttackCarrier(value, data.CalculatedAttackPower, false);
            if (value > 0)
            {
                EnemyAttackSucceed();
            }
        }
        else
        {
            value -= escortPoints;
            Debug.Log($"Enemy power: {data.CalculatedAttackPower}, our defence: {escortPoints}, result: {value}");
            AttackStrikeGroup(data.CurrentTarget, value);
        }
        if (value > 0 && data.Kamikaze)
        {
            AircraftCarrierDeckManager.Instance.SetupKamikaze();
        }
        if (!load)
        {
            AttackOnUs();
            CrewManager.Instance.StartAACooldown();
            bool showReport = true;
            foreach (var missions in TacticManager.Instance.Missions.Values)
            {
                foreach (var mission in missions)
                {
                    if (mission.OrderType == EMissionOrderType.CarriersCAP && mission.IsActive)
                    {
                        mission.HadAttack = true;
                        mission.FinishMission(showReport);
                        showReport = false;
                        break;
                    }
                }
            }
        }
    }

    private void AttackCarrier(int count, int absolutePower, bool instant)
    {
        this.StartCoroutineActionAfterFrames(() =>
            {
                var stateMan = GameStateManager.Instance;
                if (stateMan != null && !stateMan.AlreadyShown)
                {
                    EnemyAttacked();
                }
            }, 3);
        float value = UnityRandom.value;
        var sectionMan = SectionRoomManager.Instance;
        var aa = sectionMan.AAGuns;
        int injuredCount = 0;
        if (absolutePower > 0 && casualties)
        {
            if (absolutePower < 7)
            {
                if (value >= .75f)
                {
                    injuredCount = 1;
                }
            }
            else if (absolutePower < 10)
            {
                if (value >= .5f)
                {
                    injuredCount = value >= .75f ? 2 : 1;
                }
            }
            else
            {
                if (value >= .25f)
                {
                    if (value >= .5f)
                    {
                        injuredCount = value >= .75f ? 3 : 2;
                    }
                    else
                    {
                        injuredCount = 1;
                    }
                }
            }
        }

        int segmentCount = 0;
        foreach (var segment in aa.GetAllSegments(true))
        {
            if (!segment.Parent.IsBroken && !segment.Fire.Exists && !segment.IsFlooding() && !segment.HasInjured)
            {
                segmentCount++;
            }
        }

        using (var enumer = CrewManager.Instance.GetHealthyAACrew().GetEnumerator())
        {
            if (enumer.MoveNext())
            {
                var crewStatusMan = CrewStatusManager.Instance;
                for (int i = 0; i < injuredCount; i++)
                {
                    if (segmentCount > 0)
                    {
                        int rand = UnityRandom.Range(0, segmentCount--);
                        foreach (var segment in aa.GetAllSegments(false))
                        {
                            if (--rand < 0)
                            {
                                segment.MakeInjured(EWaypointTaskType.Rescue, enumer.Current, instant);
                                if (!enumer.MoveNext())
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        crewStatusMan.AddInjured(enumer.Current);
                        if (!enumer.MoveNext())
                        {
                            break;
                        }
                    }
                }
            }
        }

        if (count <= 0)
        {
            return;
        }

        foreach (var subsection in sectionMan.DestroySubsectionDamageSegment(false))
        {
            if (--count < 1)
            {
                break;
            }
        }
    }

    private void AttackStrikeGroup(EEnemyAttackTarget target, int count)
    {
        //todo
        if (!strikeGroupInvulnerable && count > 0)
        {
#if ALLOW_CHEATS
            if (HudManager.Instance.Invincibility)
            {
                return;
            }
#endif
            StrikeGroupManager.Instance.DamageRandom(count);
        }
    }

    private bool GetAnim(HashSet<int> freeAnimGroups, HashSet<int> anims, out int groupIndex, out int animIndex)
    {
        groupIndex = -1;
        animIndex = -1;
        if (freeAnimGroups.Count == 0 || anims.Count == 0)
        {
            return false;
        }
        groupIndex = RandomUtils.GetRandom(freeAnimGroups);
        animIndex = RandomUtils.GetRandom(anims);
        return true;
    }

    private IEnumerator PlayAnim(HashSet<int> freeAttacks, HashSet<int> animSet, int groupIndex, int animIndex, bool aa, EnemyAttackData allData, bool scale, double time = 0d, List<float> aaTime = null)
    {
        freeAttacks.Remove(groupIndex);
        animSet.Remove(animIndex);

        var animData = allData.AnimData;

        animData.DirectorGo.SetActive(false);
        animData.DirectorGo.SetActive(true);
        animData.CameraGo.SetActive(false);

        attacksInProgress.Add(allData);

        var director = animData.Director;
        if (Parameters.Instance.DifficultyParams.QuickerAttacks && time < 1d)
        {
            time += 90d;
        }
        director.time = time;
        while (director.time < 120d)
        {
            director.time += Time.deltaTime / 3d;
            director.playableGraph.Evaluate();
            yield return null;
        }

        while (director.time < 165d)
        {
            director.time += Time.deltaTime / 3d;
            director.playableGraph.Evaluate();
            yield return null;
        }

        while (director.time < 175d)
        {
            director.time += Time.deltaTime / 3d;
            director.playableGraph.Evaluate();
            yield return null;
        }

        if (animData.CameraGo.activeSelf)
        {
            HudManager.Instance.UnsetBlockSpeed();
            Hide();
            animData.CameraGo.SetActive(false);
        }

        var secDirector = animData.DirectorSecondary;
        secDirector.gameObject.SetActive(true);
        secDirector.time = director.time - 175d;

        if (aa)
        {
            float minAADelay = randomTime;
            float maxAADelay = 0f;
            float startDelay = Mathf.Max(0f, aaTimeToFire + animData.AADelay);

            var saveData = SaveManager.Instance.Data;
            int aaLevel = BinUtils.ExtractData(saveData.IntermissionData.CarriersUpgrades, 3, (int)saveData.SelectedAircraftCarrier + (int)ECarrierType.Count);
            aaLevel = Mathf.Min(aaLevel, aaAnims.Count - 1);
            var aaAnim = aaAnims[aaLevel];
            if (aaTime == null)
            {
                allData.AATime = new List<float>();
                foreach (var aaAnimData in aaAnim.Anims)
                {
                    float variance = UnityRandom.value * randomTime;
                    allData.AATime.Add(variance);
                    StartCoroutine(AAAnim(aaAnimData, startDelay + variance));
                    if (minAADelay > variance)
                    {
                        minAADelay = variance;
                    }
                    if (maxAADelay < variance)
                    {
                        maxAADelay = variance;
                    }
                }
            }
            else
            {
                startDelay -= (float)secDirector.time;
                for (int i = 0; i < aaAnim.Anims.Count; i++)
                {
                    StartCoroutine(AAAnimLoad(aaAnim.Anims[i], startDelay + aaTime[i]));
                    if (minAADelay > aaTime[i])
                    {
                        minAADelay = aaTime[i];
                    }
                    if (maxAADelay < aaTime[i])
                    {
                        maxAADelay = aaTime[i];
                    }
                }
            }
            maxAADelay -= minAADelay;
            minAADelay += startDelay + timeForSound;
            maxAADelay += timeToFinishSound;
            StartCoroutine(AASound((EAASound)aaLevel, minAADelay, maxAADelay));
        }

        while (true)
        {
            bool updated1 = UpdateDirector(director, Time.deltaTime);
            bool updated2 = UpdateDirector(secDirector, Time.deltaTime);
            if (!updated1)
            {
                animData.Emitter.Stop();
                director.gameObject.SetActive(false);
            }
            if (!updated2)
            {
                secDirector.gameObject.SetActive(false);
                if (!updated1)
                {
                    break;
                }
            }
            if (!animData.EmitterChanged && director.time >= animData.ExplosionDelay)
            {
                animData.EmitterChanged = true;
                animData.EmitterExplosion.Play();
            }

            yield return null;
        }
        animData.EmitterChanged = false;
        animData.DirectorGo.SetActive(false);
        secDirector.gameObject.SetActive(false);
        animData.Root.SetActive(false);

        freeAttacks.Add(groupIndex);
        animSet.Add(animIndex);
        attacksInProgress.Remove(allData);
    }

    private IEnumerator AAAnim(AAAnimationData data, float time)
    {
        if (data.Anim == null)
        {
            yield return null;
            yield break;
        }
        yield return new WaitForSeconds(time);

        data.Anim.enabled = false;
        data.Anim.enabled = true;
        data.Animator.SetBool(FireAnim, false);
        data.CrewRoot.SetActive(true);
        yield return new WaitForSeconds(crewInTime);

        data.Animator.SetBool(FireAnim, true);

        yield return new WaitForSeconds(aaFireTime);

        data.Animator.SetBool(FireAnim, false);

        yield return new WaitForSeconds(crewOutTime);

        data.HideThis.SetActive(false);
        data.CrewRoot.SetActive(false);
    }

    private IEnumerator AAAnimLoad(AAAnimationData data, float time)
    {
        bool ok = false;
        if (time > 0f)
        {
            ok = true;
            yield return new WaitForSeconds(time);
            time = 0f;
        }
        else
        {
            time = -time;
        }
        data.Anim.enabled = false;
        data.Anim.enabled = true;
        data.Animator.SetBool(FireAnim, false);
        data.CrewRoot.SetActive(true);

        if (ok || time < crewInTime)
        {
            ok = true;
            yield return new WaitForSeconds(crewInTime - time);
            time = 0f;
        }
        data.Animator.SetBool(FireAnim, true);

        if (ok || time < aaFireTime)
        {
            ok = true;
            yield return new WaitForSeconds(aaFireTime - time);
            time = 0f;
        }
        data.Animator.SetBool(FireAnim, false);

        if (ok || time < crewOutTime)
        {
            yield return new WaitForSeconds(crewOutTime - time);
        }
        data.HideThis.SetActive(false);
        data.CrewRoot.SetActive(false);
    }

    private IEnumerator AASound(EAASound type, float timeStart, float timeFinish)
    {
        if (timeStart > 0f)
        {
            yield return new WaitForSeconds(timeStart);
        }
        else
        {
            timeFinish += timeStart;
        }
        pausedAA = false;
        playingAA = true;
        aaSounds.PlayEvent(type);
        StrikeGroupManager.Instance.ToggleAA(true);
        if (timeFinish > 0f)
        {
            yield return new WaitForSeconds(timeFinish);
        }
        playingAA = false;
        pausedAA = false;

        aaSounds.Stop(true);
        StrikeGroupManager.Instance.ToggleAA(false);
    }

    private void UpdateDefencePoints()
    {
        defencePoints = defenceRoomDefencePoints;
        defencePoints += crewManagerDefencePoints;
        defencePoints += capDefencePoints;
        defencePoints += strikeGroupDefencePoints;
        defencePoints += islandBuffDefencePoints;
        defencePoints += sectionDebuff;
        defencePoints += strikeGroupExtraDefencePoints;
        defencePoints += pilotsDefencePoints;

        buffBuffPoints = currentBuff == ETemporaryBuff.MoreDefense ? 2 : 0;
        defencePoints += buffBuffPoints;
        //pointsText.text = defencePoints.ToString();
        defenceTooltip.SetValues(crewManagerDefencePoints, pilotsDefencePoints, islandBuffDefencePoints, strikeGroupDefencePoints, capDefencePoints, sectionDebuff, strikeGroupExtraDefencePoints, buffBuffPoints);
        defencePoints = Mathf.Max(defencePoints, 0);

        defenceTooltip.SectionDebuff.SetActive(sectionDebuff > 0);
        defenceTooltip.BuffBuff.SetActive(buffBuffPoints > 0);

        foreach (List<Image> li in defencePointsIcons)
        {
            var i = 0;
            foreach (Image image in li)
            {
                image.enabled = i++ < defencePoints;
            }
        }

        defencePointsText.text = defencePoints.ToString();
    }

    private void UpdateEscortPoints()
    {
        escortPoints = crewManagerEscortPoints;
        escortPoints += strikeGroupEscortPoints;
        escortPoints += capEscortPoints;
        escortPoints += strikeGroupExtraEscortPoints;
        escortPoints += islandBuffEscortPoints;
        if (currentBuff == ETemporaryBuff.MoreEscortDefense)
        {
            escortPoints += 2;
        }

        escortPointsText.text = escortPoints.ToString();
    }

    private EEnemyAttackTarget CheckStrikeGroupTarget(EEnemyAttackTarget target, bool redirect)
    {
        if (redirect && target == EEnemyAttackTarget.Carrier)
        {
            target = EEnemyAttackTarget.StrikeGroup;
        }

        if (target != EEnemyAttackTarget.Carrier)
        {
            if (!StrikeGroupManager.Instance.HasAnyEscort())
            {
                target = EEnemyAttackTarget.Carrier;
            }
        }
        return target;
    }

    private void SetupAnims(List<Dictionary<EEnemyAttackPower, List<AnimAttackData>>> attacks, List<AnimAttacksPrefabData> animsList, HashSet<int> freeAttackGroups, Dictionary<EEnemyAttackPower, HashSet<int>> freeAttacks)
    {
        for (int i = 0; i < animsList.Count; i++)
        {
            freeAttackGroups.Add(i);
            int j = 0;
            var dict = new Dictionary<EEnemyAttackPower, List<AnimAttackData>>();
            SetupAnimList(ref j, animsList[i].MediumStart, animsList[i].Anims, animsList[i].Camera, animsList[i].gameObject, dict, EEnemyAttackPower.Small);
            SetupAnimList(ref j, animsList[i].HeavyStart, animsList[i].Anims, animsList[i].Camera, animsList[i].gameObject, dict, EEnemyAttackPower.Medium);
            SetupAnimList(ref j, animsList[i].Anims.Count, animsList[i].Anims, animsList[i].Camera, animsList[i].gameObject, dict, EEnemyAttackPower.Heavy);
            attacks.Add(dict);
        }
        foreach (var pair in attacks[0])
        {
            var set = new HashSet<int>();
            for (int i = 0; i < pair.Value.Count; i++)
            {
                set.Add(i);
            }
            freeAttacks[pair.Key] = set;
        }
    }

    private void SetupAnimList(ref int index, int max, List<AnimAttackData> anims, GameObject camera, GameObject root, Dictionary<EEnemyAttackPower, List<AnimAttackData>> dict, EEnemyAttackPower key)
    {
        var list = new List<AnimAttackData>();
        for (; index < max; index++)
        {
            anims[index].Camera = camera;
            anims[index].Root = root;
            list.Add(anims[index]);
        }
        dict[key] = list;
    }

    private T ChooseRemoveRandom<T>(HashSet<T> set, List<T> list, string debug)
    {
        if (set.Count == 0)
        {
            foreach (var obj in list)
            {
                set.Add(obj);
            }
        }
        var result = RandomUtils.GetRandom(set);
        set.Remove(result);
        return result;
    }

    private void OnDateChanged()
    {
        if (isLoading)
        {
            return;
        }
        GetNextRandomAttacks(false);
    }

    private void OnViewChanged(ECameraView view)
    {
        bool previewCam = view == ECameraView.PreviewCamera;
        foreach (var bar in fullHDBars)
        {
            bar.SetActive(previewCam);
        }
        SetAASoundPosition(!previewCam);
        if (!previewCam)
        {
            foreach (var attackData in attacksInProgress)
            {
                attackData.AnimData.CameraGo.SetActive(false);
            }
            foreach (var data in reconAnims)
            {
                data.Camera.SetActive(false);
            }
            SetSubmarineCamera(false);
            HudManager.Instance.PopupHidden(this);
            //submarineTransCamera.gameObject.SetActive(false);
        }
    }

    private void SetSubmarineCamera(bool value)
    {
        if (!value && submarineAnim.Camera.activeSelf)
        {
            HudManager.Instance.UnsetBlockSpeed();
        }
        submarineAnim.Camera.SetActive(value);
        submarineAnim.Directors[0].gameObject.SetActive(value);
        if (!value || !FMODStudio.IsPlaying(submarineAnim.Emitters[0].EventInstance))
        {
            submarineAnim.Emitters[0].gameObject.SetActive(value);
        }
    }

    private void ForceSwitchCamera()
    {
        var cameraMan = CameraManager.Instance;
        if (cameraMan.CurrentCameraView != ECameraView.PreviewCamera)
        {
            if (cameraMan.CurrentCameraView != ECameraView.Blend)
            {
                prevView = cameraMan.CurrentCameraView;
            }
            cameraMan.ForceSwitchCamera(ECameraView.PreviewCamera);
            HudManager.Instance.PopupShown(this);
        }
    }

    private bool UpdateDirector(PlayableDirector director, double time)
    {
        if (director.time < (director.duration - .01d))
        {
            director.time += time;
            director.playableGraph.Evaluate();
            return true;
        }
        return false;
    }

    private void SetAASoundPosition(bool camera)
    {
        if (camera)
        {
            aaSoundsTrans.SetParent(aaSoundsCameraParent);
            aaSoundsTrans.localPosition = aaSoundsPosition;
        }
        else
        {
            aaSoundsTrans.SetParent(aaSoundPosOutsideCamera);
            aaSoundsTrans.localPosition = Vector3.zero;
        }
    }

    private void CreateSandboxAttacks(bool init)
    {
        if (WorldMap.Instance.gameObject.activeInHierarchy)
        {
            enemyAttacksOnUs = new List<EnemyAttackTimerCarrier>();
            enemyAttacksOnFriend = new List<EnemyAttackTimerFriend>();
            enemyAttacksRecon = new EnemyReconAttackTimer();
            enemyAttacksSubmarine = new EnemySubmarineAttackTimer();

            enemyAttacksRecon.Setup();
            enemyAttacksSubmarine.Setup();
            return;
        }
        times.Clear();
        int startHour = this.startHour;
        if (init)
        {
            int newStartHour = TimeManager.Instance.CurrentHour;
            if (newStartHour > startHour)
            {
                startHour = newStartHour;
            }
        }

        int minutes = startHour * 60 + minutesStep;
        int endMinutes = endHour * 60;
        while (minutes < endMinutes)
        {
            times.Add(minutes);
            minutes += minutesStep;
        }

        enemyAttacksOnUs = new List<EnemyAttackTimerCarrier>();
        enemyAttacksOnFriend = new List<EnemyAttackTimerFriend>();
        enemyAttacksRecon = new EnemyReconAttackTimer();
        enemyAttacksSubmarine = new EnemySubmarineAttackTimer();

        int level = levelsData.GetAdmiralLevel(SaveManager.Instance.Data.MissionRewards.SandboxAdmiralExp);
        AdmiralLevelAttackData admiralAttackData = null;
        foreach (var attackData in difficultyAttackDatas[(int)currentMap.Difficulty].AdmiralLevelAttackDatas)
        {
            if (attackData.AdmiralLevelThreshold > level)
            {
                admiralAttackData = attackData;
                break;
            }
        }

        var currentAttackCounts = sandboxAttacks[(int)currentMap.Difficulty].AttackCounts[(int)currentMap.EnemiesCount];
        Assert.IsTrue(currentAttackCounts.MinAttacks > 0);

        var attack = new EnemyAttackTimerCarrier();
        int currentSandboxTimes = times.Count;
        int count = CalculateCount(currentAttackCounts.MinAttacks, currentAttackCounts.MaxAttacks, currentSandboxTimes, maxSandboxTimes);

        foreach (var data in RandomiseAttackData(times, count, attack))
        {
            int min, max;
            float value = UnityRandom.value;
            if (value <= carrierTargetChance)
            {
                data.Kamikaze = value <= kamikazeChance;
                data.Target = EEnemyAttackTarget.Carrier;
                min = admiralAttackData.MinAttackPowerCarrier;
                max = admiralAttackData.MaxAttackPowerCarrier;
            }
            else
            {
                data.Target = EEnemyAttackTarget.StrikeGroup;
                min = admiralAttackData.MinAttackPowerEscort;
                max = admiralAttackData.MaxAttackPowerEscort;
            }
            data.AttackPower = UnityRandom.Range(min, max);
        }
        attack.Setup();
        enemyAttacksOnUs.Add(attack);
        Debug.Log("Sandbox attacks on us:");
        foreach (var data in attack.Datas)
        {
            Debug.Log($"{data.Hour}:{data.Minute}; kamikaze: {data.Kamikaze}; start target: {data.Target}; power: {data.AttackPower}");
        }

        enemyAttacksRecon = new EnemyReconAttackTimer();
        enemyAttacksSubmarine = new EnemySubmarineAttackTimer();

        count = CalculateCount(currentAttackCounts.MinScouts, currentAttackCounts.MaxScouts, currentSandboxTimes, maxSandboxTimes);
        for (int i = 0; i < count; i++)
        {
            if (times.Count == 0)
            {
                break;
            }
            if (scoutSubHelper.Count == 0)
            {
                scoutSubHelper.Add(true);
                scoutSubHelper.Add(true);
                scoutSubHelper.Add(false);
                scoutSubHelper.Add(false);
            }
            if (RandomUtils.GetRemoveRandom(scoutSubHelper))
            {
                enemyAttacksRecon.Add(CreateData<EnemyAttacksReconData>(times));
            }
            else
            {
                enemyAttacksSubmarine.Add(CreateData<EnemySubmarineAttackData>(times));
            }
        }
        enemyAttacksRecon.Setup();
        Debug.Log("Sandbox recon:");
        foreach (var data in enemyAttacksRecon.Datas)
        {
            Debug.Log($"{data.Hour}:{data.Minute}");
        }

        enemyAttacksSubmarine.Setup();
        Debug.Log("Sandbox submarine:");
        foreach (var data in enemyAttacksSubmarine.Datas)
        {
            Debug.Log($"{data.Hour}:{data.Minute}");
        }

        var attackFriend = new EnemyAttackTimerFriend();
        if (currentMap.AllyAttacks)
        {
            minutes = startHour * 60 + 20;
            endMinutes = endHour * 60;
            while (minutes < endMinutes)
            {
                times.Add(minutes);
                minutes += minutesStep;
            }
            count = CalculateCount(currentAttackCounts.MinAlliesAttacks, currentAttackCounts.MaxAlliesAttacks, times.Count, maxSandboxTimes2);
            foreach (var _ in RandomiseAttackData(times, count, attackFriend))
                ;
        }
        attackFriend.Setup();
        enemyAttacksOnFriend.Add(attackFriend);
        Debug.Log("Sandbox attacks on friends:");
        foreach (var data in attackFriend.Datas)
        {
            Debug.Log($"{data.Hour}:{data.Minute}");
        }
    }

    private int CalculateCount(int min, int max, int currentTimes, int maxTimes)
    {
        int count = UnityRandom.Range(min, max + 1);
        if (count > 0 && currentTimes < maxTimes)
        {
            return ((count * currentTimes) / maxTimes) + 1;
        }
        return count;
    }

    private IEnumerable<T> RandomiseAttackData<T>(HashSet<int> times, int count, EnemyAttackTimer<T> timer) where T : EnemyAttackBaseData, new()
    {
        for (int i = 0; i < count; i++)
        {
            if (times.Count == 0)
            {
                break;
            }
            var data = CreateData<T>(times);
            timer.Add(data);
            yield return data;
        }
    }

    private T CreateData<T>(HashSet<int> times) where T : EnemyAttackBaseData, new()
    {
        var data = new T();
        data.Minute = RandomUtils.GetRandom(times);
        times.Remove(data.Minute);
        data.Hour = data.Minute / 60;
        data.Minute %= 60;
        return data;
    }

    private IEnumerable<KeyValuePair<T, SandboxAttacksSaveData>> SaveDatas<T>(List<T> datas, int type) where T : EnemyAttackBaseData
    {
        SandboxAttacksSaveData result = new SandboxAttacksSaveData();
        result.Type = type;
        foreach (var data in datas)
        {
            result.Hours = data.Hour;
            result.Minutes = data.Minute;
            yield return new KeyValuePair<T, SandboxAttacksSaveData>(data, result);
        }
    }

    private IEnumerable<KeyValuePair<T, SandboxAttacksSaveData>> LoadDatas<T>(List<SandboxAttacksSaveData> saves, EnemyAttackTimer<T> timer, int startIndex, int type) where T : EnemyAttackBaseData, new()
    {
        for (int i = startIndex; i < saves.Count; i++)
        {
            var save = saves[i];
            if (save.Type != type)
            {
                break;
            }
            var data = new T();
            data.Hour = save.Hours;
            data.Minute = save.Minutes;
            data.AttackPower = save.AttackPower;

            timer.Add(data);
            yield return new KeyValuePair<T, SandboxAttacksSaveData>(data, save);
        }
        timer.Setup();
    }

    public void OnWorldMapToggled(bool shown)
    {
        StopAllCoroutines();
        foreach (var anim in attacksInProgress)
        {
            var animData = anim.AnimData;
            animData.Director.time = 0d;
            animData.Director.Stop();
            animData.Director.Evaluate();
            animData.DirectorSecondary.time = 0d;
            animData.DirectorSecondary.Stop();
            animData.DirectorSecondary.Evaluate();
            animData.EmitterChanged = false;
            animData.DirectorGo.SetActive(false);
            animData.DirectorSecondary.gameObject.SetActive(false);
            animData.EmitterExplosion.Stop();
            animData.Emitter.Stop();
            animData.Root.SetActive(false);
        }
        foreach (var pair in animations[0])
        {
            freeAttacks[pair.Key].Clear();
            for (int i = 0; i < pair.Value.Count; i++)
            {
                freeAttacks[pair.Key].Add(i);
            }
        }
        attacksInProgress.Clear();
        freeAttacksGroups.Clear();
        for (int i = 0; i < attacksAnims.Count; i++)
        {
            freeAttacksGroups.Add(i);
        }
        foreach (var aaAnim in aaAnims)
        {
            for (int i = 0; i < aaAnim.Anims.Count; i++)
            {
                var data = aaAnim.Anims[i];
                data.Animator.SetBool(FireAnim, false);
                data.HideThis.SetActive(false);
                data.CrewRoot.SetActive(false);
                data.Anim.enabled = false;
                data.Anim.time = 0;
                data.Anim.Stop();
                data.Anim.Evaluate();
            }
        }
        playingAA = false;
        pausedAA = false;
        aaSounds.Stop(true);
        StrikeGroupManager.Instance.ToggleAA(false);
    }
}
