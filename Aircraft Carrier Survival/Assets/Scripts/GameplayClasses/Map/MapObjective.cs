using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapObjective
{
    protected int index;
    protected string name;
    protected string description;
    protected int intValue = 0;
    protected int intValue2 = 0;
    protected int intValue3 = 0;
    protected string stringValue = "";

    public virtual void ActivateObjective()
    {

    }
}
