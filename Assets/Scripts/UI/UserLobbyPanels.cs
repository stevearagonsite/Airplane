using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

using CustomObjects;
using Utils;
using Consts;

public class UserLobbyPanels : MonoBehaviourPunCallbacks
{
    private const string LOGIN = "Login";
    private const string SELECTION = "Selection";
    private const string CREATE_ROOM = "CreateRoom";
    private const string RANDOM_ROOM = "RandomRoom";
    private const string LIST_ROOM = "ListRoom";
    private const string INSIDE_ROOM = "InsideRoom";
    private const string PATH_INSIDE_ROOM_PLAYER = "Prefabs/PlayerRoom";

    // Login
    public GameObject loginPanel;
    public TMP_InputField inputPlayerName;

    // Selection
    public GameObject selectionPanel;
    public TMP_InputField inputRoomName;
    public TMP_InputField inputMaxPlayers;

    // Create Room
    public GameObject createRoomPanel;

    // Random Room
    public GameObject randomRoomPanel;

    // List Room
    public GameObject listRoomPanel;

    // Inside Room
    public GameObject insideRoomPanel;
    public Button startGameButton;

    Dictionary<string, EventsAction> _onActivePanel = new Dictionary<string, EventsAction>();

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        inputPlayerName.text = "Player " + Rand.Range(1000, 10000);
    }

    private void Start()
    {
        _onActivePanel.Add(LOGIN, new EventsAction());
        _onActivePanel[LOGIN].Add(() => {
            DiableAllPanels();
            loginPanel.SetActive(true);
        });

        _onActivePanel.Add(SELECTION, new EventsAction());
        _onActivePanel[SELECTION].Add(() => {
            DiableAllPanels();
            selectionPanel.SetActive(true);
        });

        _onActivePanel.Add(CREATE_ROOM, new EventsAction());
        _onActivePanel[CREATE_ROOM].Add(() => {
            DiableAllPanels();
            createRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(RANDOM_ROOM, new EventsAction());
        _onActivePanel[RANDOM_ROOM].Add(() => {
            DiableAllPanels();
            randomRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(LIST_ROOM, new EventsAction());
        _onActivePanel[LIST_ROOM].Add(() => {
            DiableAllPanels();
            loginPanel.SetActive(true);
        });

        _onActivePanel.Add(INSIDE_ROOM, new EventsAction());
        _onActivePanel[INSIDE_ROOM].Add(() => {
            DiableAllPanels();
            insideRoomPanel.SetActive(true);
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
        Debug.Log("SetActivePanel");
        _onActivePanel[activePanel].Execute();
    }


    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(UserGame.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void LocalPlayerPropertiesUpdated()
    {
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    #region PUN-CALLBACKS
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        this.SetActivePanel(SELECTION);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
    }

    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");
        SetActivePanel(SELECTION);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed");
        SetActivePanel(SELECTION);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        string roomName = "Room " + Rand.Range(1000, 10000);

        RoomOptions options = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        SetActivePanel(INSIDE_ROOM);

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            var objectToInstantiate = Resources.Load(PATH_INSIDE_ROOM_PLAYER);
            Debug.Log(objectToInstantiate);
            GameObject entry = Instantiate((GameObject)objectToInstantiate);
            //Players canvas in gameObject
            var canvasPlayers = insideRoomPanel.transform.GetChild(0);
            entry.transform.SetParent(canvasPlayers);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<UserPlayerListEntry>().Initialize(p.ActorNumber, p.NickName);

            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(UserGame.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<UserPlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }

        startGameButton.gameObject.SetActive(CheckPlayersReady());

        Hashtable props = new Hashtable
            {
                {UserGame.PLAYER_LOADED_LEVEL, false}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnLeftRoom()
    {
        SetActivePanel(SELECTION);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("OnMasterClientSwitched");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("OnPlayerPropertiesUpdate");
    }
    #endregion PUN-CALLBACKS

    #region UI
    public void OnLoginButton()
    {
        string userID = inputPlayerName.text;

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

    public void OnBackButton()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        SetActivePanel(SELECTION);
    }

    public void OnLeaveGameButton()
    {
        //Execute -> LeftRoom callback
        PhotonNetwork.LeaveRoom();
    }

    public void OnCreateRoomButton()
    {
        string roomName = inputRoomName.text;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + Rand.Range(1000, 10000) : roomName;

        byte maxPlayers;
        byte.TryParse(inputMaxPlayers.text, out maxPlayers);
        maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        //Execute -> OnJoinedRoom callback
        PhotonNetwork.CreateRoom(roomName, options, null);
    }
    #endregion UI
}
