using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadarEnemy : MonoBehaviour, IPointerEnterHandler
{
    public Image Image;
    public RectTransform ImageRect;
    public Text Count;
    //public Sprite Icon;
    public RectTransform TransPos;
    public RectTransform TransRot;
    public RectTransform Line;

    public int AttackPower
    {
        get;
        set;
    }

    public EEnemyAttackTarget Target
    {
        get;
        set;
    }

    public SpottedButton SpottedButton
    {
        get;
        set;
    }
    public int ReconIndex
    {
        get;
        set;
    }
    public TacticalMission LinkedMission
    {
        get;
        set;
    }

    [SerializeField]
    private Animator detectionAnimator = null;

    private void Awake()
    {
        TimeManager.Instance.TimeScaleChanged -= OnTimeSpeedChanged;
        TimeManager.Instance.TimeScaleChanged += OnTimeSpeedChanged;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        detectionAnimator.gameObject.SetActive(false);
    }

    public void HighlightEnemy()
	{
        detectionAnimator.gameObject.SetActive(true);
        if(detectionAnimator.isActiveAndEnabled)
        {
            StartCoroutine(DelayedDisableHighlight());
        }
    }

	private IEnumerator DelayedDisableHighlight()
	{
        yield return new WaitForSecondsRealtime(EnemyAttacksManager.Instance.RadarHighlightTime);

        detectionAnimator.gameObject.SetActive(false);
	}

    private void OnTimeSpeedChanged()
    {
        detectionAnimator.speed = Time.timeScale == 0f ? 0f : detectionAnimator.speed;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.TimeScaleChanged -= OnTimeSpeedChanged;
    }
}
