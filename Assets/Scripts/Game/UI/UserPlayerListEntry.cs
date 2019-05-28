using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using TMPro;

using Photon.Pun;
using Consts;

public class UserPlayerListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerNameText;
    // public Image playerColorImage;

    public Button playerReadyButton;
    public Image playerReadyImage;

    private int _ownerId;
    private bool _isPlayerReady;

    public void OnEnable()
    {
        PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
    }

    public void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != _ownerId)
        {
            playerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            // Initial properties.
            Hashtable initialProps = new Hashtable() {
                { UserGame.PLAYER_READY, _isPlayerReady } ,
                { UserGame.PLAYER_LIVES, UserGame.PLAYER_MAX_LIVES }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);

            playerReadyButton.onClick.AddListener(() =>
            {
                _isPlayerReady = !_isPlayerReady;
                SetPlayerReady(_isPlayerReady);

                // Initial changes state ready.
                Hashtable props = new Hashtable() {
                    { UserGame.PLAYER_READY, _isPlayerReady }
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<UserLobbyPanels>().LocalPlayerPropertiesUpdated();
                }
            });
        }
    }

    public void Initialize(int playerId, string playerName)
    {
        _ownerId = playerId;
        playerNameText.text = playerName;
    }

    private void OnPlayerNumberingChanged()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == _ownerId)
            {
                //Personalize the player with a color.
                //playerColorImage.color = Color.random(p.GetPlayerNumber());
            }
        }
    }

    public void SetPlayerReady(bool playerReady)
    {
        var playerIsReady = playerReady ? "Ready" : "Pending";
        playerReadyButton.GetComponentInChildren<TextMeshProUGUI>().SetText(playerIsReady);
        playerReadyImage.enabled = playerReady;
    }
}
