using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using TMPro;

public class UserConnectionStatus : MonoBehaviour
{
    private const string ConnectionStatusMessage = "Connection Status:";

    [Header("UI References")]
    public TextMeshProUGUI connectionStatusText;


    public void Update()
    {
        connectionStatusText.SetText($"{ConnectionStatusMessage} {PhotonNetwork.NetworkClientState}");
    }
}
