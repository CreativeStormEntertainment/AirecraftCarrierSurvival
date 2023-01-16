using UnityEngine;
using UnityEngine.Assertions;

public class DamageRange : MonoBehaviour, ITickable
{
    private RectTransform trans;

    private DamageRangeSaveData data;
    private float sqrRange;

    public bool Active()
    {
        return data.Active;
    }

    public void Show(int power, int hours, float range, int enemyID)
    {
        data.Active = true;
        var tacMan = TacticManager.Instance;
        tacMan.RefreshCannons();
        tacMan.ObjectDestroyed += OnObjectDestroyed;
        //gameObject.SetActive(true);

        data.EnemyID = enemyID;
        var enemy = tacMan.GetShip(enemyID);
        Assert.IsTrue(!enemy.Dead);

        trans = GetComponent<RectTransform>();
        trans.anchoredPosition = enemy.RectTransform.anchoredPosition;
        trans.sizeDelta = new Vector2(2f * range, 2f * range);
        sqrRange = range * range;
        data.Range = range;

        var timeMan = TimeManager.Instance;
        timeMan.AddTickable(this);

        data.Power = power;
        data.TicksToFire = timeMan.TicksForHour * hours;
        data.Timer = 0;
    }

    public void Hide()
    {
        data.Active = false;

        var tacMan = TacticManager.Instance;
        tacMan.ObjectDestroyed -= OnObjectDestroyed;
        tacMan.RefreshCannons();
        //gameObject.SetActive(false);
        TimeManager.Instance.RemoveTickable(this);
    }

    public void LoadData(ref DamageRangeSaveData data)
    {
        if (this.data.Active && !data.Active)
        {
            Hide();
        }
        else if (data.Active)
        {
            Show(data.Power, Mathf.CeilToInt((data.TicksToFire - 1f) / TimeManager.Instance.TicksForHour), data.Range, data.EnemyID);
            this.data = data;
        }
        TacticManager.Instance.RefreshCannons();
    }

    public DamageRangeSaveData SaveData()
    {
        return data;
    }

    public void Tick()
    {
        if (Vector2.SqrMagnitude(TacticManager.Instance.Carrier.Rect.anchoredPosition - trans.anchoredPosition) < sqrRange)
        {
            if (++data.Timer >= data.TicksToFire)
            {
                EventManager.Instance.AddBigGun();
                data.Timer = 0;
                int i = 0;
                foreach (var subsection in SectionRoomManager.Instance.DestroySubsectionDamageSegment(true))
                {
                    if (++i >= data.Power)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            data.Timer = 0;
        }
    }

    private void OnObjectDestroyed(int id, bool _)
    {
        if (data.EnemyID == id)
        {
            TacticManager.Instance.HideDamageRange(id);
        }
    }
}
