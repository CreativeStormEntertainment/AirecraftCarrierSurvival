using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoiInfoPanel : MonoBehaviour, ITickable
{
    [SerializeField]
    private RectTransform rectTransform = null;
    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private GameObject missionInstanceInfo = null;
    [SerializeField]
    private RectTransform poiInRangeTooltip = null;
    [SerializeField]
    private RectTransform poiOutOfRangeTooltip = null;
    [SerializeField]
    private Text poiOutOfRangeText = null;
    [SerializeField]
    private Text poiOutOfRangeDate = null;
    [SerializeField]
    private Image poiTypeImage = null;
    [SerializeField]
    private Text poiTypeText = null;
    [SerializeField]
    private Text nodePositionDesc = null;
    [SerializeField]
    private Text objectiveType = null;
    [SerializeField]
    private Text enemyForces = null;
    [SerializeField]
    private Text date = null;
    [SerializeField]
    private Text duration = null;
    [SerializeField]
    private Text repairSpotDuration = null;
    [SerializeField]
    private Text commandPoints = null;
    [SerializeField]
    private Text upgradePoints = null;
    [SerializeField]
    private Text exp = null;
    [SerializeField]
    private RectTransform objectiveParent = null;
    [SerializeField]
    private List<SimpleObjectiveUi> objectives = null;
    [SerializeField]
    private List<Sprite> poiTypeSprites = null;
    [SerializeField]
    private float timeToHide = 1f;
    [SerializeField]
    private List<GameObject> difficulty = null;

    private float hideTimer;
    private bool init;
    private Vector2 startPos;
    private SandboxPoi sPoi;

    private void Update()
    {
        if (hideTimer > 0f && (hideTimer -= Time.unscaledDeltaTime) <= 0f)
        {
            Hide();
        }
    }

    public void Tick()
    {
        if (sPoi != null)
        {
            SetupTimer(sPoi);
        }
    }

    public void Show(SandboxPoi poi)
    {
        hideTimer = -1f;
        sPoi = poi;
        TimeManager.Instance.RemoveTickable(this);
        TimeManager.Instance.AddTickable(this);
        if (!init)
        {
            startPos = rectTransform.anchoredPosition;
            init = true;
        }
        SetPanelPosition(poi);
        container.SetActive(true);
        bool inRange = poi.InRange;
        poiInRangeTooltip.gameObject.SetActive(inRange);
        poiOutOfRangeTooltip.gameObject.SetActive(!inRange);
        var pos = poi.RectTransform.anchoredPosition + new Vector2(-poiInRangeTooltip.rect.width * 0.6f, 0f);
        pos = pos.x + poiInRangeTooltip.rect.xMin > -Screen.width / 2f ? pos : poi.RectTransform.anchoredPosition + new Vector2(poiInRangeTooltip.rect.width * 0.6f, 0f);
        poiInRangeTooltip.anchoredPosition = poiOutOfRangeTooltip.anchoredPosition = pos;
        var locMan = LocalizationManager.Instance;
        var sandMan = SandboxManager.Instance;
        var worldMap = WorldMap.Instance;
        missionInstanceInfo.SetActive(false);
        objectiveType.gameObject.SetActive(false);
        enemyForces.gameObject.SetActive(false);
        repairSpotDuration.transform.parent.gameObject.SetActive(false);
        if (poi.Data == null)
        {
            nodePositionDesc.text = locMan.GetText("PearlHarborDescription");
            poiTypeImage.sprite = poiTypeSprites[(int)EPoiType.Count];
            poiTypeText.text = locMan.GetText("PearlHarborType");
            return;
        }
        poiTypeImage.sprite = poiTypeSprites[(int)poi.Data.PoiType];
        poiTypeText.text = locMan.GetText(poi.Data.PoiType.ToString());
        if (!(poi is MissionInstancePoi))
        {
            nodePositionDesc.text = locMan.GetText("RepairSpotInfoDesc");
            return;
        }
        objectiveType.gameObject.SetActive(true);
        missionInstanceInfo.SetActive(true);
        enemyForces.gameObject.SetActive(true);
        nodePositionDesc.text = locMan.GetText("Map_" + poi.Data.RegionIndex + "_" + sandMan.SandboxTerritoryManager.GetClosestNode(poi.RectTransform.anchoredPosition).TerritoryType +
            "_" + poi.Data.DescriptionIndex.ToString("00"));
        var objective = worldMap.NodeMaps.NodeDatas[poi.Data.NodeIndex].Maps[poi.Data.MapIndex].Type;
        objectiveType.text = locMan.GetText(objective.ToString() + "_" + poi.Data.ObjectiveDescriptionIndex.ToString("00"));
        var data = worldMap.NodeMaps.NodeDatas[poi.Data.NodeIndex].Maps[poi.Data.MapIndex];
        enemyForces.text = locMan.GetText("EnemyStrength" + data.EnemiesCount + "_" + poi.Data.EnemyForcesDescriptionIndex.ToString("00"));
        commandPoints.text = poi.Data.MissionRewards.CommandPoints.ToString();
        upgradePoints.text = "2";
        exp.text = poi.Data.MissionRewards.AdmiralExp.ToString();
        foreach (var obj in objectives)
        {
            obj.gameObject.SetActive(false);
        }
        using (var enumer = worldMap.SandboxMapSpawner.GetObjectives(objective, worldMap.NodeMaps.NodeDatas[poi.Data.NodeIndex].Maps[poi.Data.MapIndex]).GetEnumerator())
        {
            int i = 0;
            while (enumer.MoveNext())
            {
                var text = enumer.Current;
                enumer.MoveNext();
                objectives[i].Setup(text, enumer.Current, ++i);
            }
        }
        for (int j = 0; j < (int)EMissionDifficulty.VeryHard; j++)
        {
            difficulty[j].SetActive(j == (int)data.Difficulty);
        }
        date.transform.parent.gameObject.SetActive(false);
        duration.transform.parent.gameObject.SetActive(false);
        SetupTimer(sPoi);
    }

    public void SetupTimer(SandboxPoi poi)
    {
        var locMan = LocalizationManager.Instance;
        if (poi.Data == null)
        {
            return;
        }
        poiOutOfRangeDate.gameObject.SetActive(false);
        poiOutOfRangeText.gameObject.SetActive(true);
        if (!(poi is MissionInstancePoi))
        {
            repairSpotDuration.transform.parent.gameObject.SetActive(true);
            int hours = poi.Data.TicksToObsolete / TimeManager.Instance.TicksForHour;
            int days = hours / 24;
            hours %= 24;
            repairSpotDuration.text = locMan.GetText("OptionalObjectiveObsolete") + " " + days + "d " + hours + "h";
            return;
        }
        if (poi.Data.PoiType == EPoiType.MainObjective)
        {
            var mainGoal = SandboxManager.Instance.SandboxGoalsManager.MainGoal;
            date.transform.parent.gameObject.SetActive(true);
            var d = mainGoal.Type == EMainGoalType.PlannedOperations ? poi.Data.PoUnlockDay.Value : mainGoal.UnlockedDay;
            date.text = poiOutOfRangeDate.text = d.Month.ToString("00") + "." + d.Day.ToString("00") + "." + d.Year.ToString();
            bool unlocked = (poi as MainObjectivePOI).Unlocked;
            if (unlocked)
            {
                poiOutOfRangeTooltip.gameObject.SetActive(!poi.InRange);
                poiInRangeTooltip.gameObject.SetActive(poi.InRange);
            }
            else
            {
                poiOutOfRangeTooltip.gameObject.SetActive(true);
                poiInRangeTooltip.gameObject.SetActive(false);
                poiOutOfRangeDate.gameObject.SetActive(true);
                poiOutOfRangeText.gameObject.SetActive(false);
            }
        }
        else
        {
            duration.transform.parent.gameObject.SetActive(true);
            int hours = poi.Data.TicksToObsolete / TimeManager.Instance.TicksForHour;
            int days = hours / 24;
            hours %= 24;
            duration.text = locMan.GetText("OptionalObjectiveObsolete") + " " + days + "d " + hours + "h";
        }
    }

    public void StartHiding()
    {
        hideTimer = timeToHide;
    }

    private void Hide()
    {
        container.SetActive(false);
        poiInRangeTooltip.gameObject.SetActive(false);
        poiOutOfRangeTooltip.gameObject.SetActive(false);
        sPoi = null;
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.DeleteOrder);
        TimeManager.Instance.RemoveTickable(this);
    }

    private void SetPanelPosition(SandboxPoi poi)
    {
        Vector2 pos = startPos;
        if (poi.RectTransform.anchoredPosition.x >= rectTransform.offsetMin.x && poi.RectTransform.anchoredPosition.x <= rectTransform.offsetMax.x &&
            poi.RectTransform.anchoredPosition.y >= rectTransform.offsetMin.y && poi.RectTransform.anchoredPosition.y <= rectTransform.offsetMax.y)
        {
            pos = new Vector2(-startPos.x, startPos.y);
        }
        rectTransform.anchoredPosition = pos;
    }
}
