using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Photon.Pun;
using UnityEngine.Serialization;

public class UserRoomListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI roomPlayersText;
    public Button joinRoomButton;

    private string _roomName;

    public void Start()
    {
        joinRoomButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinRoom(_roomName);
        });
    }

    public void Initialize(string name, byte currentPlayers, byte maxPlayers)
    {
        _roomName = name;

        roomNameText.SetText(name);
        roomPlayersText.SetText($"{currentPlayers} / {maxPlayers}");
    }
}
