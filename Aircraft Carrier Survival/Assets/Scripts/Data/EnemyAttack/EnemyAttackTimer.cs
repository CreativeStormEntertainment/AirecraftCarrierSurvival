using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class EnemyAttackTimer<T> where T : EnemyAttackBaseData
{
    public List<T> Datas => datas;

    [SerializeField]
    protected List<T> datas;

    private int timer;
    private int index;

    public EnemyAttackTimer()
    {
        datas = new List<T>();
    }

    public void Add(T data)
    {
        datas.Add(data);
        datas.Sort((x, y) => Comparer<int>.Default.Compare(x.Hour * 60 + x.Minute, y.Hour * 60 + y.Minute));
    }

    public virtual void Setup()
    {
        var timeMan = TimeManager.Instance;
        index = 0;
        timer = 0;
        int i = 0;
        foreach (var data in datas)
        {
            //Debug.Log((i++) + " - " + data.Hour + ":" + data.Minute);
            data.DelayInTicks = Mathf.RoundToInt(timeMan.TicksForHour * data.Hour + timeMan.TicksForHour * data.Minute / 60f);
        }
        timer = Mathf.RoundToInt(timeMan.CurrentHour * timeMan.TicksForHour + timeMan.TicksForHour * timeMan.CurrentMinute / 60f);
        do
        {
            if (index >= datas.Count || (timer < datas[index].DelayInTicks))
            {
                break;
            }
            index++;
        }
        while (true);
    }

    public T Tick(int enemyID, bool detected, bool inRange, bool canAttack)
    {
        if (index >= datas.Count)
        {
            return null;
        }

        timer++;
        var data = datas[index];
        if (timer >= data.DelayInTicks)
        {
            if (canAttack)
            {
                Do(data, enemyID, detected, inRange);
            }
            index++;
            return data;
        }
        return null;
    }

    public abstract void Do(T data, int enemyID, bool detected, bool inRange);
}
