using System.Collections;
using UnityEngine;

public class OrderDetailsTooltipCall : MonoBehaviour
{
    public static OrderDetailsTooltipCall Instance;

    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private float delay = 0.5f;

    private readonly int hideTrigger = Animator.StringToHash("Hide");
    private readonly int showTrigger = Animator.StringToHash("Show");
    private readonly int fadeTrigger = Animator.StringToHash("Fade");
    private readonly int orderDetailsAnimBack = Animator.StringToHash("OrderDetailsAnimBack");

    private Coroutine exitRoutine = null;
    private bool faded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator.Play(orderDetailsAnimBack, 0, 1f);
    }

    public void ShowOrderDetailsTooltip()
    {
        if (exitRoutine != null)
        {
            //Debug.Log("Stopped exit routine");
            if (!faded)
            {
                faded = true;
                animator.SetTrigger(fadeTrigger);
                BackgroundAudio.Instance.PlayEvent(EMainSceneUI.BuffHoverChanged);
            }
            StopCoroutine(exitRoutine);
            exitRoutine = null;
        }
        else
        {
            animator.SetTrigger(showTrigger);
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.BuffHoverStart);
        }
        faded = false;
    }

    public void HideOrderDetailsTooltip()
    {
        if (exitRoutine == null)
        {
            exitRoutine = StartCoroutine(CountExitTime());
        }
    }

    public IEnumerator CountExitTime()
    {
        yield return new WaitForSeconds(delay);
        animator.ResetTrigger(showTrigger);
        animator.ResetTrigger(fadeTrigger);
        animator.SetTrigger(hideTrigger);
        BackgroundAudio.Instance.PlayEvent(EMainSceneUI.BuffHoverStop);
        exitRoutine = null;
    }
}
