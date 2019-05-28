using System;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using TMPro;

using CustomObjects;

public class UserLobbyPanels : MonoBehaviourPunCallbacks
{
    // Login
    public GameObject loginPanel;
    public TMP_InputField playerNameInput;

    // Selection
    public GameObject selectionPanel;

    // Create Room
    public GameObject createRoomPanel;

    // Random Room
    public GameObject randomRoomPanel;

    // List Room
    public GameObject listRoomPanel;

    Dictionary<String, EventsAction> _onActivePanel = new Dictionary<String, EventsAction>();

    private void Start()
    {
        _onActivePanel.Add(LobbyPanel.Login.ToString(), new EventsAction());
        _onActivePanel[LobbyPanel.Login.ToString()].Add(() => {
            DiableAllPanels();
            loginPanel.SetActive(true);
        });

        _onActivePanel.Add(LobbyPanel.Selection.ToString(), new EventsAction());
        _onActivePanel[LobbyPanel.Selection.ToString()].Add(() => {
            DiableAllPanels();
            selectionPanel.SetActive(true);
        });

        _onActivePanel.Add(LobbyPanel.CreateRoom.ToString(), new EventsAction());
        _onActivePanel[LobbyPanel.CreateRoom.ToString()].Add(() => {
            DiableAllPanels();
            loginPanel.SetActive(true);
        });

        _onActivePanel.Add(LobbyPanel.RandomRoom.ToString(), new EventsAction());
        _onActivePanel[LobbyPanel.RandomRoom.ToString()].Add(() => {
            DiableAllPanels();
            randomRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(LobbyPanel.ListRoom.ToString(), new EventsAction());
        _onActivePanel[LobbyPanel.ListRoom.ToString()].Add(() => {
            DiableAllPanels();
            loginPanel.SetActive(true);
        });
    }

    private void DiableAllPanels()
    {
        loginPanel.SetActive(false);
        selectionPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        randomRoomPanel.SetActive(false);
        listRoomPanel.SetActive(false);
    }

    public void SetActivePanel(string activePanel)
    {
        _onActivePanel[activePanel.ToString()].Execute();
    }

    public void OnLoginButtonClicked()
    {
        string userID = playerNameInput.text;

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
}

public enum LobbyPanel
{
    Login,
    Selection,
    CreateRoom,
    RandomRoom,
    ListRoom
}
