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
    private List<Transform> _listOfPositions = new List<Transform>();
    private List<Transform> _listOfAvailablePositions;

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
        this._photonView = this.gameObject.GetComponent<PhotonView>();

        this._listOfPositions = this.gameObject.GetComponentsInChildren<Transform>().ToList();
        this._listOfAvailablePositions = new List<Transform>(_listOfPositions);
    }

    public Vector3 GetPosition()
    {
        var positionsCount = _listOfAvailablePositions.Count;
        var dstPositionIndex = (int)Rand.Range(1, positionsCount - 1);
        var dst = _listOfAvailablePositions[dstPositionIndex];

        _photonView.RPC("RemoveAvailablePositionByIndex", RpcTarget.OthersBuffered, dstPositionIndex);
        _listOfAvailablePositions.RemoveAt(dstPositionIndex);

        return dst.position;
    }

    [PunRPC]
    public void RemoveAvailablePositionByIndex(int index)
    {
        _listOfAvailablePositions.RemoveAt(index);
    }
}
