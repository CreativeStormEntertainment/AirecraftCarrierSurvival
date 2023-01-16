using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BasketRandom
{
    [SerializeField]
    [FormerlySerializedAs("DrawCount")]
    private int drawCount = 0;

    private bool activated;
    private float value;

    public void Init(int overridenDrawCount = -1)
    {
        if (overridenDrawCount != -1)
        {
            drawCount = overridenDrawCount;
        }
        value = drawCount;
    }

    public void LoadData(ref RandomData data)
    {
        value = data.CurrentValue;
        activated = data.Activated;
    }

    public void SaveData(ref RandomData data)
    {
        data.CurrentValue = value;
        data.Activated = activated;
    }

    public void Check(float power, Action action)
    {
        int loopBreak = 0;
        if (Check(power, out float rest))
        {
            action();
            if (rest > 0f)
            {
                if (++loopBreak > 100)
                {
                    return;
                }
                Check(rest, action);
            }
        }
    }

    public bool Check(float power, out float rest)
    {
        value -= power;
        bool result = !activated;
        if (value <= 0f)
        {
            rest = -value;
            value = drawCount;
            activated = false;
            return result;
        }
        else
        {
            rest = 0f;
            activated = activated || (UnityEngine.Random.Range(0, (int)value + 1) == 0);
            return result && activated;
        }
    }
}
