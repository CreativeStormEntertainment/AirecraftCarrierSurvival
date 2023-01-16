using System.Collections.Generic;
using UnityEngine;

public class TacticalObject : MonoBehaviour, ITacticalObjectHelper
{
    public MarkerData MarkerData
    {
        get;
        set;
    }

    public RectTransform RectTransform
    {
        get
        {
            if (!init)
            {
                init = true;
                if (rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }
            }
            return rectTransform;
        }
        protected set
        {
            rectTransform = value;
        }
    }

    public ETacticalObjectType Type
    {
        get;
        private set;
    } = ETacticalObjectType.Nothing;

    public ETacticalObjectSide Side
    {
        get;
        private set;
    } = ETacticalObjectSide.Neutral;

    public bool Visible
    {
        get;
        set;
    }

    public bool Invisible
    {
        get;
        set;
    }

    public bool Special
    {
        get;
        set;
    }

    public bool GreaterInvisibility
    {
        get;
        set;
    }

    public bool HadGreaterInvisibility
    {
        get;
        set;
    }

    public List<RectTransform> Objectives
    {
        get;
        private set;
    } = new List<RectTransform>();

    public bool InstantUpdate;

    public float MovementSpeed = 1f;

    private RectTransform rectTransform = null;
    private bool init;

    protected void ChangeType(ETacticalObjectType tType)
    {
        Type = tType;
        gameObject.name = Type.ToString();
    }

    protected void ChangeSide(ETacticalObjectSide tSide)
    {
        Side = tSide;
    }
}
