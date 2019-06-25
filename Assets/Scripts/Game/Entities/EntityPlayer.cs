using Photon.Pun;
using UnityEngine;

using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Consts;
using Utils;
using System.Collections;

public class EntityPlayer : Entity
{
    public float rotationSpeed = 90.0f;
    public float movementSpeed = 2.0f;
    public float maxSpeed = 0.2f;

    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Collider _collider;

    private float _rotation = 0.0f;
    private float _acceleration = 0.0f;

    private bool _isAccelerating = false;
    private bool _controllable = true;

    private void Start()
    {
        this._photonView = this.gameObject.GetComponent<PhotonView>();
        this._rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Debug.Log(_photonView);
        Debug.Log(_rigidbody);
    }

    public override void Execution()
    {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        _rotation = Input.GetAxisRaw("Horizontal");
        _acceleration = Input.GetAxisRaw("Vertical");
        Input.GetButtonDown("Jump");
    }

    public override void FixedExecution()
    {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        Move();
    }

    public override void Move()
    {
        transform.position += (transform.right * _rotation) * movementSpeed * Time.deltaTime;
        transform.position += (transform.forward * _acceleration) * movementSpeed * Time.deltaTime;

        if (_isAccelerating)
        {
            //photonView.RPC("Fire", RpcTarget.AllViaServer, rigidbody.position, rigidbody.rotation);
        }
    }

    private IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(UserGame.PLAYER_RESPAWN_TIME);

        _photonView.RPC("RespawnSpaceship", RpcTarget.AllViaServer);
    }

    #region PUN-CALLBACKS
    [PunRPC]
    public void RespawnSpaceship()
    {
        _collider.enabled = true;
        _renderer.enabled = true;

        _controllable = true;
    }

    [PunRPC]
    public void DestroySpaceship()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _collider.enabled = false;
        _renderer.enabled = false;
        _controllable = false;

        if (_photonView.IsMine)
        {
            object lives;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(UserGame.PLAYER_LIVES, out lives))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(
                    new Hashtable {
                        { UserGame.PLAYER_LIVES, ((int)lives <= 1) ? 0 : ((int)lives - 1) }
                    }
                );

                if (((int)lives) > 1)
                {
                    StartCoroutine("WaitForRespawn");
                }
            }
        }
    }
    #endregion PUN-CALLBACKS
}
