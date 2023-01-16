using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndJumpState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("JUMP_END", true);
    }
}
