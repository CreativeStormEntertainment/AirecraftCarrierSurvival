using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class AudioQueue : ParameterEventBase<EAudio>
{
    private Queue<EAudio> queue;
    private Dictionary<EAudio, Action> data;

    protected override void Awake()
    {
        base.Awake();

        queue = new Queue<EAudio>();
        data = new Dictionary<EAudio, Action>();
    }

    private void Update()
    {
        if (!fmodEvent.IsPlaying)
        {
            PlayNext();
        }
    }

    public void Queue(EAudio type, Action callback)
    {
        Assert.IsFalse(type == EAudio.Generate);
        if (data.TryGetValue(type, out var callback2))
        {
            var callback3 = callback;
            data[type] = () =>
                {
                    callback2?.Invoke();
                    callback3?.Invoke();
                };
        }
        else
        {
            queue.Enqueue(type);
            data.Add(type, callback);
        }
        if (!fmodEvent.IsPlaying)
        {
            PlayNext();
        }
    }

    private void PlayNext()
    {
        if (queue.Count > 0)
        {
            var type = queue.Dequeue();
            PlayEvent(type);

            if (data.TryGetValue(type, out var callback))
            {
                data.Remove(type);
                callback?.Invoke();
            }
        }
    }
}
