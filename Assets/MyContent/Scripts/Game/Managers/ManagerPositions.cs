using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Consts;
using Utils;

public class ManagerPositions : MonoBehaviour {
    public static ManagerPositions Instance;
    private PhotonView _photonView;
    private List<Transform> _listOfAvailablePositions = new List<Transform>();

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

        _listOfAvailablePositions = gameObject.GetComponentsInChildren<Transform>()
            .Where(t => t != transform)
            .ToList();
    }

    public Vector3 GetPosition(int index) {
        //HotFix hooks execution
        if (index >  _listOfAvailablePositions.Count - 1 || index < 0) {
            Debug.LogError("The index is out of range");
            return Vector3.zero;
        }
        
        var dst = _listOfAvailablePositions[index];
        _photonView.RPC("RemoveAvailablePositionByIndex", RpcTarget.AllViaServer, index);

        return dst.position;
    }

    [PunRPC]
    public void RemoveAvailablePositionByIndex(int index) {
        _listOfAvailablePositions.RemoveAt(index);
    }
}