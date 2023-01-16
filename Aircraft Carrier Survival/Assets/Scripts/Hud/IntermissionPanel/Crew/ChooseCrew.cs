using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCrew : MonoBehaviour
{
    [SerializeField]
    private RectTransform RectT = null;

    [SerializeField]
    private Transform officerOffset = null;

    [SerializeField]
    private Transform crewOffset = null;

    public void Show(Vector3 position, bool isOfficer)
    {
        RectT.anchoredPosition = position + (isOfficer ? officerOffset.localPosition : crewOffset.localPosition);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (OfficerDescription.Instance != null)
        {
            OfficerDescription.Instance.gameObject.SetActive(false);
        }

        if (CrewmanDescription.Instance != null)
        {
            CrewmanDescription.Instance.gameObject.SetActive(false);
        }
    }
}
