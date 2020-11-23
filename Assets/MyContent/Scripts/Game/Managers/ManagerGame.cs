using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using Consts;
using UnityEngine.UI;
using Utils;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class ManagerGame : MonoBehaviourPunCallbacks
{
    public GameObject canvasUIFinished;
    public Text text;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        PhotonNetwork.SendRate = 60;
    }

    private void Start()
    {
        Hashtable props = new Hashtable
        {
            {UserGame.PLAYER_LOADED_LEVEL, true}
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        TriggerWinner.Instance.eventHaveWinner += ExecuteWinner;
        TriggerWinner.Instance.eventHaveLosers += ExecuteLoser;

        StartCoroutine(InitialTimeToStart());
    }

    private void ExecuteWinner()
    {
        Debug.Log("Winner Winner!!");
        string player = PhotonNetwork.LocalPlayer.NickName;
        StartCoroutine(EndOfGame(player, true));
    }

    private void ExecuteLoser()
    {
        string player = PhotonNetwork.LocalPlayer.NickName;
        StartCoroutine(EndOfGame(player, false));
    }

    private IEnumerator InitialTimeToStart()
    {
        //InfoText.text = "Waiting for other players...";
        while (!CheckAllPlayerLoadedLevel())
        {
            yield return new WaitForSeconds(3);
        }
        //InfoText.text = "Count down timer";

        StartGame();
    }
    
    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (p.CustomProperties.TryGetValue(UserGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
            {
                if ((bool) playerLoadedLevel)
                {
                    continue;
                }
            }
            return false;
        }
        return true;
    }

    private void StartGame()
    {
        var index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        var initialPosition = ManagerPositions.Instance.GetPosition(index);
        var rotation = Quaternion.Euler(ManagerPositions.Instance.transform.forward);
        int randomAirplaneModel = Random.Range(1,5);

        PhotonNetwork.Instantiate(
            $"Prefabs/EntityPlayer{randomAirplaneModel.ToString()}", 
            initialPosition, 
            rotation, 
            0
            );
    }
    
    private void CheckEndOfGame()
    {
        var allDestroyed = true;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object lives;
            if (p.CustomProperties.TryGetValue(UserGame.PLAYER_LIVES, out lives))
            {
                if ((int) lives > 0)
                {
                    allDestroyed = false;
                    break;
                }
            }
        }

        if (allDestroyed)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            //Reset the values.
            string winner = "";
            int score = -1;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.GetScore() > score)
                {
                    winner = p.NickName;
                }
            }

            StartCoroutine(EndOfGame(winner, false));
        }
    }

    #region PUN-CALLBACKS

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckEndOfGame();
    }
    
    private IEnumerator EndOfGame(string player, bool isWinner)
    {
        canvasUIFinished.SetActive(true);
        var timer = 5.0f;
        var textToShow = isWinner ? "winner: " : "Loser: ";
        text.text = $"{textToShow}{player}";
        
        while (timer > 0.0f)
        {
            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MenuLobby");
        canvasUIFinished.SetActive(false);
    }

    #endregion PUN-CALLBACKS
}