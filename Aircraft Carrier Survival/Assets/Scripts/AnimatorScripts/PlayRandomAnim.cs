using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomAnim : StateMachineBehaviour
{
    private readonly int animation1 = Animator.StringToHash("animation1");
    private readonly int animation2 = Animator.StringToHash("animation2");
    private readonly int animation3 = Animator.StringToHash("animation3");

    private List<int> animations;

    private void Awake()
    {
        animations = new List<int>();
        animations.Add(animation1);
        animations.Add(animation2);
        animations.Add(animation3);
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger(animations[Random.Range(1, 3)]);
    }
}
