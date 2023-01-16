using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPoi : SandboxPoi
{
    public override void Setup(SandboxPoiData data)
    {
        base.Setup(data);
    }

    public override void OnClick()
    {
        base.OnClick();
        SandboxManager.Instance.ShowSandboxQuestPopup(this);
    }
}
