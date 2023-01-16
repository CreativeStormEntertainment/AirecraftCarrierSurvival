using System.Collections.Generic;
using UnityEngine;

public class CameraWaterAndWorld : MonoBehaviour
{
    [SerializeField] private Transform _mainFoamContainer;
    
    public Camera UICamera;
    public Camera SecondaryCamera;
    public float Offset;
    public float Speed; // 0-1
    public float MaxSpeed;


    private Camera cam;
    private Camera camMain;
    private Transform camMainTrans;
    private Transform camTrans;

    public static CameraWaterAndWorld Instance;

    private Dictionary<Transform, Transform> _foamContainers;

    private void Awake()
    {
        Instance = this;

        _foamContainers = new Dictionary<Transform, Transform>();
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        camTrans = transform;

        camMain = Camera.main;
        camMainTrans = camMain.transform;
    }

    public void AddFoamContainer(Transform foamContainer, Transform parentTransform)
    {
        if (_foamContainers.ContainsKey(foamContainer))
            return;

        _foamContainers.Add(foamContainer, parentTransform);
    }

    public void RemoveFoamContainer(Transform foamContainer)
    {
        if (_foamContainers.ContainsKey(foamContainer) == false)
            return;

        _foamContainers.Remove(foamContainer);
    }

    private void OnPreCull()
    {
        if (CameraPositionFixer.Position.HasValue)
        {
            camMainTrans.position = CameraPositionFixer.Position.Value;
            CameraPositionFixer.Position = null;
        }

        if (CameraPositionFixer.Rotation.HasValue)
        {
            camMainTrans.rotation = CameraPositionFixer.Rotation.Value;
            CameraPositionFixer.Rotation = null;
        }
        
        if (cam == null)
            return;
        
        Speed = TacticManager.Instance.Carrier.HasWaypoint 
            ? Mathf.Clamp(HudManager.Instance.ShipSpeedup, 0f, 1f) 
            : 0f;

        var tempOffset = Speed * MaxSpeed * Time.deltaTime;
            
        Offset -= tempOffset;

        if (Offset < -15000f)
        {
            Offset = 0;
        }

        cam.fieldOfView = camMain.fieldOfView;
        UICamera.fieldOfView = cam.fieldOfView;
        SecondaryCamera.fieldOfView = UICamera.fieldOfView;

        var trans = CameraManager.Instance.GetCurrentCamera();
        var posTemp = trans.position;
        if (trans != camMainTrans && CameraManager.Instance.FixAttackCamera)
        {
            posTemp += camMainTrans.position;
            posTemp = new Vector3(posTemp.x / 2f, posTemp.y, posTemp.z / 2f);
        }

        posTemp.z += Offset;
        camTrans.position = posTemp;
        camTrans.rotation = trans.rotation;
        
        UpdateMainFoam(_mainFoamContainer, tempOffset);

        foreach (var foamContainer in _foamContainers.Keys)
        {
            UpdateFoam(foamContainer, _foamContainers[foamContainer]);
        }        
    }

    private void UpdateMainFoam(Transform foamTransform, float tempOffset)
    {
        foamTransform.gameObject.SetActive(CameraManager.Instance.CurrentCameraView != ECameraView.PreviewCamera);
        var position = foamTransform.position;

        foamTransform.position = Offset == 0
            ? position
            : new Vector3(position.x, position.y, position.z - tempOffset);
    }

    private void UpdateFoam(Transform foamTransform, Transform parentTransform)
    {
        foamTransform.gameObject.SetActive(CameraManager.Instance.CurrentCameraView != ECameraView.PreviewCamera);
        var position = parentTransform != null 
            ? parentTransform.position 
            : foamTransform.position;
        
        var rotation = parentTransform != null 
            ? parentTransform.rotation 
            : foamTransform.rotation;

        foamTransform.position = Offset == 0
            ? position
            : new Vector3(position.x, position.y, position.z + _mainFoamContainer.position.z);

        foamTransform.rotation = rotation;
    }
}
