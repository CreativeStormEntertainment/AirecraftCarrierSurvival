using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityRandom = UnityEngine.Random;

public class RadarManager : MonoBehaviour, ITickable
{
    public event Action RadarChanged = delegate { };

    public static RadarManager Instance;

    [SerializeField]
    private float flyDistance = 0f;
    [SerializeField]
    private int flyTime = 0;
    [SerializeField]
    private RadarEnemy planePrefab = null;
    [SerializeField]
    private PlayerRadar playerRadar = null;
    [SerializeField]
    private SpottedButton spottedButton = null;
    [SerializeField]
    private SpottedButton spottedSubmarine = null;
    [SerializeField]
    private RectTransform spottedIconsParent = null;
    [SerializeField]
    private RadarEnemy scoutPrefab = null;
    [SerializeField]
    private RadarEnemy submarinePrefab = null;
    [SerializeField]
    private Sprite targetingStrikeGroupImage = null;
    [SerializeField]
    private Sprite targetingCarrierImage = null;
    [SerializeField]
    private Sprite targetingStrikeGroupIcon = null;
    [SerializeField]
    private Sprite targetingCarrierIcon = null;
    [SerializeField]
    private Sprite submarineIcon = null;
    [SerializeField]
    private Sprite submarineIconGlow = null;
    [SerializeField]
    private Sprite kamikazeIcon = null;
    [SerializeField]
    private Sprite kamikazeImage = null;
    [SerializeField]
    private RectTransform strikeGroupRect = null;
    [SerializeField]
    private float lineLengthFix = 0f;
    [SerializeField]
    private float scoutRotationSpeed = 40f;
    [SerializeField]
    private Vector2 scoutOffset = new Vector2(50f, 0f);
    [SerializeField]
    private Vector2 submarineOffset = new Vector2(50f, 50f);

    private HashSet<RadarEnemy> planePool;

    private List<RadarEnemyData> enemiesQueue;

    private List<RadarEnemy> scoutQueue;

    private List<RadarEnemy> submarineQueue;

    private RectTransform spottedIconsParentParent;

    private float randRotation;

    private void Awake()
    {
        Instance = this;
        enemiesQueue = new List<RadarEnemyData>();
        scoutQueue = new List<RadarEnemy>();
        submarineQueue = new List<RadarEnemy>();
        planePool = new HashSet<RadarEnemy>();

        spottedIconsParentParent = spottedIconsParent.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        var enemyAttacksMan = EnemyAttacksManager.Instance;
        enemyAttacksMan.DetectedChanged += OnDetectedChanged;
        OnDetectedChanged(enemyAttacksMan.IsDetected);

        WorldMap.Instance.Toggled += OnWorldMapToggled;
        SectionRoomManager.Instance.GeneratorsStateChanged += OnGeneratorsStateChanged;
    }

    private void Update()
    {
        bool quicker = Parameters.Instance.DifficultyParams.QuickerAttacks;
        for (int i = 0; i < enemiesQueue.Count; i++)
        {
            var timer = enemiesQueue[i].VisualTimer;
            enemiesQueue[i].VisualTimer = Mathf.Clamp01(timer - Time.deltaTime);
            if (quicker)
            {
                timer *= 2f;
            }

            float currentTime = Mathf.Max(enemiesQueue[i].TickTimer, 0f) + timer;
            enemiesQueue[i].Enemy.TransPos.anchoredPosition = enemiesQueue[i].Data.StartPosition - enemiesQueue[i].Data.Direction * (flyDistance * (1f - (currentTime / flyTime)));
            var length = Vector2.Distance(enemiesQueue[i].Enemy.TransPos.anchoredPosition, enemiesQueue[i].Data.CurrentTarget == EEnemyAttackTarget.Carrier ? Vector2.zero : strikeGroupRect.anchoredPosition);
            enemiesQueue[i].Enemy.Line.sizeDelta = new Vector2(length - lineLengthFix, enemiesQueue[i].Enemy.Line.sizeDelta.y);
        }

        for (int index = 0; index < scoutQueue.Count; index++)
        {
            scoutQueue[index].TransPos.Rotate(new Vector3(0f, 0f, scoutRotationSpeed * Time.deltaTime));
            scoutQueue[index].TransRot.rotation = Quaternion.identity;
        }
    }

    public bool HasAnyAttack()
    {
        return enemiesQueue.Count > 0 || scoutQueue.Count > 0 || submarineQueue.Count > 0;
    }

    public void SaveSubmarines(List<SubmarineSaveData> dataList)
    {
        dataList.Clear();
        foreach (var submarine in submarineQueue)
        {
            dataList.Add(new SubmarineSaveData
            {
                Position = submarine.TransPos.anchoredPosition,
                Mission = TacticManager.Instance.IndexOf(submarine.LinkedMission),
                Power = submarine.AttackPower,
                Target = submarine.Target,
            });
        }
    }

    public void LoadSubmarines(List<SubmarineSaveData> dataList)
    {
        var sectionMan = SectionRoomManager.Instance;
        foreach (var data in dataList)
        {
            bool isAttackingStrikeGroup = data.Target == EEnemyAttackTarget.StrikeGroup;
            var enemy = Instantiate(submarinePrefab, submarinePrefab.TransPos.parent);
            enemy.TransPos.anchoredPosition = data.Position;
            submarineQueue.Add(enemy);
            enemy.gameObject.SetActive(true);

            enemy.SpottedButton = Instantiate(spottedSubmarine, spottedIconsParent);
            enemy.SpottedButton.Setup(isAttackingStrikeGroup ? submarineIcon : submarineIconGlow, "", null, null, true);
            enemy.SpottedButton.IsTargetingStrikeGroup = isAttackingStrikeGroup;

            if (!sectionMan.GeneratorsAreWorking)
            {
                enemy.SpottedButton.gameObject.SetActive(false);
            }

            enemy.LinkedMission = TacticManager.Instance.GetMission(EMissionOrderType.SubmarineHunt, data.Mission);
            enemy.AttackPower = data.Power;
            enemy.Target = data.Target;

            enemy.LinkedMission.SetRadarObject(enemy);
            RebuildLayout();
        }
        RadarChanged();
    }

    public void LoadAttack(int time, EnemyAttackData data)
    {
        var enemy = SpawnEnemy(data);
        enemy.TickTimer = time;
        enemy.VisualTimer = 0f;
    }

    public RadarEnemy LoadRecon(int index, float zAngle)
    {
        var scout = SpawnScout(index);
        scout.TransRot.rotation = Quaternion.Euler(0f, 0f, zAngle);
        return scout;
    }

    public void Tick()
    {
        for (int i = 0; i < enemiesQueue.Count;i++)
        {
            var data = enemiesQueue[i];
            data.VisualTimer = 1f;
            data.TickTimer--;
            if (Parameters.Instance.DifficultyParams.QuickerAttacks)
            {
                data.TickTimer--;
            }
            if (!HudManager.Instance.HasNo(ETutorialMode.DisableAttack) && data.TickTimer <= 0f)
            {
                data.VisualTimer = 0f;
                data.TickTimer = 1f;
            }

            if (data.TickTimer <= 0f)
            {
                EnemyAttacksManager.Instance.StartAttack(data.Data);
                RemoveEnemy(i--);
            }
            else if (HudManager.Instance.InWorldMap)
            {
                RemoveEnemy(i--);
            }
        }
    }

    public bool HasEnemyAttack()
    {
        return enemiesQueue.Count > 0;
    }

    public bool HasScout()
    {
        return scoutQueue.Count > 0;
    }

    public bool HasSubmarine()
    {
        return submarineQueue.Count > 0;
    }

    public RadarEnemyData SpawnEnemy(EnemyAttackData data)
    {
        return SpawnEnemy(data, flyTime);
    }

    public RadarEnemyData SpawnEnemy(EnemyAttackData data, int time)
    {
        var radarEnemyData = new RadarEnemyData(data, time);
        if (enemiesQueue.Count == 0)
        {
            TimeManager.Instance.AddTickable(this);
        }
        SetupEnemy(radarEnemyData);
        enemiesQueue.Add(radarEnemyData);
        RadarChanged();

        var eventMan = EventManager.Instance;
        if (data.Kamikaze)
        {
            eventMan.AddKamikaze(radarEnemyData);
        }
        else
        {
            eventMan.AddRadarEnemyData(radarEnemyData);
        }
        return radarEnemyData;
    }

    public RadarEnemy SpawnScout(int reconIndex)
    {
        var enemy = Instantiate(scoutPrefab, scoutPrefab.TransPos.parent);
        scoutQueue.Add(enemy);
        RadarChanged();
        enemy.gameObject.SetActive(true);
        enemy.TransPos.anchoredPosition = Vector2.zero;
        enemy.ImageRect.anchoredPosition = scoutOffset; // * Random.insideUnitCircle.normalized;
        //SetSpawnRotation();
        //enemy.TransPos.Rotate(new Vector3(0f, 0f, randRotation));
        enemy.ReconIndex = reconIndex;
        enemy.SpottedButton = Instantiate(spottedButton, spottedIconsParent);
        enemy.SpottedButton.Setup(targetingCarrierIcon);
        enemy.SpottedButton.ReconIndex = reconIndex;
        //enemy.SpottedButton.ReconIndex = reconIndex;
        enemy.SpottedButton.RadarObject = enemy;

        if (!SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            enemy.SpottedButton.gameObject.SetActive(false);
        }
        return enemy;
    }

    public void SpawnSubmarine(EnemyAttackData data, TacticalMission mission)
    {
        bool isAttackingStrikeGroup = data.CurrentTarget == EEnemyAttackTarget.StrikeGroup;
        var enemy = Instantiate(submarinePrefab, submarinePrefab.TransPos.parent);
        enemy.TransPos.anchoredPosition = new Vector2(submarineOffset.x * UnityRandom.Range(-1f, 1f), submarineOffset.y * UnityRandom.Range(-1f, 1f));
        submarineQueue.Add(enemy);
        enemy.gameObject.SetActive(true);
        RadarChanged();

        enemy.SpottedButton = Instantiate(spottedSubmarine, spottedIconsParent);
        enemy.SpottedButton.Setup(isAttackingStrikeGroup ? submarineIcon : submarineIconGlow, "", null, null, true);
        enemy.SpottedButton.IsTargetingStrikeGroup = isAttackingStrikeGroup;

        if (!SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            enemy.SpottedButton.gameObject.SetActive(false);
        }

        enemy.LinkedMission = mission;
        enemy.AttackPower = data.AttackPower;
        enemy.Target = data.CurrentTarget;

        enemy.LinkedMission.SetRadarObject(enemy);
        RebuildLayout();
    }

    public void RemoveScout(RadarEnemy enemy)
    {
        if (enemy == null)
        {
            return;
        }
        scoutQueue.Remove(enemy);
        RadarChanged();
        Destroy(enemy.SpottedButton.gameObject);
        Destroy(enemy.gameObject);
    }

    public void RemoveSubmarine(bool isTargetingStrikeGroup)
    {
        for (int i = 0; i < submarineQueue.Count; i++)
        {
            if (submarineQueue[i].SpottedButton.IsTargetingStrikeGroup == isTargetingStrikeGroup)
            {
                var enemy = submarineQueue[i];
                Destroy(enemy.SpottedButton.gameObject);
                Destroy(enemy.gameObject);
                submarineQueue.RemoveAt(i);
                RadarChanged();
                return;
            }
        }
    }

    public void RedirectAttack(float hours)
    {
        int index = -1;
        float timer = Mathf.Round(TimeManager.Instance.TicksForHour * hours) + 1;
        for (int i = 0; i < enemiesQueue.Count; i++)
        {
            if (enemiesQueue[i].Data.CurrentTarget == EEnemyAttackTarget.Carrier && enemiesQueue[i].TickTimer < timer)
            {
                index = i;
                timer = enemiesQueue[i].TickTimer;
            }
        }
        if (index != -1)
        {
            var data = enemiesQueue[index];
            data.Data.CurrentTarget = EEnemyAttackTarget.StrikeGroup;

            data.Enemy.SpottedButton.SetIcon(targetingStrikeGroupIcon);

            data.Enemy.Image.sprite = targetingStrikeGroupImage;
            data.Enemy.SpottedButton.Tooltip.ChangeState(ESpottedTooltip.CarrierAttack);
            data.Data.Direction = -(strikeGroupRect.anchoredPosition - data.Enemy.TransPos.anchoredPosition).normalized;
            data.Enemy.Image.rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sign(data.Data.Direction.y) * Vector2.Angle(Vector2.left, -data.Data.Direction));
            data.Enemy.Count.transform.rotation = Quaternion.identity;
        }
    }

    public RadarEnemyData GetEnemy(EnemyAttackData data)
    {
        return enemiesQueue.Find(enemy => enemy.Data == data);
    }

    public EnemyAttackData GetEnemy(RadarEnemyData data)
    {
        return enemiesQueue.Find(enemy => enemy == data).Data;
    }

    public float GetReconAngle(int index)
    {
        return scoutQueue.Find(scout => scout.SpottedButton.ReconIndex == index).TransPos.rotation.eulerAngles.z;
    }

    private void OnDetectedChanged(bool detected)
    {
        playerRadar.SetPlayerDetected(detected);
    }

    private void SetupEnemy(RadarEnemyData data)
    {
        data.Enemy = GetEnemy(planePool, planePrefab);
        data.Enemy.gameObject.SetActive(true);

        Sprite image;
        Sprite icon;

        var power = data.Data.CalculatedAttackPower.ToString();
        data.Enemy.Count.text = power;

        data.Data.StartPosition = data.Enemy.TransPos.anchoredPosition = data.Data.Direction * flyDistance;
        if (data.Data.Kamikaze)
        {
            image = kamikazeImage;
            icon = kamikazeIcon;
        }
        else
        {
            if (data.Data.CurrentTarget == EEnemyAttackTarget.Carrier)
            {
                image = targetingCarrierImage;
                icon = targetingCarrierIcon;
            }
            else
            {
                image = targetingStrikeGroupImage;
                icon = targetingStrikeGroupIcon;
                data.Data.Direction = -(strikeGroupRect.anchoredPosition - data.Enemy.TransPos.anchoredPosition).normalized;
            }
        }

        data.Enemy.Image.rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sign(data.Data.Direction.y) * Vector2.Angle(Vector2.left, -data.Data.Direction));
        data.Enemy.Count.transform.rotation = Quaternion.identity;
        data.Enemy.Count.rectTransform.anchoredPosition = data.Data.Direction.x > 0 ? data.Enemy.Count.rectTransform.anchoredPosition : new Vector2(-data.Enemy.Count.rectTransform.anchoredPosition.x, data.Enemy.Count.rectTransform.anchoredPosition.y);
        data.Enemy.SpottedButton = Instantiate(spottedButton, spottedIconsParent);
        data.Enemy.SpottedButton.Setup(icon, power, data.Enemy.Line, data.Data);

        if (!SectionRoomManager.Instance.GeneratorsAreWorking)
        {
            data.Enemy.SpottedButton.gameObject.SetActive(false);
        }

        data.Enemy.Image.sprite = image;
        RebuildLayout();
    }

    private RadarEnemy GetEnemy(HashSet<RadarEnemy> pool, RadarEnemy prefab)
    {
        RadarEnemy result;
        using (var enumer = pool.GetEnumerator())
        {
            if (enumer.MoveNext())
            {
                result = enumer.Current;
                pool.Remove(result);
            }
            else
            {
                result = Instantiate(prefab, prefab.TransPos.parent);
            }
        }
        return result;
    }

    private void RemoveEnemy(int index)
    {
        var enemy = enemiesQueue[index];

        var eventMan = EventManager.Instance;
        if (enemy.Data.Kamikaze)
        {
            eventMan.RemoveKamikaze(enemy);
        }
        else
        {
            eventMan.RemoveRadarEnemyData(enemy);
        }

        enemy.Enemy.gameObject.SetActive(false);
        enemy.Enemy.SpottedButton.gameObject.SetActive(false);
        planePool.Add(enemy.Enemy);
        enemiesQueue.RemoveAt(index);
        RadarChanged();

        Destroy(enemy.Enemy.SpottedButton.gameObject);

        if (enemiesQueue.Count == 0)
        {
            TimeManager.Instance.RemoveTickable(this);
        }
        RebuildLayout();
    }

    private void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(spottedIconsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(spottedIconsParentParent);
    }

    private void Clear()
    {
        for (int i = 0; i < enemiesQueue.Count; i++)
        {
            RemoveEnemy(i);
        }
        var submarineQueue = new List<RadarEnemy>(this.submarineQueue);
        foreach (var submarine in submarineQueue)
        {
            RemoveSubmarine(submarine.Target == EEnemyAttackTarget.StrikeGroup);
        }
        var scoutQueue = new List<RadarEnemy>(this.scoutQueue);
        foreach (var scout in scoutQueue)
        {
            RemoveScout(scout);
        }
    }

    private void OnWorldMapToggled(bool value)
    {
        if (value)
        {
            Clear();
        }
    }

    private void OnGeneratorsStateChanged(bool active)
    {
        foreach (var data in enemiesQueue)
        {
            SetEnemyButton(data.Enemy, active);
        }
        SetEnemiesButtons(scoutQueue, active);
        SetEnemiesButtons(submarineQueue, active);
    }

    private void SetEnemiesButtons(List<RadarEnemy> list, bool show)
    {
        foreach (var enemy in list)
        {
            SetEnemyButton(enemy, show);
        }
    }

    private void SetEnemyButton(RadarEnemy enemy, bool show)
    {
        enemy.SpottedButton.gameObject.SetActive(show);
    }
}
