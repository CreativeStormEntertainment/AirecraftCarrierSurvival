using System;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

#endif

using UnityEngine;
using UnityEngine.Assertions;

using UnityRandom = UnityEngine.Random;

public class SectionRoomManager : ParameterEventBase<ESectionUIState>, ITickable
{
#if UNITY_EDITOR

    private static class DataOrder
    {
        public const int SectionName = 0;
        public const int BaseEffects = 1;
        public const int BonusEffects = 2;
        public const int GroupNr = 3;
        public const int RoomNr = 4;
        public const int Count = 5;
    }

    [MenuItem("Tools/Data/Load section category effects", false, 206)]
    private static void LoadSectionCategoryEffects()
    {
        var managers = GameObject.Find("Managers");
        var effMan = managers.GetComponent<EffectManager>();

        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\SectionCategoryEffects.tsv");
        var categories = new List<SectionCategoryData>();
        foreach (var line in lines)
        {
            categories.Add(new SectionCategoryData(effMan, line));
        }

        var roomMan = managers.GetComponent<SectionRoomManager>();
        Undo.RecordObject(roomMan, "Loaded section category effects");
        roomMan.Categories = categories;
        EditorUtility.SetDirty(roomMan);
        EditorSceneManager.MarkSceneDirty(roomMan.gameObject.scene);
    }

    [MenuItem("Tools/Data/Load section effects", false, 207)]
    private static void LoadSectionEffects()
    {
        var managers = GameObject.Find("Managers");
        var effMan = managers.GetComponent<EffectManager>();
        var roomMan = managers.GetComponent<SectionRoomManager>();
        roomMan.SetupRoomList();

        var lines = TSVUtils.LoadData(@"Assets\Data\TSV\SectionEffects.tsv");
        for (int i = 0; i < lines.Count; i++)
        {
            Assert.IsTrue(lines[i].Length == DataOrder.Count);
            int groupID = int.Parse(lines[i][DataOrder.GroupNr]) - 1;
            int roomID = int.Parse(lines[i][DataOrder.RoomNr]) - 1;
            var room = roomMan.RoomsListBySections[groupID][roomID];
            room.BaseEffectsWrapper = new EffectsDataWrapper(TSVUtils.GetEffectIndices(effMan, lines[i][DataOrder.BaseEffects]));
            room.BonusEffectsWrapper = new EffectsDataWrapper(TSVUtils.GetEffectIndices(effMan, lines[i][DataOrder.BonusEffects]));
            PrefabUtility.ApplyPrefabInstance(room.gameObject, InteractionMode.AutomatedAction);
        }
    }

    [MenuItem("Tools/Utils/Update sections center point", false, 202)]
    private static void UpdateSectionsPoint()
    {
        var rooms = FindObjectsOfType<SectionRoom>();
        foreach (var room in rooms)
        {
            var subsections = room.GetComponentsInChildren<SubSectionRoom>(true);
            if (subsections.Length == 3)
            {
                continue;
            }
            Assert.IsTrue(subsections.Length == 2);
            room.ZCenter = 0f;
            foreach (var subsection in subsections)
            {
                room.ZCenter += subsection.GetComponent<MeshRenderer>().bounds.center.z;
            }
            room.ZCenter /= subsections.Length;
            EditorUtility.SetDirty(room);
        }
    }

    [MenuItem("Tools/Utils/Update segments center point", false, 203)]
    private static void UpdateSegmentCenter()
    {
        var segments = FindObjectsOfType<SectionSegment>();
        foreach (var segment in segments)
        {
            segment.Center = segment.Collider.bounds.center;
            segment.Center.x += segment.Collider.bounds.extents.x;
            segment.Center.y += segment.Collider.bounds.extents.y / 2f;
            EditorUtility.SetDirty(segment);
        }
    }

#endif
    public static SectionRoomManager Instance;

    public event Action<bool> GeneratorsStateChanged = delegate { };
    public event Action IssueFixed = delegate { };
    public event Action AllIssuesFixed = delegate { };
    public event Action CarrierDestroyed = delegate { };
    public event Action SectionBrokenChanged = delegate { };

    public List<SectionRoom> Section01Rooms = new List<SectionRoom>();
    public List<SectionRoom> Section02Rooms = new List<SectionRoom>();
    public List<SectionRoom> Section03Rooms = new List<SectionRoom>();
    public List<SectionRoom> Section04Rooms = new List<SectionRoom>();
    public List<List<SectionRoom>> RoomsListBySections = new List<List<SectionRoom>>();

    public List<SectionCategoryData> Categories;
    public SectionCategoryData NoneData;

    [NonSerialized]
    public Dictionary<ESectionCategory, SectionCategoryData> categoriesData;

    [NonSerialized]
    public int TotalSegments;

    public SectionRoom AAGuns;
    public SectionRoom AAGuns2;
    public SectionRoom Pilots;
    public SectionRoom Engines;
    public SectionRoom Deck;
    public SectionRoom AircraftWorkshop;
    public SectionRoom Sickbay;

    //public SectionRoom DC;

    public SectionRoom Meteo;
    public SectionRoom Targeting;
    public SectionRoom Fuel;
    public SectionRoom Comms;
    public SectionRoom Hangar;
    public SectionRoom CrewQuarters;
    public SectionRoom Turbines;
    public SectionRoom Lockers;
    public SectionRoom Workshop;
    public SectionRoom Helm;
    public SectionRoom AmmoSupply;

    public SectionRoom MainGenerators;
    public SectionRoom BackupGenerators;

    public bool GeneratorsAreWorking => true/*MainGenerators.IsWorking || BackupGenerators.IsWorking*/;

    public bool BlockedByIslandBuff
    {
        get;
        set;
    }

    public List<SectionSegment> AllSectionSegments
    {
        get;
        private set;
    }

    public List<SubSectionRoom> AllSubsections
    {
        get;
        private set;
    }

    public List<SubSectionRoom> BrokenSubsections
    {
        get;
        private set;
    } = new List<SubSectionRoom>();

    public bool DisableDangers
    {
        get;
        private set;
    }

    public Material BlinkingMaterial;
    public Material HoverSectionMaterial;

    [SerializeField]
    private float percentDestroyedSectionToFail = .5f;

    [SerializeField]
    private List<SubSectionRoom> lowerDeckSections = null;

    [SerializeField]
    private List<SubSectionRoom> middleDeckSections = null;

    [SerializeField]
    private List<SubSectionRoom> topDeckSections = null;

    private int sectionCount;

    private HashSet<SectionSegmentGroup> groups;

    private int prevDestroyed;

    private List<SectionSegment> segments;
    private List<SectionRoom> sections;

    private HashSet<SubSectionRoom> subsectionsSet;
    private HashSet<SectionSegment> segmentSet1;
    private HashSet<SectionSegment> segmentSet2;
    private HashSet<SectionSegment> segmentSet3;

    private int cannon;

    private List<SectionSegment> segmentList;
    private Queue<SectionSegment> queue;

    private int secretIndex;

    private bool destroyed;

    protected override void Awake()
    {
        prevDestroyed = -1;
        secretIndex = -1;

        base.Awake();

        Assert.IsNull(Instance);
        Instance = this;

        groups = new HashSet<SectionSegmentGroup>();

        SetupRoomList();

        categoriesData = new Dictionary<ESectionCategory, SectionCategoryData>();
        foreach (var category in Categories)
        {
            Assert.IsFalse(categoriesData.ContainsKey(category.Category));
            categoriesData[category.Category] = category;
        }
        Assert.IsFalse(categoriesData.ContainsKey(ESectionCategory.None));
        categoriesData[ESectionCategory.None] = NoneData;

        AllSectionSegments = new List<SectionSegment>(FindObjectsOfType<SectionSegment>());
        AllSubsections = new List<SubSectionRoom>(FindObjectsOfType<SubSectionRoom>());

        var set = new HashSet<SubSectionRoom>();
        foreach (var subsection in lowerDeckSections)
        {
            if (!set.Add(subsection))
            {
                Debug.LogError("Duplicate " + subsection.name + " in lowerDeckSections", subsection);
            }
        }
        foreach (var subsection in middleDeckSections)
        {
            if (!set.Add(subsection))
            {
                Debug.LogError("Duplicate " + subsection.name + " in middleDeckSections", subsection);
            }
        }
        foreach (var subsection in topDeckSections)
        {
            if (!set.Add(subsection))
            {
                Debug.LogError("Duplicate " + subsection.name + " in topDeckSections", subsection);
            }
        }
        foreach (var list in RoomsListBySections)
        {
            foreach (var section in list)
            {
                foreach (var subsection in section.SubsectionRooms)
                {
                    if (!set.Contains(subsection) && (subsection.Segments.Count == 0 || !subsection.Segments[0].Untouchable))
                    {
                        Debug.LogError(subsection.name + " is not added in deck sections", subsection);
                    }
                }
            }
        }
        subsectionsSet = new HashSet<SubSectionRoom>();
        segmentSet1 = new HashSet<SectionSegment>();
        segmentSet2 = new HashSet<SectionSegment>();
        segmentSet3 = new HashSet<SectionSegment>();

        segmentList = new List<SectionSegment>();
        queue = new Queue<SectionSegment>();
    }

    private void Start()
    {
        var effMan = EffectManager.Instance;
        foreach (var category in categoriesData.Values)
        {
            category.GameplayInit(effMan);
        }

        foreach (var list in RoomsListBySections)
        {
            foreach (var section in list)
            {
                section.SectionWorkingChanged += OnSectionWorkingChanged;
            }
            sectionCount += list.Count;
        }
        MainGenerators.SectionWorkingChanged += OnGeneratorsChanged;
        BackupGenerators.SectionWorkingChanged += OnGeneratorsChanged;
        WorldMap.Instance.Toggled += OnWorldMapToggled;

        segments = new List<SectionSegment>(GetAllSegments());
        sections = new List<SectionRoom>(GetAllSections());
    }

    public void Setup(SOTacticMap map)
    {
        DisableDangers = map.Overrides.EnableNoSegmentDangers;

        TotalSegments = 0;
        foreach (var subsection in GetAllSubsections())
        {
            TotalSegments += subsection.Segments.Count;
        }

        foreach (var group in groups)
        {
            group.Setup();
        }

        TimeManager.Instance.AddTickable(this);
    }

    public void PostSetup()
    {
        foreach (var subsection in GetAllSubsections())
        {
            subsection.Setup();
        }
        foreach (var subsection in DamageControlManager.Instance.WreckSection.SubsectionRooms)
        {
            subsection.Setup();
        }

        //DamageControlPanel panel = FindObjectOfType<DamageControlPanel>();
        //panel.Setup();
    }

    public void LoadData(List<SegmentSaveData> segments)
    {
        int i = 0;
        foreach (var segment in GetAllSegments())
        {
            segment.LoadData(segments[i++]);
        }
    }

    public void SaveData(List<SegmentSaveData> segments)
    {
        segments.Clear();
        foreach (var segment in GetAllSegments())
        {
            segments.Add(segment.SaveData());
        }
    }

    public void Tick()
    {
        secretIndex = -1;
        foreach (var segment in GetAllSegments())
        {
            if (segment.HasAnyIssue())
            {
                return;
            }
        }
        AllIssuesFixed();
    }

    public void SetupRoomList()
    {
        RoomsListBySections.Clear();
        RoomsListBySections.Add(Section01Rooms);
        RoomsListBySections.Add(Section02Rooms);
        RoomsListBySections.Add(Section03Rooms);
        RoomsListBySections.Add(Section04Rooms);
    }

    public void ChangeBrokenSections(bool add, SubSectionRoom section)
    {
        if (add)
        {
            BrokenSubsections.Add(section);
        }
        else
        {
            BrokenSubsections.Remove(section);
        }
        SectionBrokenChanged();
    }

    public IEnumerable<SectionRoom> GetAllSections()
    {
        foreach (var sectionGroup in RoomsListBySections)
        {
            foreach (var section in sectionGroup)
            {
                yield return section;
            }
        }
    }

    public IEnumerable<SubSectionRoom> GetAllSubsections()
    {
        foreach (var sectionGroup in RoomsListBySections)
        {
            foreach (var section in sectionGroup)
            {
                foreach (var subsection in section.SubsectionRooms)
                {
                    yield return subsection;
                }
            }
        }
    }

    public IEnumerable<SectionSegment> GetAllSegments()
    {
        foreach (var section in GetAllSections())
        {
            foreach (var segment in section.GetAllSegments(true))
            {
                yield return segment;
            }
        }
    }

    public void RegisterGroup(SectionSegmentGroup group)
    {
        groups.Add(group);
    }

    public void GetFloodableDeckSegments(HashSet<SectionSegment> segments)
    {
        GetFloodableDeckSegments(segments, lowerDeckSections);
        if (segments.Count == 0)
        {
            GetFloodableDeckSegments(segments, middleDeckSections);
            if (segments.Count == 0)
            {
                GetFloodableDeckSegments(segments, topDeckSections);
            }
        }
    }

    public int IndexOf(SectionSegment segment)
    {
        int result = segments.IndexOf(segment);
        Assert.IsFalse(result == -1, 
            $"{gameObject.scene.name}, {segment.name}, {(segment.Parent == null ? "no subsection" : segment.Parent.name)}, {(segment.Parent == null || segment.Parent.ParentSection == null ? "no section" : segment.Parent.ParentSection.name)}");
        return result;
    }

    public int IndexOfSafe(SectionSegment segment)
    {
        int result = segments.IndexOf(segment);
        if (result == -1)
        {
            Debug.LogError(
                $"{gameObject.scene.name}, {segment.name}, {(segment.Parent == null ? "no subsection" : segment.Parent.name)}, {(segment.Parent == null || segment.Parent.ParentSection == null ? "no section" : segment.Parent.ParentSection.name)}");
            for (int i = secretIndex + 1; i < segments.Count; i++)
            {
                if (segments[i].DcCanEnter())
                {
                    secretIndex++;
                    return i;
                }
            }
        }
        return result;
    }

    public int IndexOf(SectionRoom section)
    {
        int result = sections.IndexOf(section);
        Assert.IsFalse(result == -1);
        return result;
    }

    public SectionSegment GetSegment(int index)
    {
        return segments[index];
    }

    public SectionRoom GetSection(int index)
    {
        return sections[index];
    }

    public SectionRoom GetSection(ESections section)
    {
        switch (section)
        {
            case ESections.AA:
                return AAGuns;
            case ESections.Pilots:
                return Pilots;
            case ESections.Engines:
                return Engines;
            case ESections.Deck:
                return Deck;
            case ESections.AircraftWorkshop:
                return AircraftWorkshop;
            case ESections.Sickbay:
                return Sickbay;
            case ESections.Meteo:
                return Meteo;
            case ESections.Targeting:
                return Targeting;
            case ESections.Fuel:
                return Fuel;
            case ESections.Comms:
                return Comms;
            case ESections.Hangar:
                return Hangar;
            case ESections.CrewQuarters:
                return CrewQuarters;
            case ESections.Turbines:
                return Turbines;
            case ESections.Lockers:
                return Lockers;
            case ESections.Workshop:
                return Workshop;
            case ESections.Helm:
                return Helm;
            case ESections.AmmoSupply:
                return AmmoSupply;
            case ESections.MainGenerators:
                return MainGenerators;
            case ESections.BackupGenerators:
                return BackupGenerators;
        }
        return null;
    }

    public void FireIssueFixed()
    {
        IssueFixed();
    }

    public SectionSegment FindEmptySegment(SubSectionRoom outsideOfRoom, SectionSegment segment)
    {
        return CrawlFindSegment(segment, (x) => x.Dc == null && !x.IsFlooded() && x.Parent != outsideOfRoom);
    }

    public SectionSegment FindEmptyOKSegment(SubSectionRoom outsideOfRoom, SectionSegment segment)
    {
        return CrawlFindSegment(segment, (x) => x.Dc == null && !x.HasAnyRepairableIssue() && x.Parent != outsideOfRoom);
    }

    public IEnumerable<SubSectionRoom> DestroySubsectionDamageSegment(bool cannon)
    {
        if (cannon)
        {
            this.cannon = Time.frameCount;
        }
        subsectionsSet.Clear();
        foreach (var subsection in GetAllSubsections())
        {
            if (!subsection.IsBroken && !subsection.Segments[0].Untouchable)
            {
                subsectionsSet.Add(subsection);
            }
        }

        segmentSet2.Clear();
        while (true)
        {
            if (subsectionsSet.Count == 0)
            {
                yield break;
            }
            var subsection = RandomUtils.GetRandom(subsectionsSet);
            subsectionsSet.Remove(subsection);
            subsection.IsBroken = true;

            bool flood = UnityRandom.value > .5f;
            segmentSet1.Clear();
            GetDamagableSegments(subsection, segmentSet1, flood);
            if (flood)
            {
                if (segmentSet2.Count == 0)
                {
                    GetFloodableDeckSegments(segmentSet2);
                }
                segmentSet3.Clear();
                foreach (var segment in segmentSet2)
                {
                    if (segmentSet1.Contains(segment))
                    {
                        segmentSet3.Add(segment);
                    }
                }
                if (segmentSet3.Count > 0)
                {
                    RandomUtils.GetRandom(segmentSet3).MakeFlood(false);
                    continue;
                }
                else
                {
                    GetDamagableSegments(subsection, segmentSet1, false);
                }
            }
            if (segmentSet1.Count > 0)
            {
                RandomUtils.GetRandom(segmentSet1).MakeFire(false);
            }
            yield return subsection;
        }
    }

    private void OnSectionWorkingChanged(bool state)
    {
        int destroyed = 0;
        float all = 0f;
        foreach (var list in RoomsListBySections)
        {
            foreach (var section in list)
            {
                all++;
                if (!section.IsWorking)
                {
                    destroyed++;
                }
            }
        }

        if (destroyed == prevDestroyed)
        {
            return;
        }

        float percent = destroyed / all;
        if (percent >= percentDestroyedSectionToFail)
        {
            if (!this.destroyed)
            {
                this.destroyed = true;
                CarrierDestroyed();
            }
            int damages = 0;
            int fires = 0;
            int floods = 0;
            int brokens = 0;
            foreach (var segment in segments)
            {
                if (segment.Damage.Exists)
                {
                    damages++;
                }
                if (segment.Fire.Exists)
                {
                    fires++;
                }
                if (segment.IsFlooding())
                {
                    floods++;
                }
                if (segment.Parent.IsBroken)
                {
                    brokens++;
                }
            }
            int max = Mathf.Max(damages, fires, floods, brokens);
            EMissionLoseCause loseCause;
            if ((Time.frameCount - cannon) < 3)
            {
                loseCause = EMissionLoseCause.M3_4_DestroyedByCannon;
            }
            else if (max == damages)
            {
                loseCause = EMissionLoseCause.SectionsDamaged;
            }
            else if (max == fires)
            {
                loseCause = EMissionLoseCause.SectionsFired;
            }
            else if (max == floods)
            {
                loseCause = EMissionLoseCause.SectionsFlooded;
            }
            else
            {
                loseCause = EMissionLoseCause.SectionsDestroyed;
            }
            GameStateManager.Instance.ShowMissionSummary(false, loseCause, "");
        }
        else
        {
            EventManager.Instance.SetReachedCriticalDamage(percent);
        }
    }

    private void OnGeneratorsChanged(bool state)
    {
        //GeneratorsStateChanged(GeneratorsAreWorking);
    }

    private void OnWorldMapToggled(bool state)
    {
        if (state)
        {
            foreach (var subsection in AllSubsections)
            {
                subsection.IsBroken = false;
                foreach (var segment in subsection.Segments)
                {
                    segment.RemoveDamage();
                    segment.RemoveFire();
                    segment.KillInjured();
                }
            }
        }
    }

    private void GetFloodableDeckSegments(HashSet<SectionSegment> segments, List<SubSectionRoom> deck)
    {
        segments.Clear();
        foreach (var room in deck)
        {
            foreach (var segment in room.Segments)
            {
                if (!segment.CanPumpWater && !segment.Untouchable && !segment.IsFlooding())
                {
                    segments.Add(segment);
                }
            }
        }
    }

    private void GetDamagableSegments(SubSectionRoom subsection, HashSet<SectionSegment> set, bool flood)
    {
        foreach (var segment in subsection.Segments)
        {
            foreach (var segment2 in segment.NeighboursDirectionDictionary.Keys)
            {
                if (!set.Contains(segment2) && segment2.Parent != subsection && !segment2.Untouchable &&
                    !segment2.IsFlooding() && !(flood ? segment2.CanPumpWater : segment2.Fire.Exists))
                {
                    set.Add(segment2);
                }
            }
        }
    }

    private SectionSegment CrawlFindSegment(SectionSegment segment, Func<SectionSegment, bool> condition)
    {
        segmentSet1.Clear();
        segmentSet1.Add(segment);
        queue.Enqueue(segment);
        do
        {
            if (queue.Count == 0)
            {
                return null;
            }
            segment = queue.Dequeue();
            foreach (var key in segment.NeighboursDirectionDictionary.Keys)
            {
                if (segmentSet1.Add(key))
                {
                    queue.Enqueue(key);
                }
            }
        }
        while (!condition(segment));
        segmentSet1.Clear();
        segmentList.Add(segment);
        foreach (var segment2 in queue)
        {
            if (condition(segment2))
            {
                segmentList.Add(segment2);
            }
        }
        var result = RandomUtils.GetRandom(segmentList);
        segmentList.Clear();
        queue.Clear();
        return result;
    }
}