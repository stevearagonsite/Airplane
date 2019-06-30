using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IObserverTimer
{
    void SubscribeStartTime(Action observer);
    void UnSubscribeStartTime(Action observer);
    
    void SubscribeEndTime(Action observer);
    void UnSubscribeEndTime(Action observer);
}