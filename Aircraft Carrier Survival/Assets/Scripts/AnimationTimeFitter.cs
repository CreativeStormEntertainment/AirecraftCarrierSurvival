using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTimeFitter : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;

    private void Start()
    {
        TimeManager.Instance.TimeScaleChanged += CheckTimeScale;
        CheckTimeScale();
    }

    private void OnDestroy()
    {
        TimeManager.Instance.TimeScaleChanged -= CheckTimeScale;
    }

    private void CheckTimeScale()
    {
        if (Time.timeScale == 0)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
        }
        else
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
}
