using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseIdleState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetInteger("STATE") == 0)
        {
            animator.SetInteger("IDLE", Random.Range(1, 11));
        }
    }
}
