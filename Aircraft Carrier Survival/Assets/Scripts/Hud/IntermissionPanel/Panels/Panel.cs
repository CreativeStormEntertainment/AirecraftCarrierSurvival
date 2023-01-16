using System.Collections.Generic;
using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    [SerializeField]
    protected List<UpgradeControl> controls = null;

    protected ECarrierType currentCarrier;

    public virtual void Setup(NewSaveData data)
    {
        currentCarrier = IntermissionManager.Instance.CurrentCarrier;
    }

    public virtual void SetShow(bool show)
    {
        gameObject.SetActive(show);
    }

    public abstract void Save(NewSaveData data);

    public void Refresh()
    {
        var newCarrier = IntermissionManager.Instance.CurrentCarrier;
        int prevCarrier = -1;
        if (currentCarrier == newCarrier)
        {
            controls.Refresh();
        }
        else
        {
            prevCarrier = (int)currentCarrier;
            currentCarrier = newCarrier;
        }
        InnerRefresh(prevCarrier);
    }

    protected virtual void InnerRefresh(int prevCarrier)
    {
        if (prevCarrier == -1)
        {
            controls.Refresh();
        }
        else
        {
            controls.SetData((int)currentCarrier);
        }
    }
}
