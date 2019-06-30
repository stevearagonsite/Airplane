using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Entity : MonoBehaviour
{
    protected PhotonView _photonView;
    protected int _life;

    private void Awake()
    {
        ManagerUpdate.Instance.Execute += Execution;
        ManagerUpdate.Instance.ExecuteFixed += FixedExecution;
    }

    public abstract void Move();
    protected abstract void Execution();
    protected abstract void FixedExecution();
}
