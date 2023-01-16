//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlaneList : MonoBehaviour
//{
//    [SerializeField] Vector2 basePlaneListPosition;
//    [SerializeField] float ySpacing;
//    [SerializeField] float iconHeight;
//    [SerializeField] float gapBetweenLists;

//    private List<PlaneIcon> planeList = new List<PlaneIcon>();

//    private List<PlaneIcon> assignedPlaneList = new List<PlaneIcon>();
//    private List<PlaneIcon> unassignedPlaneList = new List<PlaneIcon>();
//    private List<PlaneIcon> inactivePlaneList = new List<PlaneIcon>();

//    private void Start()
//    {
//        PlaneIcon[] childrenIcons = GetComponentsInChildren<PlaneIcon>();
//        LoadPlanesToList(childrenIcons);
//        RefreshList();
//    }

//    public void LoadPlanesToList(List<PlaneIcon> planesToLoad)
//    {
//        foreach (PlaneIcon p in planesToLoad)
//        {
//            planeList.Add(p);
//        }
//    }

//    public void LoadPlanesToList(PlaneIcon[] planesToLoad)
//    {
//        foreach (PlaneIcon p in planesToLoad)
//        {
//            planeList.Add(p);
//        }
//    }

//    public void RefreshList()
//    {
//        LoadAssignedPlaneList();
//        int i = 0;
//        for (; i<assignedPlaneList.Count; i++)
//        {
//            planeList[i].IsAssigned = true;
//        }
//        for (; i<planeList.Count; i++)
//        {
//            planeList[i].IsAssigned = false;
//        }
//        //for (int i=0; i<assignedPlaneList.Count; i++)
//        //{
//        //    Vector2 iconPos = basePlaneListPosition - new Vector2(0, (ySpacing+iconHeight)*i);
//        //    assignedPlaneList[i].rectTransform.anchoredPosition = iconPos;
//        //}
//        //float yOffset = assignedPlaneList.Count * (ySpacing + iconHeight) - gapBetweenLists;
//        //for (int i = 0; i < unassignedPlaneList.Count; i++)
//        //{
//        //    Vector2 iconPos = basePlaneListPosition - new Vector2(0, (ySpacing + iconHeight) * i + yOffset);
//        //    unassignedPlaneList[i].rectTransform.anchoredPosition = iconPos;
//        //}
//    }

//    private void LoadAssignedPlaneList()
//    {
//        ClearLists();
//        foreach (PlaneIcon p in planeList)
//        {
//            if (p.IsAssigned)
//            {
//                assignedPlaneList.Add(p);
//                continue;
//            }
//            unassignedPlaneList.Add(p);
//        }
//    }

//    private void ClearLists()
//    {
//        assignedPlaneList.Clear();
//        unassignedPlaneList.Clear();
//        inactivePlaneList.Clear();
//    }

//}
