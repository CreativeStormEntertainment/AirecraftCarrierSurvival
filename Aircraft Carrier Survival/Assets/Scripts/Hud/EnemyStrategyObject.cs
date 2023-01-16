using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyStrategyObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public EnemyManeuverInstanceData Data
    {
        get;
        private set;
    }

    public GameObject Outline => outline;

    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    private GameObject chosen = null;
    [SerializeField]
    private GameObject dead = null;
    [SerializeField]
    private GameObject outline = null;
    [SerializeField]
    private StudioEventEmitter selectSound = null;
    [SerializeField]
    private StudioEventEmitter hoverSound = null;

    [SerializeField]
    private EnemyStrategySubobject objA = null;
    [SerializeField]
    private EnemyStrategySubobject objB = null;
    [SerializeField]
    private EnemyStrategySubobject objC = null;
    [SerializeField]
    private Button switchButton = null;
    [SerializeField]
    private ButtonHighlightMaster buttonHighlight = null;
    [SerializeField]
    private Button switchButton2 = null;
    [SerializeField]
    private ButtonHighlightMaster buttonHighlight2 = null;
    [SerializeField]
    private GameObject mark = null;
    [SerializeField]
    private GameObject mark2 = null;

    private int index;
    private int aliveIndex;
    private bool selected;

    private bool blocked;

    private EnemyStrategyPanel enemyStrategyPanel;

    private bool alternative;
    private bool leftActive;

    private bool init;

    public void Init()
    {
        var size = (transform.parent as RectTransform).sizeDelta;
        objA.Init(size);
        objB.Init(size);
        objC.Init(size);
    }

    public void Setup(EnemyManeuverInstanceData data, int index, EnemyStrategyPanel enemyStrategyPanel, int aliveIndex)
    {
        alternative = false;

        Data = data;
        SetSelected(false);
        this.enemyStrategyPanel = enemyStrategyPanel;
        this.index = index;
        this.aliveIndex = aliveIndex;

        if (data == null)
        {
            blocked = true;
            dead.SetActive(false);
            objC.gameObject.SetActive(false);
        }
        else
        {
            dead.SetActive(data.Dead);
            blocked = data.Dead;

            objC.Setup(data.Data, (data.Dead ? 0 : data.CurrentDurability), ((data.Dead || !data.Visible) ? 0 : data.Data.Durability));
            objC.gameObject.SetActive(true);
        }

        transform.parent.gameObject.SetActive(true);

        objA.gameObject.SetActive(false);
        objB.gameObject.SetActive(false);
        objC.gameObject.SetActive(true);
        mark.gameObject.SetActive(false);
        mark2.gameObject.SetActive(false);
        switchButton.transform.parent.gameObject.SetActive(false);
        switchButton2.transform.parent.gameObject.SetActive(false);
    }

    public void Setup(EnemyManeuverInstanceData data, EnemyManeuverData alternativeBlock, bool alternativeLeft, int index, EnemyStrategyPanel enemyStrategyPanel, int aliveIndex)
    {
        if (!init)
        {
            init = true;
            switchButton.onClick.AddListener(Switch);
            switchButton2.onClick.AddListener(Switch);
        }
        Data = data;
        SetSelected(false);
        this.enemyStrategyPanel = enemyStrategyPanel;
        this.index = index;
        this.aliveIndex = aliveIndex;
        dead.SetActive(false);

        alternative = true;

        var first = data.Data;
        var second = alternativeBlock;
        if (alternativeLeft)
        {
            (first, second) = (second, first);
        }

        transform.parent.gameObject.SetActive(true);
        objA.Setup(first, first, second);
        objB.Setup(second, first, second);
        objC.gameObject.SetActive(false);
        mark.SetActive(true);
        mark2.SetActive(true);

        leftActive = false;
        SwitchInner();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected && !blocked)
        {
            highlight.SetActive(true);
            hoverSound.Play();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected && !blocked)
        {
            highlight.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!selected && !blocked)
        {
            enemyStrategyPanel.SetSelectedObject(index);
            selectSound.Play();
        }
    }

    public void SetSelected(bool value)
    {
        selected = value;
        highlight.SetActive(false);
        chosen.SetActive(value);
    }

    private void Switch()
    {
        if (alternative)
        {
            enemyStrategyPanel.SwitchType(aliveIndex);
            SwitchInner();
            (leftActive ? objA : objB).Animate();
        }
    }

    private void SwitchInner()
    {
        leftActive = !leftActive;
        objA.gameObject.SetActive(leftActive);
        objB.gameObject.SetActive(!leftActive);

        if (leftActive)
        {
            buttonHighlight2.ResetHighlight();
        }
        else
        {
            buttonHighlight.ResetHighlight();
        }

        switchButton.transform.parent.gameObject.SetActive(leftActive);
        switchButton2.transform.parent.gameObject.SetActive(!leftActive);
    }
}
