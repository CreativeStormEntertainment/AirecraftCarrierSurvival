using UnityEngine;

public class CarrierWallAnim : MonoBehaviour
{
    private static readonly int PullAnimID = Animator.StringToHash("Pull");
    private static readonly int ReturnAnimID = Animator.StringToHash("Return");

    [SerializeField]
    private Animator animator = null;

    private bool pulling;

    public void SetAnim(bool pull)
    {
        if (pull != pulling)
        {
            pulling = pull;
            float progress = Mathf.Min(1f, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            //animator.speed = 1f;
            animator.Play(pull ? PullAnimID : ReturnAnimID, 0, 1f - progress);
        }
    }

    public void EndAnim()
    {
        //animator.Play(ReturnAnimID, 0, 1f);
    }
}
