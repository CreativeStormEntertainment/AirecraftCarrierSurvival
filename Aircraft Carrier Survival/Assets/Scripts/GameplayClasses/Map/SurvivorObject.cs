using UnityEngine;

public class SurvivorObject : TacticalObject, ITickable
{
    //public int TicksToFail
    //{
    //    get;
    //    private set;
    //}

    public float PlayerDetectionRange
    {
        get;
        private set;
    }

    public bool PlayerInRange
    {
        get;
        private set;
    }

    public bool Dead
    {
        get;
        private set;
    }

    private float sqrPlayerDetectionRange;
    private TacticalMap tacMap;
    private bool inited;

    public void Init(Vector2 position)
    {
        if (inited)
        {
            return;
        }
        RectTransform.anchoredPosition = position;
        var parameters = Parameters.Instance;
        ChangeType(ETacticalObjectType.Survivors);
        //TicksToFail = Random.Range(parameters.MinHoursToFailSurvivor, parameters.MaxHoursToFailSurvivor) * TimeManager.Instance.TicksForHour;
        PlayerDetectionRange = parameters.PlayerDetectionRange;
        sqrPlayerDetectionRange = PlayerDetectionRange * PlayerDetectionRange;
        tacMap = TacticalMap.Instance;
        TimeManager.Instance.AddTickable(this);
        inited = true;
        Invisible = true;

        UIManager.Instance.SurvivorsRange.sizeDelta = Vector2.one * 2f * PlayerDetectionRange;
    }

    public SurvivorData SaveData()
    {
        var data = new SurvivorData();
        data.Position = RectTransform.anchoredPosition;
        data.Dead = Dead;
        return data;
    }

    public void LoadData(SurvivorData data)
    {
        Init(data.Position);
        if (data.ObjectData.Revealed)
        {
            Invisible = false;
            Visible = true;
        }
        if (data.Dead)
        {
            Die();
        }
    }

    public void Tick()
    {
        if (Visible && tacMap != null)
        {
            //TicksToFail--;
            PlayerInRange = (tacMap.MapShip.Position - RectTransform.anchoredPosition).sqrMagnitude < sqrPlayerDetectionRange;
            //if (TicksToFail < 0)
            //{
            //    var tacMan = TacticManager.Instance;
            //    tacMan.DestroySurvivor(this);
            //    tacMan.FireSurvivorObjectFinished(false);
            //}
        }
        else
        {
            PlayerInRange = false;
        }
    }

    public void Die()
    {
        TimeManager.Instance.RemoveTickable(this);
        Dead = true;
    }
}
