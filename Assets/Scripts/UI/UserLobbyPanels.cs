using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using Photon.Pun;
using TMPro;
using System;

public class UserLobbyPanels : MonoBehaviourPunCallbacks
{
    [Header("Login")]
    public GameObject LoginPanel;
    public TMP_InputField PlayerNameInput;
    public Dictionary<LobbyPanel, Action> onActivePanels = new Dictionary<LobbyPanel, Action>();

    private void Start()
    {
        onActivePanels.Add(LobbyPanel.Login,      delegate { });
        onActivePanels[LobbyPanel.ListRoom] += () => { };

        onActivePanels.Add(LobbyPanel.CreateRoom, delegate { });
        onActivePanels[LobbyPanel.ListRoom] += () => { };
    
        onActivePanels.Add(LobbyPanel.RandomRoom, delegate { });
        onActivePanels[LobbyPanel.ListRoom] += () => { };

        onActivePanels.Add(LobbyPanel.ListRoom,   delegate { });
        onActivePanels[LobbyPanel.ListRoom] += () => { };
    }

    public void OnLoginButtonClicked()
    {
        string userID = PlayerNameInput.text;

        if (!string.IsNullOrEmpty(userID))
        {
            PhotonNetwork.LocalPlayer.NickName = userID;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public override void OnConnectedToMaster()
    {

    }

    private void SetActivePanel(LobbyPanel activePanel)
    {
        onActivePanels[activePanel]();
    }
}

public enum LobbyPanel
{
    Login,
    CreateRoom,
    RandomRoom,
    ListRoom
}
