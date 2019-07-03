using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using TMPro;

using CustomObjects;
using Utils;
using Consts;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class UserLobbyPanels : MonoBehaviourPunCallbacks
{
    private const string Login = "Login";
    private const string Selection = "Selection";
    private const string CreateRoom = "CreateRoom";
    private const string RandomRoom = "RandomRoom";
    private const string ListRooms = "ListRoom";
    private const string HowToPlay = "HowToPlay";
    private const string Credits = "Credits";
    private const string InsideRoom = "InsideRoom";
    private const string Loading = "Loading";
    private const string PathInsideRoomPlayer = "Prefabs/PlayerRoom";
    private const string PathListObjectRoom = "Prefabs/ListObjectRoom";

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
    public GameObject roomListContent;
    
    // How to play
    public GameObject howToPlayPanel;

    // Credits
    public GameObject creditsPanel;
    
    // Loading
    public GameObject LoadingPanel;
    public UserProgress progress;
    
    // Inside Room
    public GameObject insideRoomPanel;
    public Button startGameButton;

    private Dictionary<string, EventsAction> _onActivePanel = new Dictionary<string, EventsAction>();
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        inputPlayerName.text = "Player " + (int)Rand.Range(1000, 10000);
        //loadBalancingClient.ConnectToRegionMaster("us");
    }

    private void Start()
    {
        _onActivePanel.Add(Login, new EventsAction());
        _onActivePanel[Login].Add(() => {
            DisableAllPanels();
            loginPanel.SetActive(true);
        });

        _onActivePanel.Add(Selection, new EventsAction());
        _onActivePanel[Selection].Add(() => {
            DisableAllPanels();
            selectionPanel.SetActive(true);
        });

        _onActivePanel.Add(CreateRoom, new EventsAction());
        _onActivePanel[CreateRoom].Add(() => {
            DisableAllPanels();
            createRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(RandomRoom, new EventsAction());
        _onActivePanel[RandomRoom].Add(() => {
            DisableAllPanels();
            randomRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(ListRooms, new EventsAction());
        _onActivePanel[ListRooms].Add(() => {
            DisableAllPanels();
            listRoomPanel.SetActive(true);
        });

        _onActivePanel.Add(InsideRoom, new EventsAction());
        _onActivePanel[InsideRoom].Add(() => {
            DisableAllPanels();
            insideRoomPanel.SetActive(true);
        });
        
        _onActivePanel.Add(HowToPlay, new EventsAction());
        _onActivePanel[HowToPlay].Add(() => {
            DisableAllPanels();
            howToPlayPanel.SetActive(true);
        });
        
        _onActivePanel.Add(Credits, new EventsAction());
        _onActivePanel[Credits].Add(() => {
            DisableAllPanels();
            creditsPanel.SetActive(true);
        });
        
        _onActivePanel.Add(Loading, new EventsAction());
        _onActivePanel[Loading].Add(() => {
            DisableAllPanels();
            LoadingPanel.SetActive(true);
        });
    }

    private void DisableAllPanels()
    {
        loginPanel.SetActive(false);
        selectionPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        randomRoomPanel.SetActive(false);
        listRoomPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        creditsPanel.SetActive(false);
        insideRoomPanel.SetActive(false);
        LoadingPanel.SetActive(false);
    }

    public void SetActivePanel(string activePanel)
    {
        _onActivePanel[activePanel].Execute();
    }

    #region INSIDE-ROOM
    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient /*|| PhotonNetwork.PlayerList.Length < 2*/) return false;

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
    #endregion INSIDE-ROOM

    #region ROOM-LIST
    private void UpdateRoomListView()
    {
        foreach (var info in cachedRoomList.Values)
        {
            var objectToInstantiate = Resources.Load(PathListObjectRoom);
            var entry = Instantiate((GameObject)objectToInstantiate);
            entry.transform.SetParent(roomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<UserRoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void ClearRoomListView()
    {
        foreach (var entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }
    #endregion ROOM-LIST

    #region PUN-CALLBACKS
    public override void OnConnectedToMaster()
    {
        this.SetActivePanel(Selection);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();

        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(Selection);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(Selection);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + (int)Rand.Range(1000, 10000);

        RoomOptions options = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        SetActivePanel(InsideRoom);

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            var objectToInstantiate = Resources.Load(PathInsideRoomPlayer);
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
        SetActivePanel(Selection);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        var objectToInstantiate = Resources.Load(PathInsideRoomPlayer);
        GameObject entry = Instantiate((GameObject)objectToInstantiate);
        //Players canvas in gameObject
        var canvasPlayers = insideRoomPanel.transform.GetChild(0);
        entry.transform.SetParent(canvasPlayers.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<UserPlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            startGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(UserGame.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<UserPlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        startGameButton.gameObject.SetActive(CheckPlayersReady());
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

        SetActivePanel(Selection);
    }

    public void OnLeaveGameButton()
    {
        //Execute -> LeftRoom callback
        PhotonNetwork.LeaveRoom();
    }

    public void OnCreateRoomButton()
    {
        string roomName = inputRoomName.text;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + (int)Rand.Range(1000, 10000) : roomName;

        byte maxPlayers;
        byte.TryParse(inputMaxPlayers.text, out maxPlayers);
        maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        //Execute -> OnJoinedRoom callback
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnRandomRoomButton()
    {
        SetActivePanel(RandomRoom);
        //Execute -> OnJoinedRoom callback
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnRoomListButton()
    {
        if (!PhotonNetwork.InLobby)
        {
            //Execute ->  callback
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(ListRooms);
    }

    public void OnStartGameButton(string scene)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel(scene);
        StartCoroutine ("LoadScene");
    }

    private IEnumerator LoadScene()
    {
        while (PhotonNetwork.LevelLoadingProgress < 0.98f)
        {
            progress.SetProgress(PhotonNetwork.LevelLoadingProgress);
            progress.SetText((PhotonNetwork.LevelLoadingProgress * 100).ToString("F0") + "%"); 
            yield return null;
        }
        LoadingPanel.SetActive (false);
    }

    #endregion UI
}
