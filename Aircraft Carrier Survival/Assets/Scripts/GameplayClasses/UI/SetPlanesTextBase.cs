using System.Collections.Generic;
using UnityEngine;

public class SetPlanesTextBase : MonoBehaviour
{
    [SerializeField]
    protected List<PlaneResourceText> texts = null;

    protected virtual void Start()
    {
        AircraftCarrierDeckManager.Instance.PlaneCountChanged += SetPlanes;
        TacticManager.Instance.MissionLost += SetPlanes;
        SetPlanes();
    }

    public void SetPlanes()
    {
        foreach (var text in texts)
        {
            text.Refresh();
        }
    }
}
