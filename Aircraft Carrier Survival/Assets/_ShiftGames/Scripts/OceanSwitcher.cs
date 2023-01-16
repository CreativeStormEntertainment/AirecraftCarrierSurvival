using Crest;
using DG.Tweening;
using UnityEngine;

public class OceanSwitcher : MonoBehaviour
{
    [SerializeField]
    private ShapeGerstnerBatched _shapeGerstnerBatched;
    
    [SerializeField]
    private WaterFloat _oceanWaterFloat;
    
    [SerializeField]
    private Vector3 _offsetPosition = new Vector3(0f, -8f, 0f);

    [SerializeField]
    private float _offsetWeight = -0.25f;
   
    [SerializeField]
    private float _tweenDuration = 0.25f;
    
    private float _defaultWaterWeight;
    private Vector3 _defaultOceanPosition;
    private Tween _tweenUpdateOceanPosition;
    private Tween _tweenUpdateOceanWeight;

    private void Start()
    {
        CameraManager.Instance.ViewChanged += OnViewChanged;

        if (_shapeGerstnerBatched == null)
        {
            Debug.LogError("Field _shapeGerstnerBatched is empty.");
            enabled = false;
            
            return;
        }
        
        if (_oceanWaterFloat == null)
        {
            Debug.LogError("Field _waterFloat is empty.");
            enabled = false;
            
            return;
        }
        
        _defaultWaterWeight = _shapeGerstnerBatched._weight;
        _defaultOceanPosition = _oceanWaterFloat.transform.position;
    }

    private void OnDestroy()
    {
        CameraManager.Instance.ViewChanged -= OnViewChanged;
    }

    private void OnViewChanged(ECameraView view)
    {
        switch (view)
        {
            case ECameraView.Sections:
                DeactivateOcean();
                SetOceanWeight(_defaultWaterWeight + _offsetWeight);
                SetOceanPosition(_defaultOceanPosition + _offsetPosition);
                
                break;
            
            default:
                SetOceanWeight(_defaultWaterWeight);
                SetOceanPosition(_defaultOceanPosition);
                ActivateOcean();
            
                break;       
        }
    }

    private void SetOceanPosition(Vector3 position)
    {
        if (_tweenUpdateOceanPosition != null && _tweenUpdateOceanPosition.active)
        {
            _tweenUpdateOceanPosition.Kill();
        }
        
        _tweenUpdateOceanPosition =
            DOTween.To(
                () => _oceanWaterFloat.transform.position,
                x => _oceanWaterFloat.transform.position = x,
                position,
                _tweenDuration);

        _tweenUpdateOceanPosition.SetAutoKill();
    }

    private void SetOceanWeight(float weight)
    {
        if (_tweenUpdateOceanWeight != null && _tweenUpdateOceanWeight.active)
        {
            _tweenUpdateOceanWeight.Kill();
        }
        
        _tweenUpdateOceanWeight = 
            DOTween.To(
                () => _shapeGerstnerBatched._weight, 
                x => _shapeGerstnerBatched._weight = x, 
                weight, 
                _tweenDuration);
        
        _tweenUpdateOceanWeight.SetAutoKill();
    }
    
    private void ActivateOcean()
    {
        _oceanWaterFloat.enabled = true;
    }
    
    private void DeactivateOcean()
    {
        _oceanWaterFloat.enabled = false;
    }
}
