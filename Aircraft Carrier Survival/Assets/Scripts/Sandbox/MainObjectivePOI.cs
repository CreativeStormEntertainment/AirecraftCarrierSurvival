using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainObjectivePOI : MissionInstancePoi
{
    public bool Unlocked => unlocked;

    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Sprite normalSprite = null;
    [SerializeField]
    private Sprite disabledSprite = null;
    [SerializeField]
    private float blinkTime = 1f;

    private MainGoalData mainGoal;
    private bool unlocked;
    private float timer;
    private int direction = 1;
    private bool finalPoi;

    protected override void Update()
    {
        base.Update();
        if (unlocked)
        {
            timer += Time.unscaledDeltaTime * direction;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, timer / blinkTime);
            if (timer >= blinkTime)
            {
                direction = -1;
            }
            else if (timer <= 0)
            {
                direction = 1;
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.PoiInfoPanel.Show(this);
        BackgroundAudio.Instance.PlayEvent(EButtonState.Hover);
    }

    public override void Setup(SandboxPoiData data)
    {
        var sandMan = SandboxManager.Instance;
        base.Setup(data);
        mainGoal = sandMan.SandboxGoalsManager.MainGoal;
        finalPoi = true;
        if (mainGoal.UnlockedDay.Year == 0)
        {
            var date = TimeManager.Instance.GetDateAfterDays(mainGoal.DaysToFinish);
            mainGoal.UnlockedDay = new DayTime(date.Year, date.Month, date.Day, 0, 0);
        }
        if (data.ObjectiveDescriptionIndex == -1)
        {
            if (mainGoal.Type == EMainGoalType.PlannedOperations)
            {
                data.ObjectiveDescriptionIndex = mainGoal.ObjectiveIdIndexes[mainGoal.SpawnedPOMainPoiIndex];
            }
            else
            {
                data.ObjectiveDescriptionIndex = mainGoal.ObjectiveIdIndexes[0];
            }
        }
        UpdateSprite();
    }

    public override void Load(SandboxPoiData data)
    {
        base.Load(data);
        var timeMan = TimeManager.Instance;
        if (mainGoal.Type == EMainGoalType.PlannedOperations)
        {
            finalPoi = Data.PoUnlockDay.Value.Month == mainGoal.UnlockedDay.Month && Data.PoUnlockDay.Value.Day == mainGoal.UnlockedDay.Day;
            if (data.PoUnlockDay.Value.Month == timeMan.CurrentMonth && data.PoUnlockDay.Value.Day == timeMan.CurrentDay)
            {
                unlocked = true;
                UpdateSprite();
            }
        }
        else
        {
            finalPoi = true;
            if (mainGoal.UnlockedDay.Month == timeMan.CurrentMonth && mainGoal.UnlockedDay.Day == timeMan.CurrentDay)
            {
                unlocked = true;
                UpdateSprite();
            }
        }
    }

    public override bool Tick()
    {
        var timeMan = TimeManager.Instance;
        if (mainGoal.UnlockedDay.Month == timeMan.CurrentMonth && mainGoal.UnlockedDay.Day == timeMan.CurrentDay)
        {
            unlocked = true;
            UpdateSprite();
        }
        else if (unlocked && finalPoi)
        {
            var sandMan = SandboxManager.Instance;
            unlocked = false;
            UpdateSprite();
            sandMan.SandboxGoalsManager.FinishMainGoal();
            sandMan.PoiManager.RemovePoi(this);
            return true;
        }
        if (mainGoal.Type == EMainGoalType.PlannedOperations)
        {
            if (Data.PoUnlockDay.Value.Month == timeMan.CurrentMonth && Data.PoUnlockDay.Value.Day == timeMan.CurrentDay)
            {
                unlocked = true;
                UpdateSprite();
            }
            else if (unlocked)
            {
                SandboxManager.Instance.PoiManager.RemovePoi(this);
                return true;
            }
        }
        return false;
    }

    public override void OnClick()
    {
        Locked = !unlocked;
        base.OnClick();
    }

    public void SetPlannedOperationsDateToActivate(DayTime day)
    {
        Data.PoUnlockDay = day;
        finalPoi = Data.PoUnlockDay.Value.Month == mainGoal.UnlockedDay.Month && Data.PoUnlockDay.Value.Day == mainGoal.UnlockedDay.Day;
    }

    private void UpdateSprite()
    {
        image.sprite = unlocked ? normalSprite : disabledSprite;
    }
}
