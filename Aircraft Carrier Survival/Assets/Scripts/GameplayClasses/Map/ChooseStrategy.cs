using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseStrategy : MonoBehaviour
{
    [SerializeField] private GameObject strategiesHandle;
    [SerializeField] private Image icon;

    public Strategy strategy = null;


    public void SetStrategy()
    {
        //GetComponentInParent<ShipWaypoint>().Map.TacticManager.SetStrategy(strategy);
    }
}
