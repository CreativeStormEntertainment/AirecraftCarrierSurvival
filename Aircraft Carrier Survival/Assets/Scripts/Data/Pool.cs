using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pool<T> where T : new()
{
    [SerializeField]
    private int startCount = 10;
    private HashSet<T> pool;

    public void Init()
    {
        pool = new HashSet<T>();
        for (int i = 0; i < startCount; i++)
        {
            Push(Spawn());
        }
    }

    public virtual T Get(bool show = true)
    {
        if (pool.Count == 0)
        {
            pool.Add(Spawn());
        }
        using (var enumer = pool.GetEnumerator())
        {
            enumer.MoveNext();
            var data = enumer.Current;
            pool.Remove(data);
            return data;
        }
    }

    public virtual void Push(T data)
    {
        pool.Add(data);
    }

    protected virtual T Spawn()
    {
        return new T();
    }
}
