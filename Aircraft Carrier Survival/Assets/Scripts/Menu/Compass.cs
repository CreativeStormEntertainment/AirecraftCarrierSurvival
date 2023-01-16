using FMODUnity;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public ChooseMainMenuButtons CurrentTarget
    {
        get => currentTarget;
        set
        {
            if (value != null)
            {
                float newOffset = value.WholeOffset;
                if (!enabled && offset != newOffset)
                {
                    currentTarget = null;
                }
                offset = newOffset;
            }
            if (currentTarget != value)
            {
                enabled = true;
                currentTarget = value;
                time = 0f;
                start = anchoredPos.y;

                dest = currentTarget.RectTransform.anchoredPosition.y + offset - MiddleButton.anchoredPosition.y;
                float p = 2f * SlowAfterPercent - 1f;
                total = start + (dest - start) / (p);
                modTime = Mathf.Max(Mathf.Abs(dest - start) / MaxSpeed, 1f) * MoveTime;
                if (currentTarget != null && Mathf.Abs(start - dest) > Mathf.Epsilon)
                {
                    loop.Play();
                }
            }
        }
    }

    public RectTransform RectTransform;
    public RectTransform MiddleButton;
    public float MoveTime = .5f;
    public float SlowAfterPercent = .9f;
    public float MaxSpeed = 200f;

    private float offset;

    [SerializeField]
    private StudioEventEmitter loop = null;

    private float modTime;
    private Vector2 anchoredPos;
    private float time;
    private float start;
    private float dest;
    private float total;

    private ChooseMainMenuButtons currentTarget;

    private void Start()
    {
        anchoredPos = RectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            time += Time.unscaledDeltaTime;
            if (time > modTime)
            {
                anchoredPos.y = dest;
                //currentTarget = null;
                loop.Stop();
                enabled = false;
            }
            else
            {
                float percent = time / modTime;
                if ((percent - SlowAfterPercent) > Mathf.Epsilon)
                {
                    percent = 2f * SlowAfterPercent - percent;
                }
                float pos = (total - start) * percent;
                anchoredPos.y = start + pos;
            }
            RectTransform.anchoredPosition = anchoredPos;
        }
    }
}
