using GambitUtils;
using System;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    public void StartCoroutine(Action action)
    {
        StopCoroutine();
        this.StartCoroutineActionAfterTime(action, 1f);
    }

    public void StopCoroutine()
    {
        StopAllCoroutines();
    }
}
