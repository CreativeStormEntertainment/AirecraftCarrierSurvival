using UnityEngine;
using UnityEngine.UI;

public class OpenObjectives : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => HudManager.Instance.OpenObjectives());
    }
}
