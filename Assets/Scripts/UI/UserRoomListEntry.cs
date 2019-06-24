using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Photon.Pun;

public class UserRoomListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI RoomPlayersText;
    public Button JoinRoomButton;

    private string _roomName;

    public void Start()
    {
        JoinRoomButton.onClick.AddListener(() =>
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
        RoomPlayersText.SetText($"{currentPlayers} / {maxPlayers}");
    }
}
