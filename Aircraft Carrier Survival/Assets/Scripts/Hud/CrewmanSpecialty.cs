using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewmanSpecialty : MonoBehaviour
{
    [SerializeField]
    private List<Image> specIcons = null;

    public void SetSpecialties(List<ECrewmanSpecialty> specialties)
    {
        for (int i = 0; i < specialties.Count; i++)
        {
            specIcons[i].sprite = CrewManager.Instance.CrewSpecialtiesDict[specialties[i]].Icon;
            specIcons[i].transform.parent.gameObject.SetActive(true);
        }
        for (int i = specialties.Count; i < specIcons.Count; i++)
        {
            specIcons[i].transform.parent.gameObject.SetActive(false);
        }
    }
}
