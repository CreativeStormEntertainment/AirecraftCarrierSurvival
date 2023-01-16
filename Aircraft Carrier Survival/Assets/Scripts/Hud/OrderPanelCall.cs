using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderPanelCall : MonoBehaviour, IEnableable
{
    private static readonly int PressedTrigger = Animator.StringToHash("Pressed");
    private static readonly int NormalTrigger = Animator.StringToHash("Normal");
    private static readonly int StartTrigger = Animator.StringToHash("Start");

    public IslandBuff IslandBuff => islandBuff;

    [SerializeField]
    private Button orderCallButton = null;
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private Text hoursText = null;
    [SerializeField]
    private Text minutesText = null;
    [SerializeField]
    private Image image = null;
    [SerializeField]
    private Image cooldownImage = null;
    [SerializeField]
    private GameObject selectedImage = null;

    private bool isClicked;

    private float cdTimer;
    private int buffTimer;
    private int cooldown;
    private int minutes;
    private IslandBuff islandBuff;

    private void Start()
    {
        animator.SetTrigger(StartTrigger);
        orderCallButton.GetComponent<Button>().onClick.AddListener(() => CallOrderPanel(false));
    }

    public IslandBuffSaveData Save()
    {
        var save = new IslandBuffSaveData();
        save.AssignedOfficers = new List<int>();
        for (int i = 0; i < islandBuff.AssignedOfficers.Count; i++)
        {
            save.AssignedOfficers.Add(IslandsAndOfficersManager.Instance.CurrentOfficersList.IndexOf(islandBuff.AssignedOfficers[i]));
            save.OfficersCooldown = islandBuff.AssignedOfficers[i].Cooldown;
        }
        save.CurrentBuff = islandBuff.IslandBuffType;
        save.BuffTimer = buffTimer;
        return save;
    }

    public void Load(IslandBuffSaveData data)
    {
        buffTimer = data.BuffTimer;
    }

    public void UpdateCooldown()
    {
        cdTimer += Time.deltaTime;
        cooldownImage.fillAmount = (buffTimer + cdTimer) / cooldown;
    }

    public void CooldownTick()
    {
        buffTimer++;
        cdTimer = 0f;
        if (buffTimer < cooldown)
        {
            SetCooldownText();
        }
        else
        {
            IslandsAndOfficersManager.Instance.FinishBuff(islandBuff);
            FinishBuff();
        }
    }

    public void StartBuff(IslandBuff buff)
    {
        islandBuff = buff;
        cooldown = (int)(buff.CooldownInTicks * IslandsAndOfficersManager.Instance.BuffsTimeModifier);
        image.sprite = buff.Icon;
        image.gameObject.SetActive(true);
        HideOrderPanel(true);
    }

    public void FinishBuff()
    {
        islandBuff = null;
        image.gameObject.SetActive(false);
        buffTimer = 0;
    }

    private void SetCooldownText()
    {
        minutes = Mathf.RoundToInt(60f * (cooldown - buffTimer) / TimeManager.Instance.TicksForHour);
        minutes = Mathf.Max(minutes, 0);
        hoursText.text = (minutes / 60).ToString("00");
        minutesText.text = (minutes % 60).ToString("00");
    }

    public void SetEnable(bool enable)
    {
        if (!enable && IslandsAndOfficersManager.Instance.BuffsPanelOpen)
        {
            CallOrderPanel();
        }
    }

    public void OpenOrderPanel()
    {
        if (!IslandsAndOfficersManager.Instance.BuffsPanelOpen)
        {
            CallOrderPanel();
        }
    }

    public void HideOrderPanel(bool force = false)
    {
        if (IslandsAndOfficersManager.Instance.BuffsPanelOpen)
        {
            CallOrderPanel(force);
        }
    }

    public void SetSelected(bool selected)
    {
        selectedImage.SetActive(selected);
    }

    public void CallOrderPanel(bool force = false)
    {
        var islMan = IslandsAndOfficersManager.Instance;
        EMainSceneUI sound;
        if (islMan.BuffsPanelOpen)
        {
            if (islMan.DisableBuffClose && !force)
            {
                return;
            }
            if (islMan.CurrentBuffButton != null && islMan.CurrentBuffButton == this)
            {
                islMan.CurrentBuffButton = null;
            }
            HudManager.Instance.PopupHidden(islMan);
            islMan.CancelBuffSetup();
            islMan.BuffsPanelOpen = false;
            islMan.HideOrderPanel();
            animator.SetTrigger(NormalTrigger);
            sound = EMainSceneUI.BuffPanelClickOff;
            islMan.SetOfficerPortraitsHidden(true);
        }
        else
        {
            HudManager.Instance.PopupShown(islMan);
            islMan.BuffsPanelOpen = true;
            islMan.CurrentBuffButton = this;
            animator.SetTrigger(PressedTrigger);
            sound = EMainSceneUI.BuffPanelClickOn;
            SetSelected(true);
        }
        BackgroundAudio.Instance.PlayEvent(sound);
        HudManager.Instance.FireBuffPanelChanged(islMan.BuffsPanelOpen);
    }

    public void SetInteractable(bool interactable)
    {
        orderCallButton.interactable = interactable;
    }
}
