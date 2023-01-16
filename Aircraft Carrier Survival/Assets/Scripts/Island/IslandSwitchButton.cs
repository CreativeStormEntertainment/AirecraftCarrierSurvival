using FMODUnity;
using GambitUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IslandSwitchButton : MonoBehaviour, IEnableable
{
    public bool IsDisabled
    {
        get => isDisabled || blocked;
        set
        {
            isDisabled = value;
            if (selectedFrame.activeSelf)
            {
                description.color = selectedColor;
            }
            else
            {
                description.color = IsDisabled ? disabledColor : defaultColor;
            }
            button.color = IsDisabled ? disabledColor : defaultColor;
        }
    }

    public RectTransform Parent => parent;

    [SerializeField]
    private SpriteRenderer button = null;
    [SerializeField]
    private GameObject selectedFrame = null;
    [SerializeField]
    private Sprite hoveredSprite = null;

    [SerializeField]
    private TextMeshPro description = null;
    [SerializeField]
    private RectTransform parent = null;
    [SerializeField]
    private RectTransform buttonRect = null;
    [SerializeField]
    private RectTransform textRect = null;

    [SerializeField]
    private Color selectedColor = Color.white;
    [SerializeField]
    private Color defaultColor = new Color(0.54f, 0.54f, 0.54f);
    [SerializeField]
    private Color disabledColor = new Color(0.283f, 0.283f, 0.283f, 0.67f);

    [SerializeField]
    private StudioEventEmitter hoverEmitter = null;

    private bool isDisabled = false;
    private bool interfaceDisabled;
    private bool originalInterfaceDisabled;
    private bool selectedInterfaceDisabled;

    private Sprite defaultSprite = null;
    private int myID = -1;
    private IslandRoom room = null;
    private IslandUI islandUI = null;

    private bool blocked;

    private void OnMouseEnter()
    {
        if (!interfaceDisabled)
        {
            if (!IsDisabled)
            {
                button.sprite = hoveredSprite;
                hoverEmitter.Play();
            }
            islandUI.IsUIHovered(true);
        }
    }

    private void OnMouseExit()
    {
        if (!IsDisabled)
        {
            button.sprite = defaultSprite;
        }
        islandUI.IsUIHovered(false);
    }

    private void OnMouseUpAsButton()
    {
        if (!interfaceDisabled && !IsDisabled)
        {
            bool success = !IsDisabled && room.SetSwitch(myID);
            IslandsAndOfficersManager.Instance.PlayEvent(success ? EIslandUIState.ClickSwitch : EIslandUIState.DisabledClickSwitch);
        }
    }

    public void SetBlocked(bool blocked)
    {
        this.blocked = blocked;
    }

    public void SetEnable(bool enable)
    {
        originalInterfaceDisabled = !enable;
        enable = enable && !selectedInterfaceDisabled;
        interfaceDisabled = !enable;
    }

    public void SetSelectedEnable(bool enable)
    {
        selectedInterfaceDisabled = !enable;
        SetEnable(!originalInterfaceDisabled);
    }

    public void Setup(string text, bool selected, IslandRoom room, int id, IslandUI ui)
    {
        islandUI = ui;
        this.room = room;
        myID = id;
        defaultSprite = button.sprite;
        SetSelected(selected);
        description.text = text;
        if (!description.font.HasCharacters(description.text, out var list))
        {
            text = "Missing characters:";
            foreach (char ch in list)
            {
                text += $" \"{ch}\";";
            }
            Debug.LogError(text);
        }

        var islandMan = IslandsAndOfficersManager.Instance;
        islandMan.StartCoroutineActionAfterFrames(() => LayoutRebuilder.ForceRebuildLayoutImmediate(textRect), 1);
        islandMan.StartCoroutineActionAfterFrames(SetupParentSize, 2);

        IsDisabled = true;
    }

    public void SetSelected(bool selected)
    {
        selectedFrame.SetActive(selected);
        if (selected)
        {
            description.color = selectedColor;
        }
        else
        {
            description.color = IsDisabled ? disabledColor : defaultColor;
        }
        button.color = IsDisabled ? disabledColor : defaultColor;
    }

    private void SetupParentSize()
    {
        parent.sizeDelta = new Vector2(parent.sizeDelta.x, Mathf.Max(textRect.sizeDelta.y, buttonRect.sizeDelta.y));
    }
}
