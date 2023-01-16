
public class ObjectivesTopRightButton : TopRightButton
{
    protected override void Start()
    {
        Button.onClick.AddListener(() => ObjectivesManager.Instance.ToggleObjectives());
    }
}
