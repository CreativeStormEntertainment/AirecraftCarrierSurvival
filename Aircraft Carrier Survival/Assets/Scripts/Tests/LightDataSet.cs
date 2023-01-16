using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightDataSet : MonoBehaviour
{
    private Light _light;

    private void OnEnable()
    {
        // Setup shadow cull distances
        var shadowCullDistances = new float[32];
        shadowCullDistances[30] = 50f;   // Let's imagine this is our 'Tiny Objects' layer
        shadowCullDistances[29] = 50f;  // Let's imagine this is our 'Small Things' layer
        shadowCullDistances[28] = 50f; // Let's imagine this is our 'Trees' layer

        // Assign shadow cull distances. This will only affect layers 10, 11 and 12.
        _light = this.GetComponent<Light>();// = shadowCullDistances;
    }
    private void Update()
    {
        if (_light)
        {
            if (Vector3.SqrMagnitude(Camera.main.transform.position - this.transform.position) > 225f)
            {
                _light.shadows = LightShadows.None;
            }
            else
            {
                _light.shadows = LightShadows.Soft;
            }

        }
    }
    void OnDisable()
    {
        // Completely disable shadow cull distances
        GetComponent<Light>().layerShadowCullDistances = null;
    }
}