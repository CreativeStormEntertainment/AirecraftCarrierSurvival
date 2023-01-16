using UnityEngine;
using UnityEngine.UI;

public class StrikeGroupActiveSkillSelection : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect = null;
    [SerializeField]
    private Button fighterButton = null;
    [SerializeField]
    private Button bomberButton = null;
    [SerializeField]
    private Button torpedoButton = null;

    private StrikeGroupManager manager = null;
    private StrikeGroupMember member = null;
    public void Setup(StrikeGroupManager strikeGroupManager)
    {
        manager = strikeGroupManager;
        fighterButton.onClick.AddListener(() => SelectType(EPlaneType.Fighter));
        bomberButton.onClick.AddListener(() => SelectType(EPlaneType.Bomber));
        torpedoButton.onClick.AddListener(() => SelectType(EPlaneType.TorpedoBomber));
        gameObject.SetActive(false);
    }

    public void OpenPanel(StrikeGroupMember member)
    {
        gameObject.SetActive(true);
        rect.SetParent(member.Button.WindowContainer);
        rect.anchoredPosition = Vector2.zero;
        this.member = member;
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    private void SelectType(EPlaneType type)
    {
        manager.UseSquadronSkill(type, member);
        CloseWindow();
    }
}
