using System;
using System.Collections;
using System.Collections.Generic;
using Consts;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PhotonView))]
public class TriggerWinner : MonoBehaviour
{
    public static TriggerWinner Instance; 
    private PhotonView _photonView;
    public bool haveWinner { get; private set; }
    public event Action eventHaveWinner = delegate {};
    public event Action eventHaveLosers = delegate {};

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

    private void Start()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
    }
    
    
    private void OnTriggerEnter(Collider c)
    {
        var layer = c.gameObject.layer;
        if (layer == Layers.PLAYERS_NUM_LAYER)
        {
            haveWinner = true;
            _photonView.RPC("Winner", RpcTarget.AllViaServer);
        }
    }
    
    [PunRPC]
    public void Winner()
    {
        if (_photonView.IsMine)
        {
            eventHaveWinner();
        }
        else
        {
            eventHaveLosers();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
