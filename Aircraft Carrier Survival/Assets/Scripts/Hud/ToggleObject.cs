using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public void Toggle()
    {
        SetShow(!gameObject.activeSelf);
    }

    public virtual void SetShow(bool show)
    {
        gameObject.SetActive(show);
    }
}
