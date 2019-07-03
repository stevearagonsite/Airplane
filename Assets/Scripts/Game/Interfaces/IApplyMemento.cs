using System;
using System.Collections;
using System.Collections.Generic;
using Memento;
using UnityEngine;

public interface IApplyMemento<T>
{
    void Save(T state);

    T UnDo();

    T ReDo();
    
    T LastDo();
}
