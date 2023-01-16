using UnityEngine;

public class BindFoamContainer : MonoBehaviour
{
    [SerializeField] private Transform _parentTransform;
    private void Start()
    {
        CameraWaterAndWorld.Instance.AddFoamContainer(transform, _parentTransform);
    }

    private void OnDestroy()
    {
        CameraWaterAndWorld.Instance.RemoveFoamContainer(transform);
    }
}
