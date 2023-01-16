using System;
using UnityEngine.Assertions;

[Serializable]
public class SectionCategoryData : EffectsDataWrapper
{
#if UNITY_EDITOR
    static class DataOrder
    {
        public const int Category = 0;
        public const int Effects = 1;
        public const int Count = 2;
    }

    public SectionCategoryData(EffectManager effMan, string[] data)
    {
        Assert.IsTrue(data.Length == DataOrder.Count);
        Category = TSVUtils.ParseEnum<ESectionCategory>(data[DataOrder.Category]);
        EffectIndices = TSVUtils.GetEffectIndices(effMan, data[DataOrder.Effects]);
    }
#endif

    private SectionCategoryData() { }

    public ESectionCategory Category;
    [NonSerialized]
    private int multiplier;
    public int Multiplier
    {
        get => multiplier;
        set
        {
            Assert.IsTrue(UnityEngine.Mathf.Abs(multiplier - value) == 1);
            Assert.IsFalse(value < 0);

            var effMan = EffectManager.Instance;
            if (value > multiplier)
            {
                foreach (var effect in Effects)
                {
                    effMan.OnEffectStart(effect);
                }
            }
            else
            {
                foreach (var effect in Effects)
                {
                    effMan.OnEffectFinish(effect);
                }
            }
            multiplier = value;
        }
    }
}
