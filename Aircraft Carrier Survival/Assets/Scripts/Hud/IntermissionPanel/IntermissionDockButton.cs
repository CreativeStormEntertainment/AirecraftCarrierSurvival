using UnityEngine;

public class IntermissionDockButton : ButtonIntermissionPanel
{
    [SerializeField]
    private GameObject panel = null;

    public override void Deselect()
    {
        base.Deselect();
        panel.SetActive(false);
        CamerasIntermission.Instance.BlendFinished -= OnBlendFinished;
    }

    public override void SetCamera()
    {
        CamerasIntermission.Instance.BlendFinished += OnBlendFinished;
        base.SetCamera();
    }

    private void OnBlendFinished()
    {
        CamerasIntermission.Instance.BlendFinished -= OnBlendFinished;
        panel.SetActive(true);
    }
}
