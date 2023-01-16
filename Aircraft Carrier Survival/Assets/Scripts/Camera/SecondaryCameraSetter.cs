using UnityEngine;

public class SecondaryCameraSetter : MonoBehaviour
{
    private void OnEnable()
    {
        CameraManager.Instance.SetSecondaryCameraPosition(new SecondaryCameraParamsData(transform, true));
    }

    private void OnDisable()
    {
        CameraManager.Instance.SetSecondaryCameraPosition(new SecondaryCameraParamsData(transform, false));
    }
}
