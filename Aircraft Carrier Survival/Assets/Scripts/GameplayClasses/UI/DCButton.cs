using System;
using UnityEngine;

public class DCButton : GameButton
{
    public event Action<DCInstanceGroup> Selected = delegate { };

    public GameObject Outline;

    [SerializeField]
    private SpriteRenderer image = null;
    [SerializeField]
    private GameObject pressed = null;
    private Transform trans;
    private DCInstanceGroup group;

    private PathData pathData;
    private PathData nextPathData;

    private Vector3 normalScale;
    private Vector3 biggerScale;

    private bool originalDisabled;
    private bool selectedDisabled;

    protected override void Awake()
    {
        base.Awake();
        trans = transform;

        Outline.SetActive(false);

        normalScale = trans.localScale;
        biggerScale = normalScale * 1.5f;
    }

    private void Update()
    {
        if (pathData != null)
        {
            float percent = group.SegmentTransitionPercent();
            Vector3 start, stop;
            if (pathData.HelperPos.HasValue)
            {
                if (percent > pathData.Percent)
                {
                    percent = (percent - pathData.Percent) / (1f - pathData.Percent);
                    start = pathData.HelperPos.Value;
                    stop = nextPathData.Pos;
                }
                else
                {
                    percent /= pathData.Percent;
                    start = pathData.Pos;
                    stop = pathData.HelperPos.Value;
                }
            }
            else
            {
                start = pathData.Pos;
                stop = nextPathData.Pos;
            }
            trans.position = Vector3.Lerp(start, stop, percent);
        }
    }

    public override void OnClickEnd(bool success)
    {
        base.OnClickEnd(success);
        pressed.SetActive(false);
        if (!disabled && success)
        {
            Selected(group);
            SectionRoomManager.Instance.PlayEvent(ESectionUIState.DCSelect);
        }
    }

    public override void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        base.SetEnable(enable);
        pressed.SetActive(false);
    }

    public void Setup(DCInstanceGroup group)
    {
        this.group = group;
    }

    public void SetupIcon(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void UpdateSegment()
    {
        if (group.NextSegmentIndex > 0 && group.Path.Count > group.NextSegmentIndex)
        {
            pathData = group.PathPos[group.NextSegmentIndex - 1];
            nextPathData = group.PathPos[group.NextSegmentIndex];
            trans.position = pathData.Pos;
        }
        else
        {
            pathData = null;
            trans.position = group.CurrentSegment.Center;
        }
    }

    public void SetSelected(bool select)
    {
        Outline.SetActive(select);
        pressed.SetActive(false);
        trans.localScale = select ? biggerScale : normalScale;
    }

    public override void OnClickStart()
    {
        if (!disabled)
        {
            //base.OnClickStart();
            pressed.SetActive(true);
        }
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedDisabled = !enable;
        SetEnable(!originalDisabled);
    }
}
