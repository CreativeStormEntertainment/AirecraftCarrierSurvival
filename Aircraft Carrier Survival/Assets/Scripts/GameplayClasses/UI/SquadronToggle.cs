using UnityEngine;
using UnityEngine.UI;

public class SquadronToggle : ThreeStatesToggle
{
    public ImageBlink LandingHover => landingHover;

    [SerializeField]
    private Image onIcon = null;
    [SerializeField]
    private Image stopIcon = null;
    [SerializeField]
    private Image offIcon = null;
    [SerializeField]
    private GameObject damaged = null;
    [SerializeField]
    private ImageBlink landingHover = null;

    [SerializeField]
    private Sprite bomber = null;
    [SerializeField]
    private Sprite fighter = null;
    [SerializeField]
    private Sprite torpedo = null;

    public void Setup(int state, bool damaged, EPlaneType type, bool active)
    {
        Setup(state, active);
        this.damaged.SetActive(damaged);

        switch (type)
        {
            case EPlaneType.Bomber:
                onIcon.sprite = bomber;
                stopIcon.sprite = bomber;
                offIcon.sprite = bomber;
                break;
            case EPlaneType.Fighter:
                onIcon.sprite = fighter;
                stopIcon.sprite = fighter;
                offIcon.sprite = fighter;
                break;
            case EPlaneType.TorpedoBomber:
                onIcon.sprite = torpedo;
                stopIcon.sprite = torpedo;
                offIcon.sprite = torpedo;
                break;
        }
    }
}
