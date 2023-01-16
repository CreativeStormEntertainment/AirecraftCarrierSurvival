
public class CrewManagementTopRightButton : TopRightButton
{
    protected override void Start()
    {
        Button.onClick.AddListener(() => CrewManager.Instance.ToggleShow());
    }
}
