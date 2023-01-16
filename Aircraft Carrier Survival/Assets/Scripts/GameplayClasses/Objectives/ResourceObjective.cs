using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObjective : MapObjective
{
    public EResourceType resourceType;

    public ResourceObjective(int intValue, string stringValue, EResourceType resourceType)
    {
        this.intValue = intValue;
        this.stringValue = stringValue;
        this.resourceType = resourceType;
    }

    public override void ActivateObjective()
    {
        switch (resourceType)
        {
            case EResourceType.PLANES:
                //AircraftCarrierDeckManager.Instance.AddPlanes(intValue, stringValue);
                break;
            case EResourceType.TOOLS:
                LocalResourceManager.Instance.ToolsCurrentAmount += intValue;
                break;
            case EResourceType.AMMO:
                LocalResourceManager.Instance.AmmoCurrentAmount += intValue;
                break;
        }
    }
}
