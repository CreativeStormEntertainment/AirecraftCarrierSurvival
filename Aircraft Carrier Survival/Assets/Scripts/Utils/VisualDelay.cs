using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDelay : MonoBehaviour
{
    Dictionary<EDelayedVisual, Coroutine> coroutines;

    void Start()
    {
        coroutines = new Dictionary<EDelayedVisual, Coroutine>();
    }

    public void StartCoroutine(EDelayedVisual delayed, IEnumerator coroutine)
    {
        StopCoroutine(delayed);
        coroutines[delayed] = StartCoroutine(coroutine);
    }

    public void StopCoroutine(EDelayedVisual delayed)
    {
        if (coroutines.TryGetValue(delayed, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            coroutines.Remove(delayed);
        }
    }
}
