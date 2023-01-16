using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class AllyHudMarker : MonoBehaviour, ITickable
{
    [SerializeField]
    private List<AllyHudMarkerData> datas = null;

    [SerializeField]
    private Image bg = null;

    [SerializeField]
    private Text text = null;

    [SerializeField]
    private Sprite normalSprite = null;

    [SerializeField]
    private Sprite attackedSprite = null;

    [SerializeField]
    private GameObject underAttack = null;

    [SerializeField]
    private float flashTime = .3f;

    [SerializeField]
    private Color health = default;
    [SerializeField]
    private Color dead = default;

    [SerializeField]
    private bool outpost = false;

    private TacticalEnemyShip ally;
    private float time = float.PositiveInfinity;
    private bool flash;

    private void Update()
    {
        time -= Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f;
        if (time < 0f)
        {
            time += flashTime;
            flash = !flash;
            SetState(flash);
        }
    }

    public void Setup(TacticalEnemyShip ally)
    {
        if (outpost != (ally.Type == ETacticalObjectType.Outpost))
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);
        foreach (var data in datas)
        {
            foreach (var bar in data.HealthBars)
            {
                bar.gameObject.SetActive(false);
            }
        }
        this.ally = ally;
        SetState(false);

        text.text = ally.LocalizedName;

        TimeManager.Instance.AddTickable(this);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        TimeManager.Instance.RemoveTickable(this);
    }

    public void Tick()
    {
        foreach (var mission in TacticManager.Instance.Missions[EMissionOrderType.FriendlyFleetCAP])
        {
            if (mission.EnemyAttackFriend != null && mission.EnemyAttackFriend.FriendID == ally.Id && mission.MissionStage < EMissionStage.Launching)
            {
                if (time > 100f)
                {
                    underAttack.SetActive(true);
                    time = 0f;
                    flash = false;
                }
                return;
            }
        }
        if (time < 100f)
        {
            underAttack.SetActive(false);
            time = float.PositiveInfinity;
            flash = false;
        }
        SetState(false);
    }

    private void SetState(bool attack)
    {
        bg.sprite = attack ? attackedSprite : normalSprite;

        int count = ally.Blocks.Count;
        var bars = datas[count - 1].HealthBars;
        int healthyCount = 0;
        for (int i = 0; i < count; i++)
        {
            bars[i].gameObject.SetActive(true);
            if (!ally.Blocks[i].Dead)
            {
                healthyCount++;
            }
        }
        int j = 0;
        for (; j < healthyCount; j++)
        {
            bars[j].color = health;
        }
        for (; j < count; j++)
        {
            bars[j].color = dead;
        }
    }
}
