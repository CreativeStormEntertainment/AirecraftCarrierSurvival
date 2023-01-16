using UnityEngine;
using UnityEngine.UI;

public class ObjectiveDescription : MonoBehaviour
{
    [SerializeField]
    private Text text = null;

    public void Setup(string objectiveDescription)
    {
        text.text = objectiveDescription;
    }
}
