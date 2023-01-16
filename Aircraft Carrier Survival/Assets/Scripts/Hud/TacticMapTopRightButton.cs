
public class TacticMapTopRightButton : TopRightButton
{
    protected override void Start()
    {
        Button.onClick.AddListener(() => HudManager.Instance.ToggleTacticMap());
    }
}
