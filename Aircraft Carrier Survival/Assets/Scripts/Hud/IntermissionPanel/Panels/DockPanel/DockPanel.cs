using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DockPanel : Panel
{
    [SerializeField]
    private Button crewButton = null;
    [SerializeField]
    private Button aircraftButton = null;
    [SerializeField]
    private Button carrierButton = null;
    [SerializeField]
    private Button escortButton = null;

    private IntermissionManager intermissionManager;

    private void Start()
    {
        intermissionManager = IntermissionManager.Instance;
        crewButton.onClick.AddListener(() => intermissionManager.CurrentPanel = EIntermissionCategory.Crew);
        aircraftButton.onClick.AddListener(() => intermissionManager.CurrentPanel = EIntermissionCategory.Aircraft);
        carrierButton.onClick.AddListener(() => intermissionManager.CurrentPanel = EIntermissionCategory.Carrier);
        escortButton.onClick.AddListener(() => intermissionManager.CurrentPanel = EIntermissionCategory.Escort);
    }

    public override void Setup(NewSaveData data)
    {
    }

    public override void Save(NewSaveData data)
    {
    }
}
