using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Platform : MonoBehaviour
{
    public bool isActive;
    protected PhotonView _photonView;
}
