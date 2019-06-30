using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Photon.Pun;

using Consts;
using Utils;

public class ManagerPositions : MonoBehaviour
{
    private PhotonView _photonView;
    public static ManagerPositions Instance;
    private List<Transform> _listOfAvailablePositions = new List<Transform>();

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

        _listOfAvailablePositions = gameObject.GetComponentsInChildren<Transform>()
            .Where(t => t != transform)
            .ToList();
    }

    public Vector3 GetPosition(int index)
    {
        var dst = _listOfAvailablePositions[index];
        _photonView.RPC("RemoveAvailablePositionByIndex", RpcTarget.AllViaServer, index);

        return dst.position;
    }

    [PunRPC]
    public void RemoveAvailablePositionByIndex(int index)
    {
        _listOfAvailablePositions.RemoveAt(index);
    }
}
