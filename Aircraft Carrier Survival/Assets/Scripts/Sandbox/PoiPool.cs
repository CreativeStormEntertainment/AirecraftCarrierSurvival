using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PoiPool : Pool<SandboxPoi>
{
    [SerializeField]
    private SandboxPoi prefab = null;

    private RectTransform parent;
    private HashSet<SandboxPoi> children;

    public void Init(RectTransform parent)
    {
        children = new HashSet<SandboxPoi>();
        this.parent = parent;
        Init();
    }

    public override SandboxPoi Get(bool show = true)
    {
        var data = base.Get(show);
        data.gameObject.SetActive(show);
        return data;
    }

    public override void Push(SandboxPoi data)
    {
        data.gameObject.SetActive(false);
        base.Push(data);
    }

    protected override SandboxPoi Spawn()
    {
        var obj = UnityEngine.Object.Instantiate(prefab, parent);
        children.Add(obj);
        return obj;
    }
}
