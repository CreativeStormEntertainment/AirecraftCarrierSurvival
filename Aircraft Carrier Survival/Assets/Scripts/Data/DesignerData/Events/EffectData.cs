using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class EffectData
{
    static int FloatLambdas;
    static int IntLambdas;
    static int BitLambdas;
    static List<Func<EffectData, object, float, object>> lambdaList;

    static EffectData()
    {
        lambdaList = new List<Func<EffectData, object, float, object>>();
        FloatLambdas = lambdaList.Count;

        lambdaList.Add((data, value, crewEff) => { return (float)value + data.mod * crewEff; });
        lambdaList.Add((data, value, crewEff) => { return (float)value + data.mod; });
        lambdaList.Add((data, value, crewEff) => { return (float)value - data.mod * crewEff; });
        lambdaList.Add((data, value, crewEff) => { return (float)value - data.mod; });
        lambdaList.Add((data, value, crewEff) => { return (float)value * data.mod * crewEff; });
        lambdaList.Add((data, value, crewEff) => { return (float)value * data.mod; });
        lambdaList.Add((data, value, crewEff) => { return (float)value / data.mod * crewEff; });
        lambdaList.Add((data, value, crewEff) => { return (float)value / data.mod; });
        lambdaList.Add((data, value, crewEff) => { return data.mod; });

        IntLambdas = lambdaList.Count;
        lambdaList.Add((data, value, crewEff) => { return (int)((int)value + data.mod); });
        lambdaList.Add((data, value, crewEff) => { return (int)((int)value - data.mod); });
        lambdaList.Add((data, value, crewEff) => { return Mathf.CeilToInt((int)value * data.mod); });
        lambdaList.Add((data, value, crewEff) => { return Mathf.FloorToInt((int)value / data.mod); });
        lambdaList.Add((data, value, crewEff) => { return (int)data.mod; });

        BitLambdas = lambdaList.Count;
        lambdaList.Add((data, value, crewEff) => { return Enum.ToObject(data.Property.PropertyType, (int)value | (int)data.mod); });
        lambdaList.Add((data, value, crewEff) => { return Enum.ToObject(data.Property.PropertyType, (int)value & ~(int)data.mod); });
    }

#if UNITY_EDITOR
    static class DataOrder
    {
        public const int EffectName = 0;
        public const int ClassName = 1;
        public const int PropertyName = 2;
        public const int Value = 3;
        public const int Operation = 4;
        public const int UseCrewEffectiveness = 5;
        public const int Count = 6;
    }
    public EffectData(string[] data, List<EffectReflectionData> reflections)
    {
        Assert.IsTrue(data.Length == DataOrder.Count);
        EffectName = data[DataOrder.EffectName];

        var className = data[DataOrder.ClassName];
        var reflData = reflections.Find(x => x.ClassName == className);
        if (reflData == null)
        {
            reflData = new EffectReflectionData(className);
            reflections.Add(reflData);
        }
        classID = reflections.IndexOf(reflData);

        var propName = data[DataOrder.PropertyName];
        if (reflData.PropertyNames.Find(x => x == propName) == null)
        {
            reflData.PropertyNames.Add(propName);
        }
        propID = reflData.PropertyNames.IndexOf(propName);

        EOperation operationType;
        bool parsed = Enum.TryParse(data[DataOrder.Operation], out operationType);
        Assert.IsTrue(parsed);
        bool useCrewEff = bool.Parse(data[DataOrder.UseCrewEffectiveness]);

        if (operationType == EOperation.BitwiseOR || operationType == EOperation.InversedBitwiseOR)
        {
            ESubSectionRoomState state;
            parsed = Enum.TryParse(data[DataOrder.Value], out state);
            Assert.IsTrue(parsed, data[DataOrder.Value]);
            mod = (float)state;
        }
        else
        {
            mod = float.Parse(data[DataOrder.Value]);
        }

        int addEff = -1, add = -1, remEff = -1, rem = -1, mulEff = -1, mul = -1, divEff = -1, div = -1, assign = -1, bitOR = -1, bitORInv = -1;
        var type = Type.GetType(className);
        Assert.IsNotNull(type, className);
        var prop = type.GetProperty(propName);
        Assert.IsNotNull(prop, className + "." + propName);
        var propType = Type.GetType(className).GetProperty(propName).PropertyType;
        if (propType == typeof(float))
        {
            Assert.IsFalse(operationType == EOperation.BitwiseOR || operationType == EOperation.InversedBitwiseOR);
            addEff = FloatLambdas;
            add = FloatLambdas + 1;
            remEff = FloatLambdas + 2;
            rem = FloatLambdas + 3;
            mulEff = FloatLambdas + 4;
            mul = FloatLambdas + 5;
            divEff = FloatLambdas + 6;
            div = FloatLambdas + 7;
            assign = FloatLambdas + 8;
        }
        else if (propType == typeof(int))
        {
            Assert.IsFalse(operationType == EOperation.BitwiseOR || operationType == EOperation.InversedBitwiseOR);
            Assert.IsFalse(useCrewEff, EffectName);
            add = IntLambdas;
            rem = IntLambdas + 1;
            mul = IntLambdas + 2;
            div = IntLambdas + 3;
            assign = IntLambdas + 4;
        }
        //else if (propType == typeof(bool))
        //{
        //    Assert.IsTrue(this.mod == 1f);
        //    Assert.IsFalse(useCrewEff);
        //    add = BoolLambdas;
        //    rem = BoolLambdas + 1;
        //    mul = BoolLambdas + 2;
        //    div = BoolLambdas + 3;
        //    assign = BoolLambdas + 4;
        //}
        else
        {
            Assert.IsTrue(propType.IsEnum);
            Assert.IsTrue(operationType == EOperation.BitwiseOR || operationType == EOperation.InversedBitwiseOR);
            Assert.IsFalse(useCrewEff);
            Assert.IsTrue(Enum.IsDefined(propType, (int)mod));
            bitOR = BitLambdas;
            bitORInv = BitLambdas + 1;
        }
        switch (operationType)
        {
            case EOperation.Addition:
                if (useCrewEff)
                {
                    operationID = addEff;
                    operationInvID = remEff;
                }
                else
                {
                    operationID = add;
                    operationInvID = rem;
                }
                break;
            case EOperation.Subtraction:
                if (useCrewEff)
                {
                    operationID = remEff;
                    operationInvID = addEff;
                }
                else
                {
                    operationID = rem;
                    operationInvID = add;
                }
                break;
            case EOperation.Multiplication:
                if (useCrewEff)
                {
                    operationID = mulEff;
                    operationInvID = divEff;
                }
                else
                {
                    operationID = mul;
                    operationInvID = div;
                }
                break;
            case EOperation.Division:
                if (useCrewEff)
                {
                    operationID = divEff;
                    operationInvID = mulEff;
                }
                else
                {
                    operationID = div;
                    operationInvID = mul;
                }
                break;
            case EOperation.Assignment:
                operationID = assign;
                operationInvID = -1;
                break;
            case EOperation.BitwiseOR:
                operationID = bitOR;
                operationInvID = bitORInv;
                break;
            case EOperation.InversedBitwiseOR:
                operationID = bitORInv;
                operationInvID = bitOR;
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }
#endif

    public string EffectName;

    [NonSerialized]
    public PropertyInfo Property;

    [SerializeField]
    private int classID;
    [SerializeField]
    private int propID;
    [SerializeField]
    public float mod;
    [SerializeField]
    private int operationID;
    [SerializeField]
    private int operationInvID;

    [NonSerialized]
    private object affectedObject;
    [NonSerialized]
    private Func<EffectData, object, float, object> operation;
    [NonSerialized]
    private Func<EffectData, object, float, object> operationInv;

    private EffectData() { }

    public void Init(List<EffectReflectionData> reflections)
    {
        var reflData = reflections[classID];
        affectedObject = reflData.Instance;
        Property = reflData.Properties[propID];

        Assert.IsNotNull(lambdaList);
        Assert.IsFalse(operationID == -1);
        operation = lambdaList[operationID];
        if (operationInvID != -1)
        {
            operationInv = lambdaList[operationInvID];
        }
    }

    public void Effect(float crewEff, object affected = null)
    {
        Effect(affected ?? affectedObject, crewEff, operation);
    }

    public void InverseEffect(float crewEff, object affected = null)
    {
        Effect(affected ?? affectedObject, crewEff, operationInv);
    }

    private void Effect(object affected, float crewEff, Func<EffectData, object, float, object> operation)
    {
        Assert.IsNotNull(Property, EffectName);
        Assert.IsNotNull(affected, EffectName + ", " + Property.DeclaringType.Name + "." + Property.Name);
        Property.SetValue(affected, operation(this, Property.GetValue(affected), crewEff));
    }
}