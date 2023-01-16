using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Markers : MonoBehaviour, ITickable
{
    public event Action<bool> UoShowChanged = delegate { };

    public Dictionary<TacticalObject, MarkerData> StrikeGroups => strikeGroups;
    public Dictionary<TacticalObject, MarkerData> Outposts => outposts;
    public Dictionary<TacticalObject, MarkerData> Uos => uos;

    public bool DisableDisappear
    {
        get;
        set;
    }

    public string LastReport => lastReport;
    public string HoursAgo => hoursAgo;

    [SerializeField]
    private float baseHoursUpdate = 6f;
    [SerializeField]
    private float uoBaseHoursUpdate = 2f;
    [SerializeField]
    private int maxMultiplierUpdate = 12;
    [SerializeField]
    private float hoursToDisappearUO = 12f;

    [SerializeField]
    private RectTransform parent = null;
    [SerializeField]
    private MarkerPool uoPool = null;
    [SerializeField]
    private MarkerPool strikeGroupPool = null;
    [SerializeField]
    private MarkerPool outpostPool = null;
    [SerializeField]
    private MarkerPool survivorPool = null;

    [SerializeField]
    private Sprite enemyFleet = null;
    [SerializeField]
    private Sprite neutralFleet = null;
    [SerializeField]
    private Sprite friendlyFleet = null;

    [SerializeField]
    private Sprite enemyBase = null;
    [SerializeField]
    private Sprite neutralBase = null;
    [SerializeField]
    private Sprite friendlyBase = null;
    [SerializeField]
    private Sprite specialBase = null;

    [SerializeField]
    private string neutralBaseTooltipText = "neutralBaseTooltipText";
    [SerializeField]
    private string neutralShipTooltipText = "neutralShipTooltipText";
    [SerializeField]
    private string survivorTooltipText = "survivorTooltipText";

    [SerializeField]
    private float hoursToChangeUoTooltip = 6f;
    [SerializeField]
    private string uoSecondTooltipID = "";
    [SerializeField]
    private string uoFirstTooltipID = "UOMarkerTooltip";
    [SerializeField]
    private string lastReportID = "LastReport";
    [SerializeField]
    private string hoursAgoID = "HoursAgo";

    private string secondUoTooltipTitle;
    private string firstUoTooltipTitle;
    private string lastReport;
    private string hoursAgo;

    private Dictionary<TacticalObject, MarkerData> uos;
    private Dictionary<TacticalObject, MarkerData> strikeGroups;
    private Dictionary<TacticalObject, MarkerData> outposts;
    private Dictionary<TacticalObject, MarkerData> survivors;

    private Pool<MarkerData> markerPool;
    private HashSet<int> updateTimes;
    private HashSet<int> uoUpdateTimes;

    private List<TacticalObject> toHide;

    private Dictionary<MarkerData, RectTransform> visibleObjects;

    private float ticksToChangeToTooltip;

    private List<int> targets;

    private void Awake()
    {
        uoPool.Init(parent);
        strikeGroupPool.Init(parent);
        outpostPool.Init(parent);
        survivorPool.Init(parent);

        markerPool = new Pool<MarkerData>();
        markerPool.Init();

        uos = new Dictionary<TacticalObject, MarkerData>();
        strikeGroups = new Dictionary<TacticalObject, MarkerData>();
        outposts = new Dictionary<TacticalObject, MarkerData>();
        survivors = new Dictionary<TacticalObject, MarkerData>();

        toHide = new List<TacticalObject>();

        updateTimes = new HashSet<int>();
        uoUpdateTimes = new HashSet<int>();

        visibleObjects = new Dictionary<MarkerData, RectTransform>();

        targets = new List<int>();
    }

    private void Start()
    {
        TimeManager.Instance.AddTickable(this);
        var locMan = LocalizationManager.Instance;
        neutralBaseTooltipText = locMan.GetText(neutralBaseTooltipText);
        neutralShipTooltipText = locMan.GetText(neutralShipTooltipText);
        survivorTooltipText = locMan.GetText(survivorTooltipText);
        secondUoTooltipTitle = locMan.GetText(uoSecondTooltipID);
        firstUoTooltipTitle = locMan.GetText(uoFirstTooltipID);
        lastReport = locMan.GetText(lastReportID);
        hoursAgo = locMan.GetText(hoursAgoID);

        ticksToChangeToTooltip = hoursToChangeUoTooltip * TimeManager.Instance.TicksForHour;
    }

    public bool LoadData(TacticalObject uo, ref TacticalObjectData data, bool create)
    {
        if (create && uo.Side != ETacticalObjectSide.Friendly)
        {
            ShowUO(uo, false, false);
        }
        Dictionary<TacticalObject, MarkerData> dict = null;
        if (data.Revealed)
        {
            if (uos.ContainsKey(uo) || (create && uo.Side == ETacticalObjectSide.Friendly))
            {
                Show(uo, uo.Type, uo.Side);
            }
            switch (uo.Type)
            {
                case ETacticalObjectType.StrikeGroup:
                    dict = strikeGroups;
                    break;
                case ETacticalObjectType.Outpost:
                    dict = outposts;
                    break;
                case ETacticalObjectType.Survivors:
                    dict = survivors;
                    break;
            }
        }
        else
        {
            Assert.IsFalse(uo.Side == ETacticalObjectSide.Friendly);
            dict = uos;
        }
        if (!dict.TryGetValue(uo, out var markerData))
        {
            return false;
        }

        Assert.IsNotNull(markerData);
        markerData.Transform.anchoredPosition = data.MarkerPosition;
        markerData.MarkerObj.SetIconsAnchor(data.MarkerPosition.Y < 0f);
        markerData.Container.localRotation = Quaternion.Euler(0f, 0f, data.MarkerDirectionZ);

        markerData.SecondTooltipTimer = data.TooltipTimer;
        markerData.UpdateTimer = data.UpdateTimer;
        markerData.HideTimer = data.HideTimer;
        return true;
    }

    public void SaveData(TacticalObject uo, ref TacticalObjectData data, bool survivor)
    {
        if (uos.TryGetValue(uo, out var markerData))
        {
            data.Revealed = false;
        }
        else
        {
            data.Revealed = true;
            if (!strikeGroups.TryGetValue(uo, out markerData))
            {
                try
                {
                    if (survivor)
                    {
                        markerData = survivors[uo];
                    }
                    else
                    {
                        markerData = outposts[uo];
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{uo};{uo.name};{uo.GetType().Name}");
                    Debug.LogException(ex);

                    if (uo.MarkerData == null)
                    {
                        Debug.LogError("No marker data");
                        data.MarkerPosition = uo.RectTransform.anchoredPosition;
                        data.MarkerDirectionZ = uo.RectTransform.localRotation.eulerAngles.z;

                        data.TooltipTimer = 0;
                        data.UpdateTimer = 1e9f;
                        data.HideTimer = 100_000_000;
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        throw ex;
#endif
                        return;
                    }
                    else
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        throw ex;
#endif
                        markerData = uo.MarkerData;
                    }
                }
            }
        }
        data.MarkerPosition = markerData.Transform.anchoredPosition;
        data.MarkerDirectionZ = markerData.Container.localRotation.eulerAngles.z;

        data.TooltipTimer = markerData.SecondTooltipTimer;
        data.UpdateTimer = markerData.UpdateTimer;
        data.HideTimer = markerData.HideTimer;
    }

    public void Tick()
    {
        if (!DisableDisappear)
        {
            foreach (var pair in uos)
            {
                if (pair.Key.Side == ETacticalObjectSide.Neutral && --pair.Value.HideTimer <= 0)
                {
                    toHide.Add(pair.Key);
                }
            }
            foreach (var obj in toHide)
            {
                if (obj is UnidentifiedObject uo)
                {
                    HideUO(obj, out _, out _, out _);
                    uo.Destroy();
                }
                else if (obj is TacticalEnemyShip ship)
                {
                    TacticManager.Instance.Destroy(ship);
                }
                else if (!(obj is SurvivorObject))
                {
                    Assert.IsTrue(false, obj.GetType() + " " + obj.name);
                }
            }
            toHide.Clear();
            foreach (var pair in strikeGroups)
            {
                if (pair.Key.Side == ETacticalObjectSide.Neutral && --pair.Value.HideTimer <= 0)
                {
                    toHide.Add(pair.Key);
                }
            }
            foreach (var obj in toHide)
            {
                if (obj is TacticalEnemyShip ship)
                {
                    TacticManager.Instance.Destroy(ship);
                }
                else
                {
                    Hide(obj, ETacticalObjectType.StrikeGroup, false);
                }
            }
            toHide.Clear();
        }

        CheckUpdate(uos);
        CheckUpdate(strikeGroups);

        foreach (var value in uos.Values)
        {
            value.SecondTooltipTimer++;
            value.MarkerObj.TacticalMapTooltipCaller.SetText(firstUoTooltipTitle + "\n" + lastReport + " " + value.SecondTooltipTimer / TimeManager.Instance.TicksForHour + " " + hoursAgo);
        }

        foreach (var value in strikeGroups.Values)
        {
            if (value.Object != null && value.Object.Side == ETacticalObjectSide.Enemy)
            {
                value.SecondTooltipTimer++;
                value.Highlight.SetOutdatedReportText(value.SecondTooltipTimer > ticksToChangeToTooltip ? value.SecondTooltipTimer : -1);
            }
        }

        foreach (var value in outposts.Values)
        {
            if (value.Object != null && value.Object.Side == ETacticalObjectSide.Enemy)
            {
                value.SecondTooltipTimer++;
                value.Highlight.SetOutdatedReportText(value.SecondTooltipTimer > ticksToChangeToTooltip ? value.SecondTooltipTimer : -1);
            }
        }

        toHide.Clear();

        CheckUpdate(outposts);
        CheckUpdate(survivors);
    }

    public bool IsUO(TacticalObject uo)
    {
        return uos.ContainsKey(uo);
    }

    public bool IsStrikeGroups(TacticalObject uo)
    {
        return strikeGroups.ContainsKey(uo);
    }

    public MarkerData ShowUO(TacticalObject uo, bool setUpdateTimer = true, bool updatePos = true)
    {
        UoShowChanged(true);
        Assert.IsFalse(uo.Side == ETacticalObjectSide.Friendly);
        var data = Show(uos, uoPool, uo, setUpdateTimer);
        if (updatePos)
        {
            UpdatePos(data);
        }
        return data;
    }

    public void HideUO(TacticalObject uo, out float timer, out int hideTimer, out RectTransform markerTrans)
    {
        UoShowChanged(false);
        Hide(uos, uoPool, uo, out timer, out hideTimer, out markerTrans);
    }

    public void ShowSurvivor(TacticalObject survivor, bool setUpdateTimer = true, bool updatePos = true)
    {
        HideUO(survivor, out _, out _, out _);
        if (!survivors.ContainsKey(survivor))
        {
            var data = Show(survivors, survivorPool, survivor, setUpdateTimer);
            if (updatePos)
            {
                UpdatePos(data);
            }
        }
    }

    public void Show(TacticalObject obj, ETacticalObjectType tacticalObjectType, ETacticalObjectSide side)
    {
        float updateValue;
        int hideTime = 100_000_000;
        if (side == ETacticalObjectSide.Friendly)
        {
            updateValue = 1f;
        }
        else
        {
            HideUO(obj, out _, out hideTime, out _);
            updateValue = 1e9f;
        }

        if (obj.InstantUpdate || (obj is TacticalEnemyShip ship && ship.UpdateRealtime))
        {
            updateValue = 1f;
        }

        var group = strikeGroups;
        var pool = strikeGroupPool;
        switch (tacticalObjectType)
        {
            case ETacticalObjectType.StrikeGroup:
                break;
            case ETacticalObjectType.Outpost:
                group = outposts;
                pool = outpostPool;
                break;
            case ETacticalObjectType.Survivors:
                group = survivors;
                pool = survivorPool;
                break;
        }
        var data = Show(group, pool, obj, false);
        data.HideTimer = hideTime;
        ShowObjectives(obj, data);
        visibleObjects[data] = data.Transform;
        data.UpdateTimer = updateValue;
        UpdatePos(data);

        Color color = Color.blue;
        switch (side)
        {
            case ETacticalObjectSide.Friendly:
                color = new Color(33f / 255f, 114f / 255f, 0f);
                break;
            case ETacticalObjectSide.Neutral:
                color = Color.black;
                break;
            case ETacticalObjectSide.Enemy:
                color = new Color(190f / 255f, 0f, 38f / 255f);
                break;
        }
        if (tacticalObjectType == ETacticalObjectType.Survivors)
        {
            color.a = 0f;
        }
        group[obj].Container.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = color;
    }

    public void Reshow(TacticalObject obj)
    {
        if (strikeGroups.TryGetValue(obj, out var data))
        {
            data.UpdateTimer = 1f;
        }
        else if (outposts.TryGetValue(obj, out data))
        {
            data.Highlight.SetImage(GetEnemyIcon(false, obj.Special));
        }
    }

    public void UpdateNeutralFleet(TacticalObject obj)
    {
        if (uos.TryGetValue(obj, out var data) || strikeGroups.TryGetValue(obj, out data))
        {
            data.UpdateTimer = 1f;
        }
    }

    public void UpdateObjectives()
    {
        UpdateObjectives(strikeGroups);
        UpdateObjectives(outposts);
        UpdateObjectives(survivors);
    }

    public void Hide(TacticalObject obj, ETacticalObjectType objectType, bool showUO = true)
    {
        var group = strikeGroups;
        var pool = strikeGroupPool;
        switch (objectType)
        {
            case ETacticalObjectType.StrikeGroup:
                break;
            case ETacticalObjectType.Outpost:
                group = outposts;
                pool = outpostPool;
                break;
            case ETacticalObjectType.Survivors:
                group = survivors;
                pool = survivorPool;
                break;
        }
        if (!group.TryGetValue(obj, out var data))
        {
            return;
        }
        foreach (var objective in data.Objectives)
        {
            objective.SetParent(obj.RectTransform);
        }
        data.Objectives.Clear();
        Hide(group, pool, obj, out float value, out _, out _);
        if (showUO)
        {
            var newData = ShowUO(obj, false, false);
            newData.UpdateTimer = value;
            UpdatePos(newData, obj.RectTransform);
        }
    }

    public void MakeVisible(TacticalObject obj)
    {
        if (uos.TryGetValue(obj, out var data))
        {
            data.Transform.gameObject.SetActive(true);
        }
    }

    public void SetInstantUpdateTimer(TacticalObject obj)
    {
        if (strikeGroups.TryGetValue(obj, out var data) || uos.TryGetValue(obj, out data))
        {
            if (obj.InstantUpdate)
            {
                data.UpdateTimer = 1f;
            }
            else
            {
                SetUpdateTime(data);
            }
        }
    }

    public void SetInteractable(bool interactable)
    {
        uoPool.SetInteractable(interactable);
        strikeGroupPool.SetInteractable(interactable);
        outpostPool.SetInteractable(interactable);
        survivorPool.SetInteractable(interactable);
    }

    public void SetShowAttackRange(bool show)
    {
        strikeGroupPool.SetShowAttackRange(show);
        outpostPool.SetShowAttackRange(show);
    }

    public void UpdateAttackRange(TacticalEnemyShip enemy)
    {
        if (enemy.Side == ETacticalObjectSide.Enemy && (enemy.Type == ETacticalObjectType.Outpost ? outposts : strikeGroups).TryGetValue(enemy, out var data))
        {
            SetAttackRange(data);
        }
    }

    public void SetShowReconRange(bool show)
    {
        strikeGroupPool.SetShowReconRange(show);
        outpostPool.SetShowReconRange(show);
    }

    public void SetMissionRangeTargets(List<int> targets)
    {
        this.targets.Clear();
        this.targets.AddRange(targets);

        foreach (var data in strikeGroups.Values)
        {
            data.MarkerObj.SetMissionRange(data.Highlight.EnemyShip != null && this.targets.Contains(data.Highlight.EnemyShip.Id) ? 100f : 0f);
        }
    }

    public void SetShowMissionRange(bool show)
    {
        strikeGroupPool.SetShowMissionRange(show);
    }

    public void SetShowPath(bool show)
    {
        strikeGroupPool.SetShowPath(show);
    }

    public IEnumerable<KeyValuePair<TacticalObject, MarkerData>> GetUOs()
    {
        foreach (var pair in uos)
        {
            yield return pair;
        }
    }

    private MarkerData Show(Dictionary<TacticalObject, MarkerData> group, MarkerPool pool, TacticalObject obj, bool setUpdateTimer)
    {
        Assert.IsFalse(group.ContainsKey(obj));
        var data = markerPool.Get();
        data.MarkerObj = pool.Get(!obj.Invisible);
        data.MarkerObj.SetAttackRange(0f);
        data.MarkerObj.SetReconRange(0f);
        data.MarkerObj.SetMissionRange(0f);
        data.Transform = data.MarkerObj.RectTransform;
        data.Container = data.MarkerObj.Container;
        data.MissionContainer = data.MarkerObj.MissionContainer;
        data.ObjTransform = obj.RectTransform;
        data.Object = obj;
        data.Object.MarkerData = data;
        if (setUpdateTimer)
        {
            SetUpdateTime(data);
        }

        var caller = data.Container.GetComponent<TacticalMapTooltipCaller>();
        caller.enabled = true;
        data.Highlight = data.Container.GetComponent<MarkerHighlight>();
        data.Highlight.enabled = true;

        bool clearPath = true;
        if (group != uos)
        {
            if (obj.Type == ETacticalObjectType.Survivors)
            {
                data.Highlight.EnemyShip = obj as TacticalEnemyShip;
                data.Highlight.MarkerType = EMarkerType.Friend;
                caller.SetText(survivorTooltipText);
            }
            else if (obj.Side == ETacticalObjectSide.Enemy)
            {
                data.Highlight.EnemyShip = obj as TacticalEnemyShip;
                caller.enabled = false;
                data.Highlight.SetImage(GetEnemyIcon(group == strikeGroups, obj.Special));
                data.Highlight.MarkerType = EMarkerType.Enemy;

                SetAttackRange(data);
                data.MarkerObj.SetReconRange(data.Highlight.EnemyShip.DetectRange);
                data.MarkerObj.SetupPath(data.Highlight.EnemyShip);

                clearPath = false;
            }
            else if (obj.Side == ETacticalObjectSide.Friendly)
            {
                data.Highlight.EnemyShip = obj as TacticalEnemyShip;
                data.Highlight.SetImage(group == strikeGroups ? friendlyFleet : friendlyBase);
                caller.enabled = false;
                data.Highlight.MarkerType = EMarkerType.Friend;
                if (targets.Contains(data.Highlight.EnemyShip.Id))
                {
                    data.MarkerObj.SetMissionRange(100f);
                }
            }
            else if (obj.Side == ETacticalObjectSide.Neutral)
            {
                data.Highlight.EnemyShip = obj as TacticalEnemyShip;
                data.Highlight.SetImage(group == strikeGroups ? neutralFleet : neutralBase);
                caller.SetText(obj.Type == ETacticalObjectType.Outpost ? neutralBaseTooltipText : neutralShipTooltipText);
                data.Highlight.MarkerType = EMarkerType.Neutral;
            }
        }
        else
        {
            data.MagicSprite = ObjectivesManager.Instance.GetMagicSprite(data.MarkerObj.RectTransform);

            data.Highlight.enabled = true;
            data.Highlight.MarkerType = EMarkerType.UO;
            if (data.Object.Type == ETacticalObjectType.Outpost)
            {
                data.MarkerObj.Arrow.SetActive(false);
            }
        }
        if (clearPath)
        {
            data.MarkerObj.SetupPath(null);
        }

        data.Highlight.TacticalObject = obj;
        data.HideTimer = obj.Side == ETacticalObjectSide.Neutral ? Mathf.RoundToInt(TimeManager.Instance.TicksForHour * hoursToDisappearUO) : 100_000_000;

        group[obj] = data;
        return data;
    }

    private void SetAttackRange(MarkerData data)
    {
        foreach (var block in data.Highlight.EnemyShip.Blocks)
        {
            if (!block.Visible || block.Data.ShipType == EEnemyShipType.Airport || block.Data.ShipType == EEnemyShipType.Carrier)
            {
                data.MarkerObj.SetAttackRange(data.Highlight.EnemyShip.AttackRange);
                return;
            }
        }
        data.MarkerObj.SetAttackRange(0f);
    }

    private void Hide(Dictionary<TacticalObject, MarkerData> group, MarkerPool pool, TacticalObject obj, out float timer, out int hideTimer, out RectTransform markerTrans)
    {
        var data = group[obj];

        if (data.MagicSprite != null)
        {
            ObjectivesManager.Instance.ReleaseMagicSprite(data.MagicSprite);
            data.MagicSprite = null;
        }

        data.Highlight = null;
        hideTimer = data.HideTimer;

        visibleObjects.Remove(data);

        pool.Push(data.MarkerObj);
        markerPool.Push(data);

        group.Remove(obj);

        timer = data.UpdateTimer;
        markerTrans = data.Transform;
    }

    private void UpdatePos(MarkerData data)
    {
        UpdatePos(data, data.ObjTransform);
    }

    private void UpdatePos(MarkerData data, RectTransform from)
    {
        data.Transform.anchoredPosition = from.anchoredPosition;
        data.MarkerObj.SetIconsAnchor(from.anchoredPosition.y < 0f);
        data.Container.localRotation = from.localRotation;
        data.SecondTooltipTimer = 0;
    }

    private void SetUpdateTime(MarkerData data)
    {
        int value;
        int ticksForHour = TimeManager.Instance.TicksForHour;
        int time;
        if (uos.ContainsKey(data.Object))
        {
            if (uoUpdateTimes.Count == 0)
            {
                time = Mathf.RoundToInt(uoBaseHoursUpdate * ticksForHour);
                for (int i = 1; i <= maxMultiplierUpdate; i++)
                {
                    uoUpdateTimes.Add(i * time);
                }
            }
            value = RandomUtils.GetRandom(uoUpdateTimes);
            uoUpdateTimes.Remove(value);
        }
        else
        {
            if (updateTimes.Count == 0)
            {
                time = Mathf.RoundToInt(baseHoursUpdate * ticksForHour);
                for (int i = 1; i <= maxMultiplierUpdate; i++)
                {
                    updateTimes.Add(i * time);
                }
            }
            value = RandomUtils.GetRandom(updateTimes);
            updateTimes.Remove(value);
        }
        data.UpdateTimer = value - CrewManager.Instance.DepartmentDict[EDepartments.Navigation].EfficiencyMinutes;
    }

    private void CheckUpdate(Dictionary<TacticalObject, MarkerData> group)
    {
        foreach (var pair in group)
        {
            pair.Value.UpdateTimer -= 1f;
            if (pair.Value.UpdateTimer <= 0f)
            {
                float rest = pair.Value.UpdateTimer;
                UpdatePos(pair.Value);
                var ship = pair.Key as TacticalEnemyShip;
                if (pair.Key.Side == ETacticalObjectSide.Friendly || pair.Key.InstantUpdate || (ship != null && ship.UpdateRealtime))
                {
                    pair.Value.UpdateTimer = 1f;
                    rest = 0f;
                }
                else
                {
                    if (ship == null)
                    {
                        SetUpdateTime(pair.Value);
                    }
                    else
                    {
                        pair.Value.UpdateTimer = 1e9f;
                    }
                }
                pair.Value.UpdateTimer += rest;
            }
        }
    }

    private void UpdateObjectives(Dictionary<TacticalObject, MarkerData> group)
    {
        foreach (var pair in group)
        {
            foreach (var objective in pair.Value.Objectives)
            {
                objective.SetParent(pair.Key.RectTransform);
            }
            pair.Value.Objectives.Clear();

            ShowObjectives(pair.Key, pair.Value);
        }
    }

    private void ShowObjectives(TacticalObject obj, MarkerData data)
    {
        foreach (var objective in obj.Objectives)
        {
            objective.SetParent(data.MissionContainer);
            objective.anchoredPosition = Vector2.zero;
            objective.localRotation = Quaternion.identity;
            data.Objectives.Add(objective);
        }
    }

    private Sprite GetEnemyIcon(bool fleet, bool special)
    {
        if (fleet)
        {
            return enemyFleet;
        }
        else if (special)
        {
            return specialBase;
        }
        else
        {
            return enemyBase;
        }
    }
}
