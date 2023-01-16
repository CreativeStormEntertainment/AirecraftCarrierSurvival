using UnityEngine;

public class ExtinguisherHandler : MonoBehaviour
{
    public AnimationClip Clip => clip;

    [SerializeField]
    private AnimationClip clip = null;
    [SerializeField]
    private Animator animator = null;

    [SerializeField]
    private string inClip = null;
    [SerializeField]
    private string outClip = null;

    private int inID;
    private int outID;

    public void Init()
    {
        inID = Animator.StringToHash(inClip);
        outID = Animator.StringToHash(outClip);
    }

    public void SetSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void Play(bool inClip)
    {
        animator.Play(inClip ? inID : outID);
    }
}
