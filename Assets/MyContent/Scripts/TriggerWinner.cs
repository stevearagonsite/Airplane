using System;
using System.Collections;
using System.Collections.Generic;
using Consts;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PhotonView))]
public class TriggerWinner : MonoBehaviour, IObservableEventDefeated, IObservableEventWinner {
    public static TriggerWinner Instance;
    private PhotonView _photonView;
    public bool haveWinner { get; private set; }
    public event Action onEventWinner = delegate { };
    public event Action onEventLosers = delegate { };

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            Instance = this;
        }
        else {
            Instance = this;
        }
    }

    private void Start() {
        _photonView = gameObject.GetComponent<PhotonView>();
    }


    private void OnTriggerEnter(Collider c) {
        var layer = c.gameObject.layer;
        if (layer != Layers.PLAYERS_NUM_LAYER) return;
        haveWinner = true;
        
        //TODO: Pending fix the winner condition
        _photonView.RPC("raceIsFinished", RpcTarget.AllViaServer, PhotonNetwork.NickName);
    }

    [PunRPC]
    public void raceIsFinished(string userId) {
        if (PhotonNetwork.NickName == userId) {
            onEventWinner();
        }
        else {
            onEventLosers();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }

    public void SubscribeEventWinner(IObserverEventDefeated observer) {
        throw new NotImplementedException();
    }

    public void UnSubscribeEventWinner(IObserverEventDefeated observer) {
        throw new NotImplementedException();
    }

    public void SubscribeEventWinner(IObserverEventWinner observer) {
        throw new NotImplementedException();
    }

    public void UnSubscribeEventWinner(IObserverEventWinner observer) {
        throw new NotImplementedException();
    }
}