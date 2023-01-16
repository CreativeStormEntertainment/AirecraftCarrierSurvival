using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubcrewmanIcon : MonoBehaviour, IPointerClickHandler
{
    public Image SectionCategoryIcon;
    public Image SectionIcon;
    public Subcrewman Subcrewman;

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (Subcrewman.Subsection != null)
        //{
        //    if (eventData.button == PointerEventData.InputButton.Left)
        //    {
        //        CameraManager.Instance.ZoomToSection(Subcrewman.Subsection.ParentSection);
        //    }
        //    else if (eventData.button == PointerEventData.InputButton.Right)
        //    {
        //        if (Subcrewman.DCType == EDCType.None)
        //        {
        //            SectionRoomManager.Instance.RemoveCrewman(Subcrewman.Subsection.ParentSection, false);
        //        }
        //        else
        //        {
        //            //DamageControlManager.Instance.FinishDC(Subcrewman.DCType, Subcrewman.Subsection, false);
        //        }
        //    }
        //}
    }
}
