using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Entity : MonoBehaviour
{
    protected PhotonView _photonView;
    protected int _life;
    
    //Strategy
    protected Dictionary<string, IMoveEntity> _moveBehaviors = new Dictionary<string, IMoveEntity>();
    protected IMoveEntity _currentMove;

    private void Awake()
    {
        ManagerUpdate.Instance.Execute += Execution;
        ManagerUpdate.Instance.ExecuteFixed += FixedExecution;
    }

    private void OnDestroy() {
        ManagerUpdate.Instance.Execute -= Execution;
        ManagerUpdate.Instance.ExecuteFixed -= FixedExecution;
    }

    public abstract void Move();
    protected abstract void Execution();
    protected abstract void FixedExecution();
}
