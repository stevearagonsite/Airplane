using System;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using TMPro;

using CustomObjects;
using static Consts.Methods;


public class UserLobbyPanels : MonoBehaviourPunCallbacks
{
    [Header("Login")]
    public GameObject LoginPanel;
    public TMP_InputField PlayerNameInput;

    Dictionary<LobbyPanel, EventsAction> _onActivePanel = new Dictionary<LobbyPanel, EventsAction>();

    private void Start()
    {
        _onActivePanel.Add(LobbyPanel.Login, new EventsAction());
        _onActivePanel[LobbyPanel.Login].Add(noob);

        _onActivePanel.Add(LobbyPanel.CreateRoom, new EventsAction());
        _onActivePanel[LobbyPanel.CreateRoom].Add(noob);

        _onActivePanel.Add(LobbyPanel.RandomRoom, new EventsAction());
        _onActivePanel[LobbyPanel.RandomRoom].Add(noob);

        _onActivePanel.Add(LobbyPanel.ListRoom, new EventsAction());
        _onActivePanel[LobbyPanel.ListRoom].Add(noob);
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
        _onActivePanel[activePanel].Execute();
    }
}

public enum LobbyPanel
{
    Login,
    CreateRoom,
    RandomRoom,
    ListRoom
}
