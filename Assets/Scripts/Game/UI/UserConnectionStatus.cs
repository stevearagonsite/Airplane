using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using TMPro;

public class UserConnectionStatus : MonoBehaviour
{
    private readonly string connectionStatusMessage = "Connection Status:";

    [Header("UI References")]
    public TextMeshProUGUI ConnectionStatusText;


    public void Update()
    {
        ConnectionStatusText.SetText($"{connectionStatusMessage} {PhotonNetwork.NetworkClientState}");
    }
}
