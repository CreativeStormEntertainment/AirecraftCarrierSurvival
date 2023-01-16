using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpottedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool IsTargetingStrikeGroup
    {
        get;
        set;
    }

    public int ReconIndex
    {
        get;
        set;
    }

    public RadarEnemy RadarObject
    {
        get;
        set;
    }

    public StateTooltip Tooltip => tooltip;

    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private GameObject eyeIcon = null;
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private StateTooltip tooltip = null;
    [SerializeField]
    private StudioEventEmitter hover = null;

    private GameObject lineObject;
    private EnemyAttackData data;

    private bool isSubmarine;

    public void Setup(Sprite sprite, string number = "", RectTransform line = null, EnemyAttackData data = null, bool isSubmarine = false)
    {
        SetIcon(sprite);
        if (data != null && line != null)
        {
            this.data = data;
            text.text = number;
            lineObject = line.gameObject;
        }
        else
        {
            eyeIcon.gameObject.SetActive(true);
            text.gameObject.SetActive(false);
        }

        this.isSubmarine = isSubmarine;

        SetTooltip();
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover.Play();
        if (data != null)
        {
            lineObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (data != null)
        {
            lineObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
        var enemyAttackMan = EnemyAttacksManager.Instance;
        if (data != null)
        {
            enemyAttackMan.AttackCamera(data);
        }
        else if (isSubmarine)
        {
            enemyAttackMan.SubmarineCamera();
        }
        else
        {
            enemyAttackMan.ReconCamera(ReconIndex);
        }
    }

    //Check conditions
    private void SetTooltip()
    {
        if (data == null || data.Type == EEnemyAttackType.Scout)
        {
            if (isSubmarine)
            {
                tooltip.ChangeState(ESpottedTooltip.Submarine);
            }
            else
            {
                tooltip.ChangeState(ESpottedTooltip.Recon);
            }
            return;
        }

        if (data.Type == EEnemyAttackType.Raid || data.Type == EEnemyAttackType.Submarine)
        {
            var tooltipTarget = data.CurrentTarget == EEnemyAttackTarget.Carrier ? ESpottedTooltip.CarrierAttack : ESpottedTooltip.EscortAttack;
            tooltip.ChangeStateDescriptionParams(tooltipTarget, data.CalculatedAttackPower.ToString());
        }
    }
}

