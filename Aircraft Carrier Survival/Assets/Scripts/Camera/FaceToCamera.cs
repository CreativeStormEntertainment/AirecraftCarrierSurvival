using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    public Transform Transform
    {
        get;
        private set;
    }

    [SerializeField]
    private Transform cameraTransform = null;

    protected virtual void Awake()
    {
        Transform = transform;
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    protected virtual void Update()
    {
        Transform.rotation = cameraTransform.rotation;
    }
}
