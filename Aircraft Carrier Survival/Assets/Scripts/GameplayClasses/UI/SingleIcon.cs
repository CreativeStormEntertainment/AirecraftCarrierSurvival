using System;
using UnityEngine;

public class SingleIcon : Icon
{
    [NonSerialized]
    public Renderer IconRenderer;
    //[SerializeField]
    //private GameObject doorMesh = null;
    //private int iconOffsetY = 2; // TODO set correct value

    private void Awake()
    {
        gameObject.SetActive(false);
        IconRenderer = GetComponent<Renderer>();
    }

    public void SetFill(float fill)
    {
        SetFill(IconRenderer, fill);
    }

    //protected override void Start()
    //{
    //    base.Start();

    //    var doorPosY = doorMesh.GetComponent<Transform>().position.y;
    //    var iconPos = GetComponent<Transform>().position;
    //    locked.GetComponent<Transform>().position = new Vector3(iconPos.x, doorPosY + iconOffsetY, iconPos.z);
    //}
}
