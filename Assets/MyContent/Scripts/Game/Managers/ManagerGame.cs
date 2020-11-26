using System.Collections;
using System.Collections.Generic;
using Consts;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ManagerGame : MonoBehaviourPunCallbacks, IObserverEventDead, IObserverEventDefeated, IObserverEventWinner {
    [FormerlySerializedAs("CanvasUI")] 
    public GameObject canvasUI;
    public Text text;
    public TriggerWinner triggerWinner;
    private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> playerListCharactersModel = new Dictionary<int, GameObject>();

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
        triggerWinner.SubscribeEventWinner(this);
        triggerWinner.SubscribeEventDefeated(this);
    }

    private void OnDestroy() {
        triggerWinner.UnSubscribeEventWinner(this);
        triggerWinner.UnSubscribeEventDefeated(this);
    }

    private void ExitOfRace() {
        StopAllCoroutines();
        
        // Reset player
        Hashtable initialProps = new Hashtable() {
            { UserGame.PLAYER_READY, false } ,
            { UserGame.PLAYER_LIVES, UserGame.PLAYER_MAX_LIVES }
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
        PhotonNetwork.LocalPlayer.SetScore(0);
        
        // Return to main menu
        PhotonNetwork.LeaveRoom();
    }
    
    private IEnumerator InitialSequenceForStart() {
        canvasUI.SetActive(true);
        var player = InitialSpawnPlayer();

        player.SubscribeEventDead(this);
        player.Controllable = false;
        text.text = "Waiting for other players...";
        while (!CheckAllPlayerLoadedLevel()) {
            yield return new WaitForSeconds(0.3f);
        }

        GetPlayerListEntries();

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
        
        canvasUI.SetActive(false);
    }

    private void GetPlayerListEntries() {
        var entities = FindObjectsOfType<EntityPlayer>();
        foreach (var entity in entities) {
            var entry = entity.gameObject;
            playerListEntries.Add(entity.PhotonPlayer.ActorNumber, entry);
            // playerListCharactersModel.Add(entity.PhotonPlayer.ActorNumber, entity.Character.gameObject);
        }
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
        var randomAirplaneModel = Random.Range(1, 5);
        var randomCharacterModel = Random.Range(1, 6);

        var entityPlayer = PhotonNetwork.Instantiate(
            $"Prefabs/EntityPlayer{randomAirplaneModel.ToString()}",
            initialPosition,
            rotation,
            0
        ).GetComponent<EntityPlayer>();
        
        var characterModel = PhotonNetwork.Instantiate(
            $"Characters/Character0{randomCharacterModel.ToString()}",
            initialPosition,
            rotation,
            0
        ).GetComponent<CharacterModel>();
        entityPlayer.Character = characterModel;
        characterModel.Entity = entityPlayer;
        
        return entityPlayer;
    }


    #region PUN-CALLBACKS

    public override void OnLeftRoom() {
        foreach (GameObject entry in playerListEntries.Values) {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
        
        PhotonNetwork.LoadLevel("MenuLobby");
        base.OnLeftRoom();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (playerListEntries == null) {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        // TryGetValue 
        var playerExist = playerListEntries.TryGetValue(otherPlayer.ActorNumber,out var player);
        if (!playerExist) return;
        Destroy(player.gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            entry.GetComponent<Text>().text = string.Format("{0}\nScore: {1}\nLives: {2}", targetPlayer.NickName, targetPlayer.GetScore(), targetPlayer.CustomProperties[AsteroidsGame.PLAYER_LIVES]);
        }
    }
    #endregion PUN-CALLBACKS

    #region OBSERVER-EVENT-DEAD
    public void EventDead() {
        //TODO: Assign the camera to another player
        //TODO: Listen the input for get out of race 
        StartCoroutine("GamerOverByDead");
    }

    private IEnumerator GamerOverByDead() {
        canvasUI.SetActive(true);
        text.text = "You are dead";
        
        yield return new WaitForSeconds(3);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
    }

    #endregion OBSERVER-EVENT-DEAD

    #region OBSERVER-EVENT-DEFEATED
    public void EventDefeated() {
        canvasUI.SetActive(true);
        text.text = "You has been defeated";
        StartCoroutine("TimeToExit");
    }

    #endregion OBSERVER-EVENT-DEFEATED
    
    #region OBSERVER-EVENT-WINNER
    public void EventWinner() {
        canvasUI.SetActive(true);
        text.text = "You have won";
        StartCoroutine("TimeToExit");
    }
    
    #endregion OBSERVER-EVENT-WINNER
    
    private IEnumerator TimeToExit() {
        var timer = 3.0f;
        while (timer > 0.0f)
        {
            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }
        ExitOfRace();
    }
}