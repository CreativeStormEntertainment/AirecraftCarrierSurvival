using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUISpriteData : MonoBehaviour
{
    public GameObject MapObjective;
    public GameObject ObjectObjective;
    public Text MapObjectiveText;
    public Text ObjectObjectiveText;
    public Image RaycastTargetMap;
    public Image RaycastTargetObject;

    [NonSerialized]
    public ObjectiveUISpriteData Parent;
}
