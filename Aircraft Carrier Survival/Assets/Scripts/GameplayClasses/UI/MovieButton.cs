using UnityEngine;
using UnityEngine.UI;

public class MovieButton : MonoBehaviour
{
    public ETutorialType Type
    {
        get;
        set;
    }

    [SerializeField]
    private Button button = null;

    [SerializeField]
    private GameObject frame = null;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    public void SetSelected(bool selected)
    {
        button.interactable = !selected;
        frame.SetActive(selected);
    }

    private void OnButtonClick()
    {
        MovieManager.Instance.ChangeTutorialMovie(this);
    }
}
