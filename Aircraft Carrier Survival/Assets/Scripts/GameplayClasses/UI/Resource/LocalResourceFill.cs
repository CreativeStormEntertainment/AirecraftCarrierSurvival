using UnityEngine;

public class LocalResourceFill : ResourceFill
{
    [SerializeField]
    private bool IsSupplies = false;

    protected override void Start()
    {
        base.Start();
        if (IsSupplies)
        {
            ResourceManager.Instance.SuppliesAmountChanged += SetFill;
        }
        else
        {
            ResourceManager.Instance.CrewStatusChanged += SetFill;
        }
    }
}
