using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Officer : MonoBehaviour
{
    public bool IsAdmiral
    {
        get;
        set;
    }

    public int OfficerIndex
    {
        get;
        set;
    }

    public OfficerButton Button
    {
        get;
        set;
    }
    public string Name
    {
        get;
        private set;
    }

    public string Title
    {
        get;
        private set;
    }
    public string Desc
    {
        get;
        private set;
    }

    public Sprite PortraitSprite
    {
        get;
        private set;
    }

    public Sprite CircleSprite
    {
        get;
        private set;
    }

    public Portrait Portrait
    {
        get;
        set;
    }

    public EVoiceType Voice
    {
        get;
        private set;
    }

    public IslandRoom CurrentIslandRoom
    {
        get;
        private set;
    }
    public IslandRoom LastIslandRoom
    {
        get;
        private set;
    }

    public int ManeuverLevel
    {
        get;
        private set;
    }

    public PlayerManeuverData Maneuver
    {
        get;
        private set;
    }

    public bool Occupied
    {
        get => occupied;
        set
        {
            occupied = value;
            SetHidden(occupied);
        }
    }

    public bool Assigned
    {
        get => assigned;
        set
        {
            assigned = value;
            Portrait.SetHighlighted(assigned);
        }
    }
    public List<IslandNode> Path
    {
        get => movement.Path;
    }

    public int Cooldown
    {
        get;
        set;
    }

    public Transform Transform;

    private Dictionary<EOfficerSkills, OfficerSkill> skills;
    private IslandNode occupiedNode;

    public GameObject Model = null;

    private bool assigned;
    private bool occupied;
    private OfficerMovement movement;

    private int upgradeIndex = -1;

    private void Awake()
    {
        Transform = transform;
        movement = GetComponent<OfficerMovement>();
        movement.RoomReached += OnRoomReached;
        skills = new Dictionary<EOfficerSkills, OfficerSkill>();
    }

    public void Tick()
    {
        if (Cooldown > 0)
        {
            Cooldown--;
        }
    }

    public void SetupOfficer(string name, string desc, IslandRoom room, List<OfficerSkill> officerSkills, GameObject customModel, int portraitIndex, int level, EVoiceType voice,
        string title, PlayerManeuverData manev, int upgradeIndex, IslandRoom lastRoom, bool buffs)
    {
        Name = name;
        Title = title;
        Desc = desc;
        CurrentIslandRoom = room;

        ManeuverLevel = level;
        Maneuver = manev;

        // PortraitSprite = IconManager.Instance.PortraitsAtlas.GetSprite(portraitName);
        var data = IslandsAndOfficersManager.Instance.OfficerList.Portraits[portraitIndex];
        PortraitSprite = data.Square;
        CircleSprite = data.Circle;
        if (lastRoom != null)
        {
            Assert.IsTrue(room.RoomType == EIslandRoomType.OrdersRoom);
            Assert.IsFalse(lastRoom.RoomType == EIslandRoomType.OrdersRoom);
            if (buffs)
            {
                LastIslandRoom = CurrentIslandRoom;
                CurrentIslandRoom = lastRoom;
            }
            else
            {
                LastIslandRoom = lastRoom;
            }
        }
        occupiedNode = CurrentIslandRoom.MainNode;
        occupiedNode.Occupied = true;

        transform.position = occupiedNode.Position;

        this.upgradeIndex = upgradeIndex;
        foreach (OfficerSkill skill in officerSkills)
        {
            skill.SetUpgradeIndex(upgradeIndex);
            skills.Add(skill.SkillEnum, skill);
        }
        OnRoomReached();
        if (customModel != null)
        {
            var animator = Model.GetComponent<Animator>().runtimeAnimatorController;
            var old = Model;
            Destroy(old);
            Model = Instantiate(customModel, transform);
            Model.transform.localPosition = Vector3.zero;
            Model.transform.localRotation = Quaternion.identity;
            Model.GetComponent<Animator>().runtimeAnimatorController = animator;
        }
        Voice = voice;
        movement.Setup(occupiedNode);
    }

    public void UpdateValues()
    {
        Portrait.UpdateSkillsInfo();
    }

    public void SetIslandRoom(IslandRoom islandRoom)
    {
        occupiedNode.Occupied = false;
        if (CurrentIslandRoom != null)
        {
            LastIslandRoom = CurrentIslandRoom;
            if (LastIslandRoom.RoomType == EIslandRoomType.OrdersRoom)
            {
                movement.ForceIdle();
            }
        }
        CurrentIslandRoom = islandRoom;
        //bufferBeforeWork = 100;
        occupiedNode = CurrentIslandRoom.MainNode;
        occupiedNode.Occupied = true;
        movement.StartMovement(occupiedNode);
        CurrentIslandRoom.MainNode.Occupied = true;
        //Portrait.UpdateIcon();
    }

    public void OnClickEnd(bool success)
    {
        if (success)
        {
            var islandMan = IslandsAndOfficersManager.Instance;
            islandMan.PlayEvent(EIslandUIState.Click);
            islandMan.SelectedOfficer = this;
        }
    }

    public float GetClickHoldTime()
    {
        return 0f;
    }

    public bool CanBeAssignedToCategory(EIslandRoomCategory category)
    {
        if (category == EIslandRoomCategory.AIR)
        {
            return skills.ContainsKey(EOfficerSkills.CommandingAirForce);
        }
        else
        {
            return skills.ContainsKey(EOfficerSkills.CommandingNavy);
        }
    }

    public int GetSkillLevel(EIslandRoomCategory category)
    {
        if (category == EIslandRoomCategory.AIR)
        {
            return skills.ContainsKey(EOfficerSkills.CommandingAirForce) ? skills[EOfficerSkills.CommandingAirForce].CurrentLevel : 0;
        }
        else
        {
            return skills.ContainsKey(EOfficerSkills.CommandingNavy) ? skills[EOfficerSkills.CommandingNavy].CurrentLevel : 0;
        }
    }

    public void SetHidden(bool hidden)
    {
        Portrait.SetHidden(hidden);
    }

    public void StartCooldown()
    {
        Cooldown = Parameters.Instance.OfficerCooldownTicks;
        Portrait.CooldownTime = 0f;
    }

    public void BlockForHours(int hours)
    {
        Cooldown = hours * TimeManager.Instance.TicksForHour;
        Portrait.CooldownTime = 0f;
    }

    private void OnRoomReached()
    {
        CurrentIslandRoom.OfficerStartedWorking(this);
    }
}