using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCRandomIdleAnimation : StateMachineBehaviour
{
    private readonly int nextAnimHash = Animator.StringToHash("NextAnim");
    private readonly int currentIdleHash = Animator.StringToHash("CurrentIdle");
    private bool lockChange = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        lockChange = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 1f && !lockChange)
        {
            animator.SetInteger(currentIdleHash, Random.Range(0, 10));
            animator.SetTrigger(nextAnimHash);
            lockChange = true;
        }
    }
}
