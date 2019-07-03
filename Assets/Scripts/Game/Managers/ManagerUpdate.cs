using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ManagerUpdate : MonoBehaviour {

    public static ManagerUpdate Instance;
    public event Action Execute = delegate { };
    public event Action ExecuteFixed = delegate { };
    public event Action ExecuteLate = delegate { };

    public bool isPause{ get; set;}

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            Instance = this;
        }
        else
        {
            Instance = this;
        }
    }


    #region UPDATES

    private void Update()
    {
        if (!isPause && !Execute.Equals(null))
        {
            Execute();
        }
    }

    private void LateUpdate()
    {
        if (!isPause && !ExecuteLate.Equals(null)) ExecuteLate();
    }

    private void FixedUpdate()
    {
        if (!isPause && !ExecuteFixed.Equals(null)) ExecuteFixed();
    }

    #endregion UPDATES
}

public class CoroutineUpdate{

    private event Action _Update = delegate { };
    private bool _activeLoop = true;
    public float time { get; set; }
    public bool activeTime { get; set; }

    public CoroutineUpdate(bool activeTime,float time = 0.3f)
    {
        this.activeTime = activeTime;
        this.time = time;
    }

    public void Subscribe(Action action)
    {
        _Update += action;
    }

    public void Unsubscribe(Action action)
    {
        _Update -= action;
    }

    public void Clean()
    {
        _Update = delegate { };
        _activeLoop = false;
        time = 0;
    }

    /// <summary> Execute coroutine in start and method you use this class. </summary>
    public IEnumerator CoroutineMethod()
    {
        while (_activeLoop)
        {
            if (!ManagerUpdate.Instance.isPause)
            {
                if (activeTime)
                {
                    _Update();
                    yield return new WaitForSeconds(time);
                }
                else
                {
                    _Update();
                    yield return null;
                }
            }
            yield return null;
        }
    }
}
