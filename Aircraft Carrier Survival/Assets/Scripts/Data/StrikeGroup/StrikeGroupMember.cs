using UnityEngine;

public class StrikeGroupMember
{
    public StrikeGroupMemberData Data;
    public StrikeGroupButton Button;
    public GameObject GameObject;
    public int Prepare;
    public int Duration;
    public int Cooldown;
    public int MaxCooldown;
    public int CurrentDurability;
    public int CurrentDuration;
    public int Used;
    public int Slot;
    public int Custom;
    public EscortAnim EscortAnim;
    private StrikeGroupManager manager;
    private bool active;

    public StrikeGroupMember(StrikeGroupManager manager, StrikeGroupMemberData data, StrikeGroupButton button, int slot, int custom)
    {
        this.manager = manager;
        Data = data;
        Slot = slot;
        Button = button;
        Button.Setup(data, this);

        int ticksForHour = TimeManager.Instance.TicksForHour;
        Prepare = Mathf.RoundToInt(Data.PrepareHours * ticksForHour);
        Duration = Mathf.RoundToInt(Data.DurationHours * ticksForHour);
        MaxCooldown = Mathf.RoundToInt(Data.CooldownHours * ticksForHour);

        CurrentDurability = Data.Durability;

        //Button.ActiveButton.gameObject.SetActive(Data.ActiveSkill != EStrikeGroupActiveSkill.None);
        Button.ActiveButton.onClick.AddListener(ClickActivate);

        Custom = custom;
    }

    public bool LoadData(EscortSaveData data)
    {
        CurrentDurability = data.Health;
        Button.UpdateDurability(CurrentDurability);
        Used = data.Used;
        if (CurrentDurability <= 0)
        {
            active = false;
            Update();
            return true;
        }
        else
        {
            if (data.SkillCooldown >= 0)
            {
                Cooldown = data.SkillCooldown + 1;

                Button.CooldownImage.gameObject.SetActive(true);
                active = false;
            }

            Update();
            return false;
        }
    }

    public EscortSaveData SaveData()
    {
        EscortSaveData result = new EscortSaveData();

        result.Health = CurrentDurability;
        result.SkillCooldown = Cooldown > 0 ? Cooldown : -1;
        result.Used = Used;
        result.Custom = Custom;

        return result;
    }

    public bool Damage(int damage)
    {
        CurrentDurability -= damage;
        CurrentDurability = Mathf.Min(Data.Durability, CurrentDurability);
        Button.UpdateDurability(CurrentDurability);
        if (CurrentDurability <= 0)
        {
            //Button.ActiveButton.gameObject.SetActive(false);
            if (EscortAnim != null)
            {
                if (EscortAnim.SmokeParticles.Count > 0)
                {
                    HudManager.Instance.SmokeController.RemoveSmokeMember(EscortAnim.SmokeParticles);
                }
                if (EscortAnim.Smoke.Count > 0)
                {
                    EscortAnim.StartFire();
                }
            }
            active = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Update()
    {
        if (Cooldown --> 0 && Cooldown <= 0)
        {
            Cooldown = 0;
        }

        active = true;
        UpdateButton();
        active = active && Cooldown <= 0;
        if (active)
        {
            if ((manager.EnabledShips & (1 << Slot)) == 0)
            {
                active = false;
            }
            else
            {
                active = CanUseSkill();
            }
            if (active)
            {
                Button.SetButtonSprite(StrikeGroupManager.Instance.CurrentlyActivatingSkill == this ? EButtonStates.Pressed : EButtonStates.Normal);
            }
            else
            {
                Button.SetButtonSprite(EButtonStates.Grey);
            }
        }
        else
        {
            if (CurrentDuration <= 0)
            {
                Button.SetButtonSprite(EButtonStates.Grey);
            }
        }
        var uses = SaveManager.Instance.Data.GameMode == EGameMode.Sandbox ? Data.SandboxUses : Data.Uses;
        if (uses > 0 && Used >= uses)
        {
            Button.SetButtonSprite(EButtonStates.Grey);
        }
    }

    public void UpdateButton()
    {
        int hours;
        int minutes;
        if (StrikeGroupManager.Instance.CurrentActiveSkills.TryGetValue(Data, out var data) && data != null && data.Timer > 0)
        {
            CurrentDuration = data.Timer;
            hours = (int)(CurrentDuration / (float)TimeManager.Instance.TicksForHour);
            minutes = (int)((CurrentDuration - (hours * TimeManager.Instance.TicksForHour)) / ((float)TimeManager.Instance.TicksForHour / 60f));
            Button.SetButtonSprite(EButtonStates.Red);
            active = false;
        }
        else
        {
            CurrentDuration = 0;
            hours = (int)(Cooldown / (float)TimeManager.Instance.TicksForHour);
            minutes = (int)((Cooldown - (hours * TimeManager.Instance.TicksForHour)) / ((float)TimeManager.Instance.TicksForHour / 60f));
            //Button.CooldownImage.fillAmount = ((float)Cooldown) / ((float)MaxCooldown);
        }
        Button.CooldownText.text = "-" + hours.ToString("00");
        Button.MinutesText.text = minutes.ToString("00");
        Button.CooldownImage.gameObject.SetActive(hours > 0 || minutes > 0);
    }

    public void StartCooldown()
    {
        Cooldown = Mathf.Max((int)(MaxCooldown * StrikeGroupManager.Instance.EscortCooldownModifier), Duration);
        UpdateButton();
    }

    public void Destroy()
    {
        Object.Destroy(GameObject);
        Object.Destroy(Button.gameObject);
    }

    private void ClickActivate()
    {
        if (active && Data.ActiveSkill != EStrikeGroupActiveSkill.None)
        {
            BackgroundAudio.Instance.PlayEvent(EButtonState.Click);
            manager.UseActiveSkill(this);
        }
        else
        {
            BackgroundAudio.Instance.PlayEvent(EMainSceneUI.InactiveClick);
        }
    }

    private bool CanUseSkill()
    {
        if (WorldMap.Instance.gameObject.activeInHierarchy)
        {
            return false;
        }
        int index = 0;
        switch (Data.ActiveSkill)
        {
            case EStrikeGroupActiveSkill.ReduceEscortCooldown:
                foreach (var member in StrikeGroupManager.Instance.AliveMembers)
                {
                    if (member.Cooldown > 0f)
                    {
                        index++;
                    }
                }
                return index > 0;
            case EStrikeGroupActiveSkill.RepairEscort:
                foreach (var member in StrikeGroupManager.Instance.AliveMembers)
                {
                    if (member.CurrentDurability < member.Data.Durability)
                    {
                        index++;
                    }
                }
                return index > 0;
            case EStrikeGroupActiveSkill.RescueSurvivors:
                var survivorObject = TacticManager.Instance.GetSurvivorObjectInRange();
                return survivorObject != null;
            case EStrikeGroupActiveSkill.ReturnSquadrons:
                foreach (var list in TacticManager.Instance.Missions.Values)
                {
                    foreach (var mission in list)
                    {
                        if (mission.MissionStage == EMissionStage.AwaitingRetrieval || mission.MissionStage == EMissionStage.ReadyToRetrieve || mission.MissionStage == EMissionStage.Recovering)
                        {
                            return true;
                        }
                    }
                }
                return false;
            case EStrikeGroupActiveSkill.SinkCargoShip:
                foreach (var enemy in TacticManager.Instance.GetAllShips())
                {
                    if (enemy.Visible && !enemy.IsDisabled && !enemy.Dead && enemy.Side == ETacticalObjectSide.Enemy)
                    {
                        bool show = false;
                        foreach (var block in enemy.Blocks)
                        {
                            if (block.Visible && !block.Dead && block.Data.ShipType == EEnemyShipType.Cargo)
                            {
                                show = true;
                            }
                        }
                        if (show)
                        {
                            index++;
                        }
                    }
                }
                return index > 0;
            case EStrikeGroupActiveSkill.BonusManeuversDefence:
                foreach (var enemy in TacticManager.Instance.GetAllShips())
                {
                    if (enemy.Visible && enemy.Side == ETacticalObjectSide.Enemy && enemy.Type == ETacticalObjectType.Outpost && !enemy.Dead && enemy.BonusManeuversDefence == 0)
                    {
                        index++;
                    }
                }
                return index > 0;
            case EStrikeGroupActiveSkill.SetUndetected:
                return !RadarManager.Instance.HasEnemyAttack();
            case EStrikeGroupActiveSkill.ReplenishSquadrons:
                var deck = AircraftCarrierDeckManager.Instance;
                int squadrons = 0;
                for (int i = 0; i < 3; i++)
                {
                    squadrons += deck.GetAllSquadronsCount((EPlaneType)i);
                }
                return squadrons < deck.MaxAllSquadronsCount;
            case EStrikeGroupActiveSkill.SendAntiScoutMission:
                bool hasScouts = false;
                foreach (var mission in TacticManager.Instance.Missions[EMissionOrderType.CounterHostileScouts])
                {
                    if (mission.MissionStage == EMissionStage.ReadyToLaunch)
                    {
                        hasScouts = true;
                    }
                }
                return hasScouts;
        }
        return true;
    }
}
