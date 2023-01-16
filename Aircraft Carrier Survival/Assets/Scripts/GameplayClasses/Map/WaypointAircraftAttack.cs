using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointAircraftAttack : MonoBehaviour
{
    private Animator animator = null;

    private readonly int animationVariantHash = Animator.StringToHash("AnimationVariant");
    private readonly int animationTriggerHash = Animator.StringToHash("AnimationTrigger");

    private Coroutine coroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        coroutine = StartCoroutine(ChangeAnimation());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    private IEnumerator ChangeAnimation()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(3f, 6f));
            animator.SetInteger(animationVariantHash, Random.Range(0, 3));
            animator.SetTrigger(animationTriggerHash);
        }
    }
}
