using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EventManager : MonoBehaviour, IEnableable
{
    public static EventManager Instance = null;

    public event Action EventClicked = delegate { };
    public event Action CriticalDamageReached = delegate { };

    public EEventFlag EventsEnabled
    {
        get;
        set;
    }

    private bool Ignore => isLoading || worldmap;

    [SerializeField]
    private List<DynamicEvent> events = null;

    [SerializeField]
    private MissionListExpand missionListExpand = null;
    [SerializeField]
    private GameObject radar = null;

    [SerializeField]
    private string PlayerDetectedTitle = "Player Detected";

    [SerializeField]
    private string waypointTitle = null;
    [SerializeField]
    private string nightTitle = null;
    [SerializeField]
    private string notEnoughPlanesTitle = null;
    [SerializeField]
    private string lowSuppliesTitle = null;
    [SerializeField]
    private string noSuppliesTitle = null;
    [SerializeField]
    private string heavyDmgTitle = null;
    [SerializeField]
    private string heavyManeuversTitle = null;
    [SerializeField]
    private string calloffCapTitle = null;

    [SerializeField]
    private string cantChangeSwitches = null;

    [SerializeField]
    private string runwayDamagedTitle = null;
    [SerializeField]
    private string missionStageIncorrectTitle = null;
    [SerializeField]
    private string dayTimeIncorrectTitle = null;
    [SerializeField]
    private string weatherIncorrectTitle = null;
    [SerializeField]
    private string deckModeIncorrectTitle = null;
    [SerializeField]
    private string notEnoughSpaceOnDeckTitle = null;
    [SerializeField]
    private string liftDamagedTitle = null;
    [SerializeField]
    private string squadronIsNotOnDeckTitle = null;
    [SerializeField]
    private string generatorsNotWorkingTitle = null;
    [SerializeField]
    private string cannotGetSquadronTitle = null;

    [SerializeField]
    private string orderCannotBeDoneTitle = null;
    [SerializeField]
    private string orderQueueFullTitle = null;

    [Header("Popup Objects")]
    [SerializeField]
    private Animator eventPopup = null;
    [SerializeField]
    private Text eventPopupText = null;
    [SerializeField]
    private Image eventPopupImage = null;
    [SerializeField]
    private Image eventPopupShield = null;

    [Header("Popup Shields Icons")]
    [SerializeField]
    private Sprite defaultPopupShield = null;
    [SerializeField]
    private Sprite criticalPopupShield = null;

    [Header("Popup Icons")]
    [SerializeField]
    private Sprite defaultPopupImage = null;
    [SerializeField]
    private Sprite playerDetectedImage = null;
    [SerializeField]
    private Sprite nightImage = null;
    [SerializeField]
    private Sprite notEnoughPlanesImage = null;
    [SerializeField]
    private Sprite lowSuppliesImage = null;
    [SerializeField]
    private Sprite noSuppliesImage = null;
    [SerializeField]
    private Sprite heavyDmgImage = null;
    [SerializeField]
    private Sprite calloffCapImage = null;
    [Space(20)]

    [SerializeField]
    private float minCriticalPercentage = .3f;

    [SerializeField]
    private TooltipCaller segmentDamagedTooltip = null;
    [SerializeField]
    private TooltipCaller sectionShutdownTooltip = null;
    [SerializeField]
    private TooltipCaller detectionTooltip = null;
    [SerializeField]
    private TooltipCaller enemyAttackTooltip = null;
    [SerializeField]
    private TooltipCaller enemySubmarineTooltip = null;
    [SerializeField]
    private TooltipCaller carrierImmobilizedTooltip = null;

    [SerializeField]
    private string damagedSegmentID = default;
    [SerializeField]
    private string injuredInSegmentID = default;
    [SerializeField]
    private string detectedID = default;
    [SerializeField]
    private string notDetectedID = default;
    [SerializeField]
    private string enemyTargetCarrierID = default;
    [SerializeField]
    private string enemyTargetStrikeGroupID = default;
    [SerializeField]
    private string submarineTargetCarrierID = default;
    [SerializeField]
    private string submarineTargetStrikeGroupID = default;
    [SerializeField]
    private string noSpeedID = default;
    [SerializeField]
    private string noWaypointID = default;

    [SerializeField]
    private string damagedSegmentPopupID = default;
    [SerializeField]
    private string injuredInSegmentPopupID = default;
    [SerializeField]
    private string detectedPopupID = default;
    [SerializeField]
    private string notDetectedPopupID = default;
    [SerializeField]
    private string enemyTargetCarrierPopupID = default;
    [SerializeField]
    private string enemyTargetStrikeGroupPopupID = default;
    [SerializeField]
    private string submarineTargetCarrierPopupID = default;
    [SerializeField]
    private string submarineTargetStrikeGroupPopupID = default;
    [SerializeField]
    private string noSpeedPopupID = default;
    [SerializeField]
    private string noWaypointPopupID = default;

    [SerializeField]
    private float immobileReminderTime = 120f;
    [SerializeField]
    private Expandable shipSpeed = null;
    [SerializeField]
    private Sprite midwaySprite = null;

    [SerializeField]
    private AudioQueue audioQueue = null;

    private Dictionary<string, Sprite> eventPopupSprites;

    private List<SectionSegment> damagedSegments;
    private List<SectionRoom> shutdownSection;
    private List<RadarEnemyData> radarEnemies;
    private bool newIntel;
    private bool revealedEnemy;
    private List<TacticalMission> submarineHunts;
    private List<TacticalMission> hostileScouts;
    private List<EnemyAttackData> attacksImminent;
    private List<TacticalMission> allyMissions;
    private bool savedDetected;
    private int sunkShip;
    private bool objectivesChanged;
    private bool criticalDamage;
    private float damagePercentage;
    private List<CrewUnit> crewsInjured;
    private List<CrewUnit> crewsDead;
    private SectionSegment squadronDamagedCenterSegment;
    private List<RadarEnemyData> kamikazeEnemies;

    private Dictionary<SectionRoom, EAudio> specialSoundsRooms;
    private bool setuped;
    private float time;

    private bool canEvent;
    private bool isLoading;
    private bool worldmap;
    private bool disabled;
    private EWreckType prevType = EWreckType.Wreck;

    private EEventType currentEventPopup;
    private Queue<EEventType> eventQueue;
    private Dictionary<EEventType, EventQueueData> queueData;
    private Queue<EAudio> customQueue;
    private Dictionary<EAudio, EventQueueData> customQueueData;
    private EAudio currentAudioEvent;
    private bool queueBlocked;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        damagedSegments = new List<SectionSegment>();
        shutdownSection = new List<SectionRoom>();
        radarEnemies = new List<RadarEnemyData>();
        attacksImminent = new List<EnemyAttackData>();
        submarineHunts = new List<TacticalMission>();
        hostileScouts = new List<TacticalMission>();
        allyMissions = new List<TacticalMission>();
        crewsInjured = new List<CrewUnit>();
        crewsDead = new List<CrewUnit>();
        kamikazeEnemies = new List<RadarEnemyData>();

        eventQueue = new Queue<EEventType>();
        queueData = new Dictionary<EEventType, EventQueueData>();
        customQueue = new Queue<EAudio>();
        customQueueData = new Dictionary<EAudio, EventQueueData>();

        foreach (var data in events)
        {
            data.RectTransform.gameObject.SetActive(false);
            data.Button.onClick.AddListener(() => BackgroundAudio.Instance.PlayEvent(EMainSceneUI.ClickEvent));
        }

        eventPopupSprites = new Dictionary<string, Sprite>()
        {
            {PlayerDetectedTitle, playerDetectedImage},
            {nightTitle, nightImage},
            {notEnoughPlanesTitle, notEnoughPlanesImage},
            {lowSuppliesTitle, lowSuppliesImage},
            {noSuppliesTitle, noSuppliesImage},
            {heavyDmgTitle, heavyDmgImage},
            {calloffCapTitle, calloffCapImage},
            {orderCannotBeDoneTitle, notEnoughPlanesImage},
            {orderQueueFullTitle, notEnoughPlanesImage},
            {runwayDamagedTitle, notEnoughPlanesImage},
            {missionStageIncorrectTitle, notEnoughPlanesImage},
            {dayTimeIncorrectTitle, notEnoughPlanesImage},
            {weatherIncorrectTitle, notEnoughPlanesImage},
            {deckModeIncorrectTitle, notEnoughPlanesImage},
            {notEnoughSpaceOnDeckTitle, notEnoughPlanesImage},
            {liftDamagedTitle, notEnoughPlanesImage},
            {squadronIsNotOnDeckTitle, notEnoughPlanesImage},
            {generatorsNotWorkingTitle, notEnoughPlanesImage},
            {cannotGetSquadronTitle, notEnoughPlanesImage},
        };

        specialSoundsRooms = new Dictionary<SectionRoom, EAudio>();

        EventsEnabled = (EEventFlag)(-1);
        canEvent = true;
    }

    private void Start()
    {
        events[(int)EEventType.SegmentDamage].Button.onClick.AddListener(ZoomToDynamicEvent);
        events[(int)EEventType.SectionShutdown].Button.onClick.AddListener(ZoomToSection);
        events[(int)EEventType.NewIntel].Button.onClick.AddListener(HighlightUO);
        events[(int)EEventType.UoIsEnemy].Button.onClick.AddListener(HighlightEnemy);
        events[(int)EEventType.Attack].Button.onClick.AddListener(ZoomToAttack);
        events[(int)EEventType.SubmarineHunt].Button.onClick.AddListener(HighlightSubmarineHunt);
        events[(int)EEventType.HostileScouts].Button.onClick.AddListener(HighlightHostileScouts);
        events[(int)EEventType.AllyUnderAttack].Button.onClick.AddListener(HighlightAllyCap);
        events[(int)EEventType.DetectionChanged].Button.onClick.AddListener(HighlightDetection);
        events[(int)EEventType.ShipSunk].Button.onClick.AddListener(OpenEscortPanel);
        events[(int)EEventType.BuffExpired].Button.onClick.AddListener(OpenBuffs);
        events[(int)EEventType.ObjectivesUpdated].Button.onClick.AddListener(OpenObjectives);
        events[(int)EEventType.CarrierHealthCritical].Button.onClick.AddListener(ChangeToSectionsView);
        events[(int)EEventType.CrewInjured].Button.onClick.AddListener(CrewInjured);
        events[(int)EEventType.CrewDead].Button.onClick.AddListener(CrewDead);
        events[(int)EEventType.SquadronDamaged].Button.onClick.AddListener(GoToHangar);
        events[(int)EEventType.CarrierImmobile].Button.onClick.AddListener(OnCarrierImmobile);
        events[(int)EEventType.Wreck].Button.onClick.AddListener(GoToWreck);
        events[(int)EEventType.Kamikaze].Button.onClick.AddListener(ZoomToKamikaze);
        events[(int)EEventType.BigGun].Button.onClick.AddListener(ZoomToBigGun);

        var tacticMan = TacticManager.Instance;
        tacticMan.Markers.UoShowChanged += UoShowChanged;
        tacticMan.ObjectRevealed += OnObjectRevealed;

        EnemyAttacksManager.Instance.DetectedChanged += OnDetectedChanged;
        StrikeGroupManager.Instance.ShipSunk += OnShipSunk;
        IslandsAndOfficersManager.Instance.BuffExpired += OnBuffExpired;
        IslandsAndOfficersManager.Instance.BuffConfirmed += NewBuff;

        var objectivesMan = ObjectivesManager.Instance;
        objectivesMan.ObjectiveChanged += OnObjectiveChanged;
        objectivesMan.ObjectiveFinishing += OnObjectiveFinishing;
        objectivesMan.ObjectivesOpened += OnObjectivesOpened;

        var hudMan = HudManager.Instance;
        hudMan.TacticMapOpened += OnTacticMapOpened;
        hudMan.EscortPanelOpened += OnEscortPanelOpened;

        CameraManager.Instance.ViewChanged += OnViewChanged;

        var sectionMan = SectionRoomManager.Instance;
        specialSoundsRooms[sectionMan.Sickbay] = EAudio.MedicalShutdown;
        specialSoundsRooms[sectionMan.Deck] = EAudio.DeckShutdown;
        specialSoundsRooms[sectionMan.Pilots] = EAudio.AirShutdown;
        specialSoundsRooms[sectionMan.AircraftWorkshop] = EAudio.EngineeringShutdown;
        specialSoundsRooms[sectionMan.Engines] = EAudio.NavigationShutdown;
        specialSoundsRooms[sectionMan.AAGuns] = EAudio.AAShutdown;

        sectionMan.GeneratorsStateChanged += OnGeneratorsStateChanged;

        var dcMan = DamageControlManager.Instance;
        foreach (var room in dcMan.MaintenanceRooms)
        {
            specialSoundsRooms[room] = EAudio.MaintenanceShutdown;
        }
        foreach (var room in dcMan.PumpRooms)
        {
            specialSoundsRooms[room] = EAudio.PumpsShutdown;
        }

        WorldMap.Instance.Toggled += OnWorldMapToggled;
    }

    private void Update()
    {
        if (worldmap)
        {
            return;
        }
        var hudMan = HudManager.Instance;
        bool noSpeed = hudMan.ShipSpeedup <= 0f && hudMan.CanSetSpeed;
        bool noWaypoint = !TacticManager.Instance.Carrier.HasWaypoint;

        if (!disabled && (noSpeed || noWaypoint))
        {
            if (Time.timeScale > 0f)
            {
                time += Time.unscaledDeltaTime;
            }
            if (time >= immobileReminderTime)
            {
                time -= immobileReminderTime;
                AddImmobileCarrier(noSpeed);
            }

            if (events[(int)EEventType.CarrierImmobile].RectTransform.gameObject.activeSelf)
            {
                carrierImmobilizedTooltip.SetTitles(noSpeed ? noSpeedID : noWaypointID);
            }
        }
        else
        {
            if (disabled)
            {
                time = immobileReminderTime;
            }
            RemoveImmobileCarrier();
        }
        UpdateEnemyTooltip();

        if (!queueBlocked && (!eventPopup.gameObject.activeInHierarchy || eventPopup.GetCurrentAnimatorStateInfo(0).normalizedTime >= .88f))
        {
            if (eventQueue.Count == 0)
            {
                canEvent = true;
            }
            else
            {
                currentEventPopup = eventQueue.Peek();
                if (currentEventPopup == EEventType.Count)
                {
                    EventAudioSynchronized();
                }
                else
                {
                    queueBlocked = true;

                    if (currentEventPopup == EEventType.SectionShutdown || currentEventPopup == EEventType.SegmentDamage)
                    {
                        currentAudioEvent = customQueue.Peek();
                    }
                    else
                    {
                        currentAudioEvent = EventToAdvisor(currentEventPopup);
                    }

                    audioQueue.Queue(currentAudioEvent, EventAudioSynchronized);
                }
            }
        }
    }

    public void SetEnable(bool enable)
    {
        disabled = !enable;
        if (!enable)
        {
            ClearEvents();
        }
    }

    public void Setup()
    {
        setuped = true;
    }

    public void LoadStart()
    {
        setuped = false;
        isLoading = true;
    }

    public void LoadData(List<EventSaveData> list)
    {
        isLoading = false;

        var crewMan = CrewManager.Instance;
        var dcMan = DamageControlManager.Instance;
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        var sectionMan = SectionRoomManager.Instance;
        var tacticMan = TacticManager.Instance;
        foreach (var data in list)
        {
            switch (data.Type)
            {
                case EEventType.SegmentDamage:
                    foreach (var segmentIndex in data.Params)
                    {
                        var segment = sectionMan.GetSegment(segmentIndex);
                        AddDynamicEvent(segment, segment.HasAnyDamageWithBroken() ? EDynamicEventType.Fire: EDynamicEventType.Injured);
                    }
                    break;
                case EEventType.SectionShutdown:
                    foreach (var section in data.Params)
                    {
                        if (section == -2)
                        {
                            AddSectionShutdown(dcMan.WreckSection);
                        }
                        else
                        {
                            AddSectionShutdown(sectionMan.GetSection(section));
                        }
                    }
                    break;
                case EEventType.NewIntel:
                    NewIntel();
                    break;
                case EEventType.UoIsEnemy:
                    RevealedEnemy();
                    break;
                case EEventType.Attack:
                    foreach (var enemy in data.Params)
                    {
                        AddRadarEnemyData(enemyAttacksMan.GetEnemy(enemy));
                    }
                    break;
                case EEventType.SubmarineHunt:
                    foreach (var sub in data.Params)
                    {
                        var mission = tacticMan.GetMission(EMissionOrderType.SubmarineHunt, sub);
                        StartSubmarineHunt(mission);
                    }
                    break;
                case EEventType.HostileScouts:
                    foreach (var scout in data.Params)
                    {
                        AddHostileScout(tacticMan.GetMission(EMissionOrderType.CounterHostileScouts, scout));
                    }
                    break;
                case EEventType.AllyUnderAttack:
                    var type = enemyAttacksMan.FriendlyCAPIsMidway ? EMissionOrderType.FriendlyCAPMidway : EMissionOrderType.FriendlyFleetCAP;
                    foreach (var ally in data.Params)
                    {
                        AllyAttacked(tacticMan.GetMission(type, ally));
                    }
                    break;
                case EEventType.DetectionChanged:
                    savedDetected = !EnemyAttacksManager.Instance.IsDetected;
                    DetectionChanged(!savedDetected);
                    break;
                case EEventType.ShipSunk:
                    for (int i = 0; i < data.Params[0]; i++)
                    {
                        SunkShip();
                    }
                    break;
                case EEventType.BuffExpired:
                    BuffFinished();
                    break;
                case EEventType.ObjectivesUpdated:
                    ObjectivesChanged();
                    break;
                case EEventType.CarrierHealthCritical:
                    SetReachedCriticalDamage(data.Params[0] / 100f);
                    break;
                case EEventType.CrewInjured:
                    foreach (var crew in data.Params)
                    {
                        AddCrewInjured(crewMan.GetCrew(crew));
                    }
                    break;
                case EEventType.CrewDead:
                    foreach (var crew in data.Params)
                    {
                        AddCrewDead(crewMan.GetCrew(crew));
                    }
                    break;
                case EEventType.SquadronDamaged:
                    AddSquadronDamaged(sectionMan.GetSegment(data.Params[0]));
                    break;
                case EEventType.CarrierImmobile:
                    AddImmobileCarrier(data.Params[0] > 0);
                    break;
                case EEventType.Kamikaze:
                    foreach (var enemy in data.Params)
                    {
                        AddKamikaze(enemyAttacksMan.GetEnemy(enemy));
                    }
                    break;
                case EEventType.BigGun:
                    AddBigGun();
                    break;
            }
        }
        if (AircraftCarrierDeckManager.Instance.HasDamage)
        {
            foreach (var wreck in PlaneMovementManager.Instance.CurrentWrecks)
            {
                if (wreck != null && !wreck.AnimCrash)
                {
                    AddWreck();
                    break;
                }
            }
        }

        setuped = true;
    }

    public void SaveData(List<EventSaveData> list)
    {
        list.Clear();
        var data = new EventSaveData();

        var dcMan = DamageControlManager.Instance;
        var sectionMan = SectionRoomManager.Instance;
        if (damagedSegments.Count > 0)
        {
            data.Type = EEventType.SegmentDamage;
            data.Params = new List<int>();
            foreach (var segment in damagedSegments)
            {
                data.Params.Add(sectionMan.IndexOf(segment));
            }
            list.Add(data);
        }
        if (shutdownSection.Count > 0)
        {
            data.Type = EEventType.SectionShutdown;
            data.Params = new List<int>();
            foreach (var section in shutdownSection)
            {
                if (section == dcMan.WreckSection)
                {
                    data.Params.Add(-2);
                }
                else
                {
                    data.Params.Add(sectionMan.IndexOf(section));
                }
            }
            list.Add(data);
        }
        if (newIntel)
        {
            data.Type = EEventType.NewIntel;
            data.Params = new List<int>();
            list.Add(data);
        }
        if (revealedEnemy)
        {
            data.Type = EEventType.UoIsEnemy;
            data.Params = new List<int>();
            list.Add(data);
        }
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        if (radarEnemies.Count > 0)
        {
            data.Type = EEventType.Attack;
            data.Params = new List<int>();

            foreach (var radarEnemy in radarEnemies)
            {
                data.Params.Add(enemyAttacksMan.IndexOf(radarEnemy));
            }
            list.Add(data);
        }
        SaveList(submarineHunts, EEventType.SubmarineHunt, ref data, list);
        SaveList(hostileScouts, EEventType.HostileScouts, ref data, list);
        SaveList(allyMissions, EEventType.AllyUnderAttack, ref data, list);

        if (savedDetected != enemyAttacksMan.IsDetected)
        {
            data.Type = EEventType.DetectionChanged;
            list.Add(data);
        }

        if (sunkShip > 0)
        {
            data.Type = EEventType.ShipSunk;
            data.Params = new List<int>();
            data.Params.Add(sunkShip);
            list.Add(data);
        }
        if (events[(int)EEventType.BuffExpired].RectTransform.gameObject.activeSelf)
        {
            data.Type = EEventType.BuffExpired;
            data.Params = new List<int>();
            list.Add(data);
        }
        if (objectivesChanged)
        {
            data.Type = EEventType.ObjectivesUpdated;
            data.Params = new List<int>();
            list.Add(data);
        }
        if (criticalDamage)
        {
            data.Type = EEventType.CarrierHealthCritical;
            data.Params = new List<int>();
            data.Params.Add(Mathf.RoundToInt(damagePercentage * 100f));
            list.Add(data);
        }
        SaveCrew(crewsInjured, EEventType.CrewInjured, ref data, list);
        SaveCrew(crewsDead, EEventType.CrewDead, ref data, list);
        if (events[(int)EEventType.SquadronDamaged].RectTransform.gameObject.activeSelf)
        {
            data.Type = EEventType.SquadronDamaged;
            data.Params = new List<int>();
            data.Params.Add(sectionMan.IndexOf(squadronDamagedCenterSegment));
            list.Add(data);
        }
        if (events[(int)EEventType.CarrierImmobile].RectTransform.gameObject.activeSelf)
        {
            var hudMan = HudManager.Instance;

            data.Type = EEventType.CarrierImmobile;
            data.Params = new List<int>();
            data.Params.Add((hudMan.ShipSpeedup <= 0f && hudMan.CanSetSpeed) ? 1 : 0);
            list.Add(data);
        }
        if (kamikazeEnemies.Count > 0)
        {
            data.Type = EEventType.Kamikaze;
            data.Params = new List<int>();

            foreach (var radarEnemy in kamikazeEnemies)
            {
                data.Params.Add(enemyAttacksMan.IndexOf(radarEnemy));
            }
            list.Add(data);
        }
        if (events[(int)EEventType.BigGun].RectTransform.gameObject.activeSelf)
        {
            data.Type = EEventType.BigGun;
            data.Params = new List<int>();
            list.Add(data);
        }
    }

    public void AddDynamicEvent(SectionSegment affectedSegment, EDynamicEventType dynamicType)
    {
        if (IsEventDisabled(EEventFlag.SegmentDamage))
        {
            return;
        }
        if (affectedSegment.Untouchable || damagedSegments.Contains(affectedSegment))
        {
            return;
        }
        damagedSegments.Add(affectedSegment);

        EAudio audio;
        switch(dynamicType)
        {
            case EDynamicEventType.Fire:
                audio = EAudio.Fire;
                break;
            case EDynamicEventType.Water:
                audio = EAudio.Flood;
                break;
            case EDynamicEventType.Damage:
                audio = EAudio.Fault;
                break;
            case EDynamicEventType.Injured:
                audio = EAudio.Injured;
                break;
            default:
                audio = EAudio.Generate;
                break;
        }

        UpdateButton(EEventType.SegmentDamage, damagedSegments.Count, true, audio != EAudio.Generate, audio, dynamicType == EDynamicEventType.Injured ? injuredInSegmentPopupID : damagedSegmentPopupID);

        UpdateSegmentTooltip();
    }

    public void RemoveDynamicEvent(SectionSegment affectedSegment)
    {
        if (Ignore)
        {
            return;
        }
        if (damagedSegments.Contains(affectedSegment) && !affectedSegment.HasAnyIssue())
        {
            damagedSegments.Remove(affectedSegment);
            UpdateButton(EEventType.SegmentDamage, damagedSegments.Count, false);
        }
        UpdateSegmentTooltip();
    }

    public void AddSectionShutdown(SectionRoom room)
    {
        if (IsEventDisabled(EEventFlag.SectionShutdown))
        {
            return;
        }
        if (room.SubsectionRooms[0].Segments[0].Untouchable || shutdownSection.Contains(room))
        {
            return;
        }
        if (!specialSoundsRooms.TryGetValue(room, out var audio))
        {
            audio = EAudio.SectionShutdown;
        }
        shutdownSection.Add(room);
        UpdateButton(EEventType.SectionShutdown, shutdownSection.Count, true, true, audio, room.DamageTooltipTitleID);

        UpdateShutdownTooltip();
    }

    public void RemoveSectionShutdown(SectionRoom room)
    {
        if (Ignore)
        {
            return;
        }
        foreach (var sub in room.SubsectionRooms)
        {
            if (sub.Segments[0].Untouchable)
            {
                RemoveDynamicEvent(room.SubsectionRooms[0].Segments[0]);
                return;
            }
        }
        int index = shutdownSection.IndexOf(room);
        if (index != -1)
        {
            shutdownSection.RemoveAt(index);
            UpdateButton(EEventType.SectionShutdown, shutdownSection.Count, false);
        }

        UpdateShutdownTooltip();
    }

    public void NewIntel()
    {
        if (IsEventDisabled(EEventFlag.NewIntel))
        {
            return;
        }
        if (!newIntel)
        {
            newIntel = true;
        }
        UpdateButton(EEventType.NewIntel, 1, true, true);
    }

    public void RevealedEnemy()
    {
        if (IsEventDisabled(EEventFlag.UoIsEnemy))
        {
            return;
        }
        if (!revealedEnemy)
        {
            revealedEnemy = true;
        }
        UpdateButton(EEventType.UoIsEnemy, 1, true, true);
    }

    public void AddRadarEnemyData(RadarEnemyData data)
    {
        if (IsEventDisabled(EEventFlag.Attack))
        {
            return;
        }
        radarEnemies.Add(data);

        bool carrier = data.Data.CurrentTarget == EEnemyAttackTarget.Carrier;
        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            UpdateButton(EEventType.Attack, radarEnemies.Count, true, true, EAudio.Generate, carrier ? enemyTargetCarrierPopupID : enemyTargetStrikeGroupPopupID);
        }

        UpdateEnemyTooltip();
    }

    public void RemoveRadarEnemyData(RadarEnemyData data)
    {
        if (Ignore)
        {
            return;
        }
        radarEnemies.Remove(data);
        UpdateButton(EEventType.Attack, SectionRoomManager.Instance.GeneratorsAreWorking ? radarEnemies.Count : 0, false);

        UpdateEnemyTooltip();
    }

    public void StartSubmarineHunt(TacticalMission mission)
    {
        if (IsEventDisabled(EEventFlag.SubmarineHunt))
        {
            return;
        }
        submarineHunts.Add(mission);

        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            bool carrier = mission.RadarObject.Target == EEnemyAttackTarget.Carrier;
            UpdateButton(EEventType.SubmarineHunt, submarineHunts.Count, true, true, EAudio.Generate, carrier ? submarineTargetCarrierPopupID : submarineTargetStrikeGroupPopupID);
        }

        UpdateSubmarineTooltip();
        //if (!submarineHunt)
        //{
        //    submarineHunt = true;
        //    BackgroundAudio.Instance.PlayEvent(EMainSceneUI.ShowEvent);
        //    UpdateButton(EEventType.SubmarineHunt, 1);
        //}
    }

    public void RemoveSubmarineHunt(TacticalMission mission)
    {
        if (Ignore)
        {
            return;
        }
        submarineHunts.Remove(mission);
        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            UpdateButton(EEventType.SubmarineHunt, submarineHunts.Count, false);
        }

        UpdateSubmarineTooltip();
    }

    public void AddHostileScout(TacticalMission mission)
    {
        if (IsEventDisabled(EEventFlag.HostileScouts))
        {
            return;
        }
        hostileScouts.Add(mission);
        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            UpdateButton(EEventType.HostileScouts, hostileScouts.Count, true, true);
        }
    }

    public void RemoveHostileScout(TacticalMission mission)
    {
        if (Ignore)
        {
            return;
        }
        hostileScouts.Remove(mission);
        UpdateButton(EEventType.HostileScouts, SectionRoomManager.Instance.GeneratorsAreWorking ? hostileScouts.Count : 0, false);
    }

    public void AllyAttacked(TacticalMission mission)
    {
        if (IsEventDisabled(EEventFlag.AllyUnderAttack))
        {
            return;
        }
        allyMissions.Add(mission);
        UpdateButton(EEventType.AllyUnderAttack, allyMissions.Count, true, true);
    }

    public void RemoveAllyAttacked(TacticalMission mission)
    {
        if (Ignore)
        {
            return;
        }
        if (allyMissions.Remove(mission))
        {
            UpdateButton(EEventType.AllyUnderAttack, allyMissions.Count, false);
        }
    }

    public void DetectionChanged(bool detected)
    {
        if (IsEventDisabled(EEventFlag.DetectionChanged))
        {
            return;
        }
        detectionTooltip.SetTitles(detected ? detectedID : notDetectedID);
        UpdateButton(EEventType.DetectionChanged, detected == savedDetected ? 0 : 1, detected != savedDetected, detected != savedDetected, EAudio.Generate, detected ? detectedPopupID : notDetectedPopupID);
    }

    public void SunkShip()
    {
        if (IsEventDisabled(EEventFlag.ShipSunk))
        {
            return;
        }
        sunkShip++;
        UpdateButton(EEventType.ShipSunk, sunkShip, true, true);
    }

    public void BuffFinished()
    {
        if (IsEventDisabled(EEventFlag.BuffExpired))
        {
            return;
        }
        UpdateButton(EEventType.BuffExpired, 1, true, true);
    }

    public void NewBuff()
    {
        if (Ignore || disabled)
        {
            return;
        }
        UpdateButton(EEventType.BuffExpired, 0, false);
    }

    public void ObjectivesChanged()
    {
        if (IsEventDisabled(EEventFlag.ObjectivesUpdated))
        {
            return;
        }
        if (!objectivesChanged)
        {
            objectivesChanged = true;
            UpdateButton(EEventType.ObjectivesUpdated, 1, true, true);
        }
    }

    public void SetReachedCriticalDamage(float percentage)
    {
        if (IsEventDisabled(EEventFlag.CarrierHealthCritical))
        {
            return;
        }
        if (percentage < minCriticalPercentage)
        {
            HideCriticalDamage();
        }
        else
        {
            damagePercentage = percentage;
            UpdateButton(EEventType.CarrierHealthCritical, Mathf.RoundToInt(Mathf.Min(damagePercentage * 200f, 100f)), !criticalDamage, !criticalDamage);
            criticalDamage = true;
            CriticalDamageReached();
        }
    }

    public void HideCriticalDamage()
    {
        if (Ignore)
        {
            return;
        }
        if (criticalDamage)
        {
            criticalDamage = false;
            UpdateButton(EEventType.CarrierHealthCritical, 0, false);
        }
    }

    public void AddCrewInjured(CrewUnit unit)
    {
        if (IsEventDisabled(EEventFlag.CrewInjured))
        {
            return;
        }
        crewsInjured.Add(unit);
        UpdateButton(EEventType.CrewInjured, crewsInjured.Count, true, true);
    }

    public void RemoveCrewInjured(CrewUnit unit)
    {
        if (Ignore)
        {
            return;
        }
        crewsInjured.Remove(unit);
        UpdateButton(EEventType.CrewInjured, crewsInjured.Count, false);
    }

    public void AddCrewDead(CrewUnit unit)
    {
        if (IsEventDisabled(EEventFlag.CrewDead))
        {
            return;
        }
        crewsDead.Add(unit);
        UpdateButton(EEventType.CrewDead, crewsDead.Count, true, true);
    }

    public void RemoveCrewDead(CrewUnit unit)
    {
        if (Ignore)
        {
            return;
        }
        crewsDead.Remove(unit);
        UpdateButton(EEventType.CrewDead, crewsDead.Count, false);
    }

    public void AddSquadronDamaged(SectionSegment culprit)
    {
        if (IsEventDisabled(EEventFlag.SquadronDamaged))
        {
            return;
        }
        if (squadronDamagedCenterSegment == null)
        {
            squadronDamagedCenterSegment = culprit;
        }
        UpdateButton(EEventType.SquadronDamaged, 1, true, true);
    }

    public void AddImmobileCarrier(bool noSpeed)
    {
        if (IsEventDisabled(EEventFlag.CarrierImmobile))
        {
            return;
        }
        UpdateButton(EEventType.CarrierImmobile, 1, true, true, EAudio.Generate, noSpeed ? noSpeedPopupID : noWaypointPopupID);
    }

    public void RemoveImmobileCarrier()
    {
        if (Ignore)
        {
            return;
        }
        UpdateButton(EEventType.CarrierImmobile, 0, false);
    }

    public void AddWreck()
    {
        if (IsEventDisabled(EEventFlag.Wreck))
        {
            return;
        }
        UpdateButton(EEventType.Wreck, 1, true, true);
    }

    public void RemoveWreck()
    {
        if (Ignore)
        {
            return;
        }
        UpdateButton(EEventType.Wreck, 0, false);
    }

    public void AddKamikaze(RadarEnemyData data)
    {
        if (IsEventDisabled(EEventFlag.Kamikaze))
        {
            return;
        }
        kamikazeEnemies.Add(data);

        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            UpdateButton(EEventType.Kamikaze, kamikazeEnemies.Count, true, true);
        }
    }

    public void RemoveKamikaze(RadarEnemyData data)
    {
        if (Ignore)
        {
            return;
        }
        kamikazeEnemies.Remove(data);
        UpdateButton(EEventType.Kamikaze, SectionRoomManager.Instance.GeneratorsAreWorking ? kamikazeEnemies.Count : 0, false);
    }

    public void AddBigGun()
    {
        if (IsEventDisabled(EEventFlag.BigGun))
        {
            return;
        }
        UpdateButton(EEventType.BigGun, 1, true, true);
    }

    public void RemoveBigGun()
    {
        if (Ignore)
        {
            return;
        }
        UpdateButton(EEventType.BigGun, 0, false);
    }

    public void NightTimePopup()
    {
        ShowPopup(nightTitle);
    }
    
    public void WaypointPopup()
    {
        ShowPopup(waypointTitle);
    }

    public void SwitchesPopup()
    {
        ShowPopup(cantChangeSwitches);
    }

    public void LowSuppliesPopup()
    {
        ShowPopup(lowSuppliesTitle);
    }

    public void NoSuppliesPopup()
    {
        ShowPopup(noSuppliesTitle);
    }

    public void RunwayDamagedPopup()
    {
        ShowPopup(runwayDamagedTitle);
    }

    public void MissionStageIncorrectPopup()
    {
        ShowPopup(missionStageIncorrectTitle);
    }

    public void DayTimeIncorrectPopup()
    {
        ShowPopup(dayTimeIncorrectTitle);
    }

    public void DeckModeIncorrectPopup()
    {
        ShowPopup(deckModeIncorrectTitle);
    }

    public void NotEnoughSpaceOnDeckPopup()
    {
        ShowPopup(notEnoughSpaceOnDeckTitle);
    }

    public void LiftDamagedPopup()
    {
        ShowPopup(liftDamagedTitle);
    }

    public void SquadronIsNotOnDeckPopup()
    {
        ShowPopup(squadronIsNotOnDeckTitle);
    }

    public void GeneratorsNotWorkingPopup()
    {
        ShowPopup(generatorsNotWorkingTitle);
    }

    public void CannotGetSquadronPopup()
    {
        ShowPopup(cannotGetSquadronTitle);
    }

    public void OrderCannotBeDonePopup()
    {
        ShowPopup(orderCannotBeDoneTitle);
    }

    public void OrderQueueFullPopup()
    {
        ShowPopup(orderQueueFullTitle);
    }

    public void HeavyDamagePopup()
    {
        ShowPopup(heavyDmgTitle);
    }

    public void HeavyManeuversPopup()
    {
        ShowPopup(heavyManeuversTitle);
    }

    public void SetAllyEventIcon()
    {
        var data = events[(int)EEventType.AllyUnderAttack];
        data.Icon.sprite = data.Sprite = midwaySprite;
    }

    private bool IsEventDisabled(EEventFlag flag)
    {
        return Ignore || disabled || (EventsEnabled & flag) == 0;
    }

    private void UpdateButton(EEventType eventType, int count, bool sound, bool showPopup = false, EAudio audio = EAudio.Generate, string overrideText = "")
    {
        var data = events[(int)eventType];

        if (eventType == EEventType.CarrierHealthCritical)
        {
            data.SetBorderGlow(true);
        }

        if (count == 0)
        {
            data.RectTransform.gameObject.SetActive(false);
        }
        else
        {
            if (sound && setuped && !HudManager.Instance.CinematicPlay)
            {
                BackgroundAudio.Instance.PlayEvent(EMainSceneUI.ShowEvent);
            }

            if (showPopup && setuped && !HudManager.Instance.CinematicPlay)
            {
                ShowEventPopup(new EventQueueData(eventType, audio, data.Sprite, string.IsNullOrWhiteSpace(overrideText) ? data.TextID : overrideText, eventType == EEventType.CarrierHealthCritical));
            }

            data.Text.text = count.ToString() + (eventType == EEventType.CarrierHealthCritical ? "%" : "");
            if (!data.RectTransform.gameObject.activeSelf)
            {
                data.RectTransform.gameObject.SetActive(true);
                data.RectTransform.SetAsFirstSibling();

                // eventPopupAnimator.Play("EventPopUp");
            }
            else
            {
                //var anim = data.RectTransform.GetComponentInChildren<Animator>();
                //if (data.Animator != null)
                //{
                //    data.Animator.gameObject.SetActive(false);
                //    this.StartCoroutineActionAfterFrames(() => data.Animator.gameObject.SetActive(true), 1);
                //}
            }
        }
    }

    private void ZoomToDynamicEvent()
    {
        var cameraMan = CameraManager.Instance;
        cameraMan.SwitchMode(ECameraView.Sections);
        cameraMan.ZoomToSectionSegment(damagedSegments[0]);
        damagedSegments.Add(damagedSegments[0]);
        damagedSegments.RemoveAt(0);
        UpdateButton(EEventType.SegmentDamage, damagedSegments.Count, false);

        UpdateSegmentTooltip();

        EventClicked();
    }

    private void ZoomToSection()
    {
        var cameraMan = CameraManager.Instance;
        cameraMan.SwitchMode(ECameraView.Sections);
        cameraMan.ZoomToSection(shutdownSection[0]);
        shutdownSection.Add(shutdownSection[0]);
        shutdownSection.RemoveAt(0);
        UpdateButton(EEventType.SectionShutdown, shutdownSection.Count, false);

        UpdateShutdownTooltip();

        EventClicked();
    }

    private void HighlightUO()
    {
        HudManager.Instance.ForceSetTacticMap(true);
        TacticManager.Instance.Map.HighlightMarkers(true, EMarkerType.UO);

        EventClicked();
    }

    private void HighlightEnemy()
    {
        HudManager.Instance.ForceSetTacticMap(true);
        TacticManager.Instance.Map.HighlightMarkers(true, EMarkerType.Enemy);

        EventClicked();
    }

    private void ZoomToAttack()
    {
        radar.gameObject.SetActive(true);
        EnemyAttacksManager.Instance.AttackCamera(radarEnemies[0].Data);
        radarEnemies.RemoveAt(0);
        UpdateButton(EEventType.Attack, radarEnemies.Count, false);

        UpdateEnemyTooltip();

        EventClicked();
    }

    private void HighlightSubmarineHunt()
    {
        if (submarineHunts.Count > 0 && SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            missionListExpand.ShowList();
            submarineHunts[0].ButtonMission.Blink(true);
            submarineHunts.RemoveAt(0);
            UpdateButton(EEventType.SubmarineHunt, submarineHunts.Count, false);

            UpdateSubmarineTooltip();

            EventClicked();
        }
    }

    private void HighlightHostileScouts()
    {
        if (hostileScouts.Count > 0 && SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            missionListExpand.ShowList();
            hostileScouts[0].ButtonMission.Blink(true);
            hostileScouts.RemoveAt(0);
            UpdateButton(EEventType.HostileScouts, hostileScouts.Count, false);

            EventClicked();
        }
    }

    private void HighlightAllyCap()
    {
        missionListExpand.ShowList();
        allyMissions[0].ButtonMission.Blink(true);
        allyMissions.RemoveAt(0);
        UpdateButton(EEventType.AllyUnderAttack, allyMissions.Count, false);

        EventClicked();
    }

    private void HighlightDetection()
    {
        //todo highlight?
        savedDetected = EnemyAttacksManager.Instance.IsDetected;
        UpdateButton(EEventType.DetectionChanged, 0, false);

        EventClicked();
    }

    private void OpenEscortPanel()
    {
        HudManager.Instance.OpenEscortPanel();

        EventClicked();
    }

    private void OpenBuffs()
    {
        HudManager.Instance.OpenBuffPanel();
        UpdateButton(EEventType.BuffExpired, 0, false);

        EventClicked();
    }

    private void OpenObjectives()
    {
        ObjectivesManager.Instance.SetShowObjectivesPanel(true);

        EventClicked();
    }

    private void ChangeToSectionsView()
    {
        CameraManager.Instance.SwitchMode(ECameraView.Sections);

        EventClicked();
    }

    private void CrewInjured()
    {
        OpenCrewManagment();
        RemoveCrewInjured(crewsInjured[0]);

        EventClicked();
    }

    private void CrewDead()
    {
        OpenCrewManagment();
        RemoveCrewDead(crewsDead[0]);

        EventClicked();
    }

    private void GoToHangar()
    {
        var cameraMan = CameraManager.Instance;
        cameraMan.SwitchMode(ECameraView.Sections);
        cameraMan.ZoomToSectionSegment(squadronDamagedCenterSegment);
        squadronDamagedCenterSegment = null;
        UpdateButton(EEventType.SquadronDamaged, 0, false);

        EventClicked();
    }

    private void OnCarrierImmobile()
    {
        var hudMan = HudManager.Instance;
        if (hudMan.ShipSpeedup <= 0f && hudMan.CanSetSpeed)
        {
            shipSpeed.Display(false);
        }
        else
        {
            HudManager.Instance.ForceSetTacticMap(true);
        }
        UpdateButton(EEventType.CarrierImmobile, 0, false);

        EventClicked();
    }

    private void GoToWreck()
    {
        var cameraMan = CameraManager.Instance;
        cameraMan.SwitchMode(ECameraView.Sections);

        var deck = AircraftCarrierDeckManager.Instance;
        var wreckType = prevType;
        switch (wreckType)
        {
            case EWreckType.Wreck:
                if (deck.HasKamikazeFront)
                {
                    wreckType = EWreckType.FrontKamikaze;
                }
                else if (deck.HasKamikazeEnd)
                {
                    wreckType = EWreckType.EndKamikaze;
                }
                break;
            case EWreckType.FrontKamikaze:
                if (deck.HasKamikazeEnd)
                {
                    wreckType = EWreckType.EndKamikaze;
                }
                else if (deck.HasWreck)
                {
                    wreckType = EWreckType.Wreck;
                }
                break;
            case EWreckType.EndKamikaze:
                if (deck.HasWreck)
                {
                    wreckType = EWreckType.Wreck;
                }
                else if (deck.HasKamikazeFront)
                {
                    wreckType = EWreckType.FrontKamikaze;
                }
                break;
        }
        prevType = wreckType;
        cameraMan.ZoomToSectionSegment(DamageControlManager.Instance.WreckSection.SubsectionRooms[(int)prevType].Segments[0]);

        EventClicked();
    }

    private void ZoomToKamikaze()
    {
        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            radar.gameObject.SetActive(true);
            EnemyAttacksManager.Instance.AttackCamera(kamikazeEnemies[0].Data);
        }
        kamikazeEnemies.RemoveAt(0);
        UpdateButton(EEventType.Kamikaze, kamikazeEnemies.Count, false);

        EventClicked();
    }

    private void ZoomToBigGun()
    {
        HudManager.Instance.ForceSetTacticMap(true);
        TacticManager.Instance.ShowCannonRange();

        UpdateButton(EEventType.BigGun, 0, false);

        EventClicked();
    }

    private void OpenCrewManagment()
    {
        CrewManager.Instance.SetShow(true);
    }

    private void ShowEventPopup(EventQueueData data)
    {
        if (canEvent)
        {
            canEvent = false;
            InnerShowEventPopup(data);
        }
        else
        {
            bool currentEvent = currentEventPopup != data.Type;
            if (data.Type == EEventType.SegmentDamage || data.Type == EEventType.SectionShutdown)
            {
                if (!currentEvent || currentAudioEvent != data.Audio)
                {
                    if (!customQueueData.ContainsKey(data.Audio))
                    {
                        eventQueue.Enqueue(data.Type);
                        customQueue.Enqueue(data.Audio);
                    }
                    customQueueData[data.Audio] = data;
                }
            }
            else if (!currentEvent)
            {
                if (!queueData.ContainsKey(data.Type))
                {
                    eventQueue.Enqueue(data.Type);
                }
                queueData[data.Type] = data;
            }
        }
    }

    private void InnerShowEventPopup(EventQueueData data)
    {
        eventPopupText.text = LocalizationManager.Instance.GetText(data.Text);
        eventPopupImage.sprite = data.Sprite;

        eventPopupShield.sprite = data.Critical ? criticalPopupShield : defaultPopupShield;

        eventPopup.gameObject.SetActive(false);
        eventPopup.gameObject.SetActive(true);
    }

    private void ShowPopup(string title)
    {
        if (!eventPopupSprites.TryGetValue(title, out var sprite))
        {
            sprite = defaultPopupImage;
        }
        ShowEventPopup(new EventQueueData(EEventType.Count, EAudio.Generate, sprite, title, false));
    }

    private void AddPlaneDescription(ref string text, int count, string planeName)
    {
        if (count > 0)
        {
            text += "\n";
            text += count.ToString();
            text += " ";
            text += planeName;
            if (count > 1)
            {
                text += "s";
            }
        }
    }

    private void UoShowChanged(bool show)
    {
        if (show && !HudManager.Instance.IsTacticMapOpened)
        {
            NewIntel();
        }
    }

    private void OnObjectRevealed(TacticalEnemyShip ship)
    {
        if (!HudManager.Instance.IsTacticMapOpened)
        {
            RevealedEnemy();
        }
    }

    private void OnDetectedChanged(bool detected)
    {
        DetectionChanged(detected);
    }

    private void OnShipSunk()
    {
        SunkShip();
    }

    private void OnBuffExpired()
    {
        BuffFinished();
    }

    private void OnObjectiveChanged(bool show)
    {
        if (show && setuped && !HudManager.Instance.CinematicPlay)
        {
            audioQueue.Queue(EAudio.NewObjective, null);
        }

        ObjectivesChanged();
    }

    private void OnObjectiveFinishing(int id, bool success)
    {
        var objective = ObjectivesManager.Instance.GetObjective(id);
        if (objective.Visible || objective.Category != EObjectiveCategory.None)
        {
            if (setuped && !HudManager.Instance.CinematicPlay)
            {
                audioQueue.Queue(success ? EAudio.ObjectiveCompleted : EAudio.ObjectiveFailed, null);
            }
            ObjectivesChanged();
        }
    }

    private void OnTacticMapOpened(bool opened)
    {
        if (opened)
        {
            newIntel = false;
            UpdateButton(EEventType.NewIntel, 0, false);

            revealedEnemy = false;
            UpdateButton(EEventType.UoIsEnemy, 0, false);
        }
    }

    private void OnEscortPanelOpened()
    {
        sunkShip = 0;
        UpdateButton(EEventType.ShipSunk, 0, false);
    }

    private void OnObjectivesOpened()
    {
        objectivesChanged = false;
        UpdateButton(EEventType.ObjectivesUpdated, 0, false);
    }

    private void OnViewChanged(ECameraView view)
    {
        if (view == ECameraView.Sections)
        {
            UpdateButton(EEventType.CarrierHealthCritical, 0, false);
        }
    }

    private void OnGeneratorsStateChanged(bool active)
    {
        int attacks;
        int submarines;
        int scouts;
        int kamikazes;
        if (SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            attacks = radarEnemies.Count;
            submarines = submarineHunts.Count;
            scouts = hostileScouts.Count;
            kamikazes = kamikazeEnemies.Count;
        }
        else
        {
            attacks = 0;
            submarines = 0;
            scouts = 0;
            kamikazes = 0;
        }
        UpdateButton(EEventType.Attack, attacks, false);
        UpdateButton(EEventType.SubmarineHunt, scouts, false);
        UpdateButton(EEventType.HostileScouts, submarines, false);
        UpdateButton(EEventType.Kamikaze, kamikazes, false);
    }

    private void UpdateSegmentTooltip()
    {
        if (damagedSegments.Count > 0)
        {
            segmentDamagedTooltip.SetTitles(damagedSegments[0].HasAnyDamageWithBroken() ? damagedSegmentID : injuredInSegmentID);
        }
    }

    private void UpdateShutdownTooltip()
    {
        if (shutdownSection.Count > 0)
        {
            sectionShutdownTooltip.SetTitles(shutdownSection[0].DamageTooltipTitleID, shutdownSection[0].DamageTooltipDescID);
        }
    }

    private void UpdateEnemyTooltip()
    {
        if (radarEnemies.Count > 0)
        {
            bool carrier = radarEnemies[0].Data.CurrentTarget == EEnemyAttackTarget.Carrier;
            enemyAttackTooltip.SetTitles(carrier ? enemyTargetCarrierID : enemyTargetStrikeGroupID);
        }
    }

    private void UpdateSubmarineTooltip()
    {
        if (submarineHunts.Count > 0)
        {
            bool carrier = submarineHunts[0].RadarObject.Target == EEnemyAttackTarget.Carrier;
            enemySubmarineTooltip.SetTitles(carrier ? submarineTargetCarrierID : submarineTargetStrikeGroupID);
        }
    }

    private void SaveList(List<TacticalMission> missionList, EEventType type, ref EventSaveData data, List<EventSaveData> list)
    {
        if (missionList.Count > 0)
        {
            data.Type = type;
            data.Params = new List<int>();

            var tacticMan = TacticManager.Instance;
            foreach (var mission in missionList)
            {
                data.Params.Add(tacticMan.IndexOf(mission));
            }
            list.Add(data);
        }
    }

    private void SaveCrew(List<CrewUnit> crew, EEventType type, ref EventSaveData data, List<EventSaveData> list)
    {
        if (crew.Count > 0)
        {
            data.Type = type;
            data.Params = new List<int>();

            var crewManager = CrewManager.Instance;
            foreach (var unit in crew)
            {
                data.Params.Add(crewManager.IndexOf(unit));
            }
            list.Add(data);
        }
    }

    private void EventAudioSynchronized()
    {
        queueBlocked = false;
        var type = eventQueue.Dequeue();
        if (type == EEventType.SectionShutdown || type == EEventType.SegmentDamage)
        {
            InnerShowEventPopup(customQueueData[customQueue.Dequeue()]);
        }
        else
        {
            InnerShowEventPopup(queueData[type]);
            queueData.Remove(type);
        }
    }

    private EAudio EventToAdvisor(EEventType type)
    {
        switch (type)
        {
            case EEventType.NewIntel:
                return EAudio.NewIntel;
            case EEventType.UoIsEnemy:
                return EAudio.EnemySpotted;
            case EEventType.Attack:
                return EAudio.Attack;
            case EEventType.SubmarineHunt:
                return EAudio.SubmarineDetected;
            case EEventType.AllyUnderAttack:
                return EAudio.AllyAttacked;
            case EEventType.DetectionChanged:
                return EAudio.Detected;
            case EEventType.ShipSunk:
                return EAudio.ShipSunk;
            case EEventType.BuffExpired:
                return EAudio.OrderExpired;
            case EEventType.ObjectivesUpdated:
                return EAudio.NewObjective;
            case EEventType.CarrierHealthCritical:
                return EAudio.HealthCritical;
            case EEventType.Wreck:
                return EAudio.PlaneCrashed;

            case EEventType.HostileScouts:
            case EEventType.CrewInjured:
            case EEventType.CrewDead:
            case EEventType.SquadronDamaged:
            case EEventType.CarrierImmobile:
            case EEventType.Kamikaze:
            case EEventType.BigGun:
                return EAudio.OtherEvent;

            case EEventType.SegmentDamage:
            case EEventType.SectionShutdown:
            default:
                Assert.IsTrue(false);
                return EAudio.Generate;
        }
    }

    private void ClearEvents()
    {
        damagedSegments.Clear();
        shutdownSection.Clear();
        radarEnemies.Clear();
        newIntel = false;
        revealedEnemy = false;
        submarineHunts.Clear();
        hostileScouts.Clear();
        attacksImminent.Clear();
        allyMissions.Clear();
        savedDetected = EnemyAttacksManager.Instance.IsDetected;
        sunkShip = 0;
        objectivesChanged = false;
        criticalDamage = false;
        damagePercentage = 1f;
        crewsInjured.Clear();
        crewsDead.Clear();
        squadronDamagedCenterSegment = null;
        kamikazeEnemies.Clear();

        eventQueue.Clear();
        queueData.Clear();

        foreach (var data in events)
        {
            data.RectTransform.gameObject.SetActive(false);
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        worldmap = state;
        time = 0f;
        if (state)
        {
            ClearEvents();
        }
    }
}
