using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using Consts;
using Utils;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ManagerGame : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Hashtable props = new Hashtable
        {
            {UserGame.PLAYER_LOADED_LEVEL, true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        StartCoroutine(InitialTimeToStart());
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

        PhotonNetwork.Instantiate("Prefabs/EntityPlayer", initialPosition, rotation, 0);
    }
}