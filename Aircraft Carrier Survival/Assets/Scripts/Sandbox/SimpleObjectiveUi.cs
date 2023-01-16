using UnityEngine;
using UnityEngine.UI;

public class SimpleObjectiveUi : MonoBehaviour
{
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text description = null;
    [SerializeField]
    private Text index = null;

    public void Setup(string title, string desc, int objectiveIndex)
    {
        this.title.text = title;
        description.text = desc;
        index.text = objectiveIndex.ToString();
        gameObject.SetActive(true);
    }
}
