using UnityEngine;

public class WreckTest : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private GameObject fallParticles = null;

    private void Awake()
    {
        animator.SetBool("Drop", true);
        fallParticles.SetActive(true);
    }
}
