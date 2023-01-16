using GambitUtils;
using UnityEngine;

public class WreckAnimExplosion : MonoBehaviour
{
    [SerializeField]
    private GameObject explosion = null;
    [SerializeField]
    private GameObject wreck = null;
    [SerializeField]
    private GameObject wreckAfterExplosion = null;
    [SerializeField]
    private Animator animatorAfterExplosion = null;
    [SerializeField]
    private Wreck wreckScript = null;

    [SerializeField]
    private float explosionTime = 1f;

    private void OnDestroy()
    {
        Destroy(explosion);
    }

    public void Crash()
    {
        explosion.transform.SetParent(null, true);
        explosion.SetActive(true);
        this.StartCoroutineActionAfterTime(() => AfterExplosion(false), explosionTime);
    }

    public void Load()
    {
        AfterExplosion(true);
    }

    private void AfterExplosion(bool load)
    {
        wreckAfterExplosion.SetActive(true);
        wreckScript.SetAnimator(animatorAfterExplosion, load);
        wreck.SetActive(false);
    }
}
