using UnityEngine;

public class FaceToSectionCamera : FaceToCamera
{
    public SpriteRenderer Renderer => spriteRenderer;

    [SerializeField]
    private SpriteRenderer spriteRenderer = null;

    [SerializeField]
    private Transform innerTransf = null;

    private void OnEnable()
    {
        var cameraMan = CameraManager.Instance;
        if (cameraMan != null)
        {
            cameraMan.ViewChanged += OnViewChanged;
            OnViewChanged(cameraMan.CurrentCameraView);
        }
    }

    private void OnDisable()
    {
        OnViewChanged(ECameraView.Blend);
        CameraManager.Instance.ViewChanged -= OnViewChanged;
    }

    private void OnViewChanged(ECameraView view)
    {
        spriteRenderer.enabled = view == ECameraView.Sections;
    }

    public void SetInnerTransform(float z)
    {
        var euler = innerTransf.localEulerAngles;
        euler.z = z;
        innerTransf.localEulerAngles = euler;
    }
}
