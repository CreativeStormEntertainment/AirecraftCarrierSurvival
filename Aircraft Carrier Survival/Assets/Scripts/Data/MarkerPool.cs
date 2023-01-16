using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarkerPool : Pool<InteractableMarker>
{
    [SerializeField]
    private InteractableMarker prefab = null;

    private RectTransform parent;
    private bool interactable;
    private HashSet<InteractableMarker> children;

    public void Init(RectTransform parent)
    {
        children = new HashSet<InteractableMarker>();
        this.parent = parent;
        Init();
        SetInteractable(true);
    }

    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        foreach (var obj in children)
        {
            obj.SetInteractable(interactable);
        }
    }

    public void SetShowAttackRange(bool show)
    {
        foreach (var obj in children)
        {
            obj.SetShowAttackRange(show);
        }
    }

    public void SetShowReconRange(bool show)
    {
        foreach (var obj in children)
        {
            obj.SetShowReconRange(show);
        }
    }

    public void SetShowMissionRange(bool show)
    {
        foreach (var obj in children)
        {
            obj.SetShowMissionRange(show);
        }
    }

    public void SetShowPath(bool show)
    {
        foreach (var obj in children)
        {
            obj.SetShowPath(show);
        }
    }

    public override InteractableMarker Get(bool show = true)
    {
        var data = base.Get(show);
        data.gameObject.SetActive(show);
        return data;
    }

    public override void Push(InteractableMarker data)
    {
        data.gameObject.SetActive(false);
        base.Push(data);
    }

    protected override InteractableMarker Spawn()
    {
        var obj = GameObject.Instantiate(prefab, parent);
        obj.SetInteractable(interactable);
        children.Add(obj);
        return obj;
    }
}
