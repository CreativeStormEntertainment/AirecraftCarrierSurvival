using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HideObjectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private List<GameObject> objectToHide = null;

    public bool IsLocked
    {
        get => isLocked;
        set
        {
            if (!value)
            {
                foreach (GameObject go in objectToHide)
                {
                    go.SetActive(false);
                }
            }
            isLocked = value;
        }
    }
    private bool isLocked = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (GameObject go in objectToHide)
        {
            go.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isLocked)
        {
            foreach (GameObject go in objectToHide)
            {
                go.SetActive(true);
            }
        }
    }
}
