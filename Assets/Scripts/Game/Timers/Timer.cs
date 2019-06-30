using System;
using UnityEngine;

public abstract class UserTimer: MonoBehaviour, IObserverTimer
{
    private event Action OnStartTimeEvents = delegate { }; 
    private event Action OnEndTimeEvents = delegate { }; 
    private float _baseTime;
    private float _currentBaseTime;

    public float baseTime
    {
        get { return _baseTime; }
        set { _baseTime = value; }
    }

    public abstract void StartTimer();
    public abstract void EndTimer();
        
    public virtual void SubscribeStartTime(Action observer)
    {
        OnStartTimeEvents += observer;
    }

    public virtual void UnSubscribeStartTime(Action observer)
    {
        OnStartTimeEvents -= observer;
    }

    public virtual void SubscribeEndTime(Action observer)
    {
        OnEndTimeEvents += observer;
    }

    public virtual void UnSubscribeEndTime(Action observer)
    {
        OnEndTimeEvents -= observer;                
    }

    public void UnSubscribeAllStartTime()
    {
        OnStartTimeEvents = delegate {};
    }
        
    public void UnSubscribeAllEndTime()
    {
        OnEndTimeEvents = delegate {};
    }
}