using System;
using System.Collections;
using Consts;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class ManagerGame : MonoBehaviourPunCallbacks, IObserverEventDead, IObserverEventDefeated, IObserverEventWinner {
    public GameObject canvasUIFinished;
    public Text text;
    
    private void Awake() {
        Application.targetFrameRate = 60;
        PhotonNetwork.SendRate = 60;
    }

    private void Start() {
        Hashtable props = new Hashtable {
            {UserGame.PLAYER_LOADED_LEVEL, true}
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        StartCoroutine("InitialSequenceForStart");
    }
    
    private IEnumerator InitialSequenceForStart() {
        canvasUIFinished.SetActive(true);
        var player = InitialSpawnPlayer();
        player.SubscribeEventDead(this);
        player.Controllable = false;
        text.text = "Waiting for other players...";
        while (!CheckAllPlayerLoadedLevel()) {
            yield return new WaitForSeconds(3);
        }

        text.text = "Count down timer";
        var countDown = 5;
        while (countDown > 1) {
            countDown -= 1;
            text.text = $"{countDown}";
            yield return new WaitForSeconds(1);
        }
        
        text.text = "Ready Go!!";
        player.Controllable = true;
        yield return new WaitForSeconds(3);
        
        canvasUIFinished.SetActive(false);
    }

    private bool CheckAllPlayerLoadedLevel() {
        foreach (var p in PhotonNetwork.PlayerList) {
            object playerLoadedLevel;

            if (!p.CustomProperties.TryGetValue(UserGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel)) {
                return false;
            }
            
            if ((bool) playerLoadedLevel) {
                continue;
            }

            return false;
        }

        return true;
    }

    private EntityPlayer InitialSpawnPlayer() {
        var index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        var initialPosition = ManagerPositions.Instance.GetPosition(index);
        var rotation = Quaternion.Euler(ManagerPositions.Instance.transform.forward);
        int randomAirplaneModel = Random.Range(1, 5);

        return PhotonNetwork.Instantiate(
            $"Prefabs/EntityPlayer{randomAirplaneModel.ToString()}",
            initialPosition,
            rotation,
            0
        ).GetComponent<EntityPlayer>();
    }

    private void CheckEndOfGame() {
        var allDestroyed = true;
        foreach (Player p in PhotonNetwork.PlayerList) {
            object lives;
            if (p.CustomProperties.TryGetValue(UserGame.PLAYER_LIVES, out lives)) {
                if ((int) lives <= 0) continue;
                allDestroyed = false;
                break;
            }
        }

        if (!allDestroyed) return;

        if (PhotonNetwork.IsMasterClient) {
            StopAllCoroutines();
        }

        //Reset the values.
        string winner = "";
        int score = -1;

        foreach (Player p in PhotonNetwork.PlayerList) {
            if (p.GetScore() > score) {
                winner = p.NickName;
            }
        }

        StartCoroutine(EndOfGame(winner, false));
    }

    #region PUN-CALLBACKS

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        CheckEndOfGame();
    }

    private IEnumerator EndOfGame(string player, bool isWinner) {
        canvasUIFinished.SetActive(true);
        var textToShow = isWinner ? "winner: " : "Loser: ";
        text.text = $"{textToShow}{player}";
        
        yield return new WaitForSeconds(3);

        canvasUIFinished.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
    }

    #endregion PUN-CALLBACKS

    #region OBSERVER-EVENT-DEAD
    public void EventDead() {
        //TODO: Assign the camera to another player
        //TODO: Listen the input for get out of race 
        StartCoroutine("GamerOverByDead");
    }

    private IEnumerator GamerOverByDead() {
        canvasUIFinished.SetActive(true);
        text.text = "You are dead";
        
        yield return new WaitForSeconds(3);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
    }

    #endregion OBSERVER-EVENT-DEAD

    #region OBSERVER-EVENT-DEFEATED
    public void EventDefeated() {
        StartCoroutine("GamerOverByDefeated");
    }
    
    private IEnumerator GamerOverByDefeated() {
        canvasUIFinished.SetActive(true);
        text.text = "You has been defeated";
        
        yield return new WaitForSeconds(3);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
    }

    #endregion OBSERVER-EVENT-DEFEATED
    
    #region OBSERVER-EVENT-WINNER
    public void EventWinner() {
        StartCoroutine("HaveWon");
    }
    
    private IEnumerator HaveWon() {
        canvasUIFinished.SetActive(true);
        text.text = "You have won";
        
        yield return new WaitForSeconds(3);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
    }
    #endregion OBSERVER-EVENT-WINNER
}