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
    private Dictionary<int, GameObject> _playerListEntries;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _playerListEntries = new Dictionary<int, GameObject>();

        //InfoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable
        {
            {UserGame.PLAYER_LOADED_LEVEL, true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void Start()
    {
        StartCoroutine(InitialTimeToStart());
    }

    private IEnumerator InitialTimeToStart()
    {
        yield return new WaitForSeconds(3);
        StartGame();
    }

    private void StartGame()
    {
        var index = Math.Abs(PhotonNetwork.LocalPlayer.GetPlayerNumber());
        var initialPosition = ManagerPositions.Instance.GetPosition(index);
        var rotation = Quaternion.Euler(ManagerPositions.Instance.transform.forward);

        PhotonNetwork.Instantiate("Prefabs/EntityPlayer", initialPosition, rotation, 0);
    }
}
