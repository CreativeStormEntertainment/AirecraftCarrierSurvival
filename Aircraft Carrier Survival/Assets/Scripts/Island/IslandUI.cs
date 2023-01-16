using GambitUtils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IslandUI : MonoBehaviour, IEnableable
{
    [SerializeField]
    private IslandUIHoverKeeper keeper = null;

    /*[SerializeField]
    private SpriteRenderer iconBackground = null;*/
    [SerializeField]
    private List<IslandSwitchButton> switches = null;
    [SerializeField]
    private List<string> switchesDescriptions = null;
    [SerializeField]
    private TextMeshPro roomName = null;
    [SerializeField]
    private List<BoxCollider> colliders = null;

    private bool isRoomHovered = false;
    private bool isUIHovered = false;
    private bool disabled;
    private bool originalDisabled;
    private bool selectedDisabled;
    private int switchCount;

    public void SetEnable(bool enable)
    {
        originalDisabled = !enable;
        enable = enable && !selectedDisabled;
        disabled = !enable;
        foreach (var islandSwitch in switches)
        {
            islandSwitch.SetEnable(enable);
        }
        IsUIHovered(isUIHovered);
    }

    public void SetEnableSwitches(int switchesEnabled)
    {
        selectedDisabled = switchesEnabled == 0;
        SetEnable(!originalDisabled);
        if (switchesEnabled > 0)
        {
            for (int i = 0; i < switchCount; i++)
            {
                switches[i].SetSelectedEnable((switchesEnabled & (1 << i)) != 0);
            }
        }
    }

    public void SetName(string text)
    {
        roomName.text = text;
        if (!roomName.font.HasCharacters(roomName.text, out var list))
        {
            var text2 = "Missing characters:";
            foreach (char ch in list)
            {
                text2 += $" \"{ch}\";";
            }
            Debug.LogError(text2);
        }
    }

    public void Setup(IslandRoom room, string name)
    {
        /*CameraManager.Instance.ViewChanged += Show;

        yesButton.onClick.AddListener(() =>
        {
            TacticManager.Instance.DeleteMissionSwitchMissions();
            SetPopup(false);
        });
        noButton.onClick.AddListener(() =>
        {
            SetPopup(false);
        });*/

        switchCount = room.SwitchCount;
        for (int i = 0; i < room.SwitchCount; i++)
        {
            switches[i].Setup(LocalizationManager.Instance.GetText(switchesDescriptions[i]), false, room, i, this);
            //SetupPosition(switches[i]);
            var height = 0.5f;
            switches[i].Parent.anchoredPosition = new Vector2(0, -height * i + .4f);

        }
        var tmpRoom = room;
        room.RoomIsReady += (isReady) =>
        {
            IslandsAndOfficersManager.Instance.FireRoomReached(tmpRoom.RoomType);
            for (int k = 0; k < tmpRoom.SwitchCount; ++k)
            {
                switches[k].IsDisabled = !isReady;
            }
        };
        keeper.Setup(this);
        SetEnabledColliders(false);
        //RectTransform myTransform = (RectTransform)transform;

        //myTransform.sizeDelta = new Vector2(height, myTransform.sizeDelta.y);




        /*if (backgroundSprite != null)
        {
            iconBackground.sprite = backgroundSprite;
        }*/
        //icon.sprite = sprite;
        roomName.text = name;
        if (!roomName.font.HasCharacters(roomName.text, out var list))
        {
            var text = "Missing characters:";
            foreach (char ch in list)
            {
                text += $" \"{ch}\";";
            }
            Debug.LogError(text);
        }
        //iconFill.gameObject.SetActive(true);
        SetVisible(false);
    }

    public void SetEnabledColliders(bool enabled)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = enabled;
        }
    }

    public void SetVisible(bool visible)
    {
        isRoomHovered = visible;
        IsUIHovered(isUIHovered);
    }

    public void IsUIHovered(bool isHovered)
    {
        isUIHovered = isHovered;
        gameObject.SetActive(!disabled && (isRoomHovered /*|| isUIHovered*/));
    }

    public void SelectSwitch(int num)
    {
        for (int i = 0; i < switches.Count; i++)
        {
            switches[i].SetSelected(num == i);
        }
    }

    public void SetBoost(int boost)
    {
        //iconFill.SetFill(1f - boost / 100f);
    }

    public void SetBlockedSwitch(int switchIndex, bool blocked)
    {
        switches[switchIndex].SetBlocked(blocked);
    }

    private void SetupPosition(IslandSwitchButton button)
    {
        IslandsAndOfficersManager.Instance.StartCoroutineActionAfterFrames(() =>
        {
            float height = 0f;
            height += button.Parent.sizeDelta.y;
            button.Parent.anchoredPosition = new Vector2(0, -height + .8f);
        }, 3);
    }
}
