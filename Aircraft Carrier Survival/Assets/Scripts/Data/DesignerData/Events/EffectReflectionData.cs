using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public class EffectReflectionData
{
    public string ClassName;
    public List<string> PropertyNames = new List<string>();

    [NonSerialized]
    public object Instance;
    [NonSerialized]
    public List<PropertyInfo> Properties = new List<PropertyInfo>();

    public EffectReflectionData(string className)
    {
        ClassName = className;
    }
}
