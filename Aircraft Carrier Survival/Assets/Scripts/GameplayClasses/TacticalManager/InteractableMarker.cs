using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableMarker : MonoBehaviour
{
    public TacticalMapTooltipCaller TacticalMapTooltipCaller => tacticalMapTooltipCaller;
    public RectTransform RectTransform => rectTransform;
    public RectTransform Container => container;
    public RectTransform MissionContainer => missionContainer;
    public GameObject Arrow => arrow;

    [SerializeField]
    private Image raycastTarget = null;

    [SerializeField]
    private RectTransform rectTransform = null;

    [SerializeField]
    private RectTransform container = null;

    [SerializeField]
    private RectTransform missionContainer = null;

    [SerializeField]
    private GameObject arrow = null;

    [SerializeField]
    private TacticalMapTooltipCaller tacticalMapTooltipCaller = null;

    [SerializeField]
    private RectTransform attackRange = null;

    [SerializeField]
    private RectTransform attackRangeIcon = null;

    [SerializeField]
    private Image attackRangeImage = null;

    [SerializeField]
    private Sprite smallAttackRange = null;

    [SerializeField]
    private Sprite mediumAttackRange = null;

    [SerializeField]
    private Sprite largeAttackRange = null;

    [SerializeField]
    private RectTransform reconRange = null;

    [SerializeField]
    private RectTransform reconRangeIcon = null;

    [SerializeField]
    private Image reconRangeImage = null;

    [SerializeField]
    private Sprite extraSmallReconRange = null;

    [SerializeField]
    private Sprite smallReconRange = null;

    [SerializeField]
    private Sprite mediumReconRange = null;

    [SerializeField]
    private Sprite largeReconRange = null;

    [SerializeField]
    private RectTransform missionRange = null;

    [SerializeField]
    private RectTransform pathTrans = null;

    private bool iconsUp;
    private bool showAttack;
    private bool showRecon;
    private bool showMission;
    private bool showPath;
    private int currentSetup;
    private int setup;
    private List<RectTransform> list = new List<RectTransform>();
    private TacticalEnemyShip ship;

    private void OnEnable()
    {
        foreach (var obj in list)
        {
            obj.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        foreach (var obj in list)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public void SetInteractable(bool interactable)
    {
        raycastTarget.raycastTarget = interactable;
    }

    public void SetAttackRange(float range)
    {
        if (attackRange == null)
        {
            return;
        }
        attackRange.sizeDelta = Vector2.one * 2f * range;
        SetShowAttackRange(showAttack);
        if (range < 600f)
        {
            attackRangeImage.sprite = smallAttackRange;
        }
        else if (range < 1000f)
        {
            attackRangeImage.sprite = mediumAttackRange;
        }
        else
        {
            attackRangeImage.sprite = largeAttackRange;
        }
    }

    public void SetReconRange(float range)
    {
        if (reconRange == null)
        {
            return;
        }
        reconRange.sizeDelta = Vector2.one * 2f * range;
        SetShowReconRange(showRecon);
        if (range < 350f)
        {
            reconRangeImage.sprite = extraSmallReconRange;
        }
        else if (range < 600f)
        {
            reconRangeImage.sprite = smallReconRange;
        }
        else if (range < 1000f)
        {
            reconRangeImage.sprite = mediumReconRange;
        }
        else
        {
            reconRangeImage.sprite = largeReconRange;
        }
    }

    public void SetMissionRange(float range)
    {
        if (missionRange == null)
        {
            return;
        }
        missionRange.sizeDelta = Vector2.one * 2f * range;
        SetShowMissionRange(showMission);
    }

    public void SetupPath(TacticalEnemyShip ship)
    {
        this.ship = ship;
        setup++;
        InnerSetup();
    }

    public void SetShowAttackRange(bool show)
    {
        showAttack = show;
        attackRange.gameObject.SetActive(showAttack && attackRange.sizeDelta.x > 5f);
    }

    public void SetShowReconRange(bool show)
    {
        showRecon = show;
        reconRange.gameObject.SetActive(showRecon && reconRange.sizeDelta.x > 5f);
    }

    public void SetShowMissionRange(bool show)
    {
        showMission = show;
        missionRange.gameObject.SetActive(showMission && missionRange.sizeDelta.x > 5f);
    }

    public void SetShowPath(bool show)
    {
        showPath = show;
        InnerSetup();
    }

    public void SetIconsAnchor(bool up)
    {
        if (attackRangeIcon == null)
        {
            return;
        }
        if (up != iconsUp)
        {
            iconsUp = up;
            var vec = attackRangeIcon.anchorMin;
            vec.y = up ? 1f : 0f;
            attackRangeIcon.anchorMin = attackRangeIcon.anchorMax = reconRangeIcon.anchorMin = reconRangeIcon.anchorMax = vec;
            attackRangeIcon.anchoredPosition = reconRangeIcon.anchoredPosition = Vector2.zero;
        }
    }

    private void InnerSetup()
    {
        if (showPath && setup != currentSetup)
        {
            int count = 0;
            int listCount = list.Count;
            if (ship != null)
            {
                var poses = ship.CurrentPatrol.SNodePoses;
                for (int i = 1; i < poses.Count; i++)
                {
                    if (listCount == count)
                    {
                        list.Add(Instantiate(pathTrans, TacticManager.Instance.Map.transform));
                        listCount++;
                    }
                    var obj = list[count];

                    var diff = poses[i].Position - poses[i - 1].Position;
                    var size = obj.sizeDelta;
                    size.x = diff.magnitude;
                    obj.sizeDelta = size;

                    obj.anchoredPosition = poses[i - 1].Position + diff / 2f;

                    diff /= size.x;
                    float angle = Vector2.Angle(Vector2.right, diff);
                    if (diff.y < 0f)
                    {
                        angle = 180f - angle;
                    }
                    obj.localRotation = Quaternion.Euler(0f, 0f, angle);

                    count++;
                }
            }

            while (count != listCount)
            {
                listCount--;
                Destroy(list[listCount].gameObject);
                list.RemoveAt(listCount);
            }
            currentSetup = setup;
        }
        foreach (var item in list)
        {
            item.gameObject.SetActive(showPath);
        }
    }
}
