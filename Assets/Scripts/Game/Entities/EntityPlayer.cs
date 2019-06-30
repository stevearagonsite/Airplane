using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Consts;
using Utils;
using System.Collections;
using System;

public class EntityPlayer : Entity
{
    public float rotationSpeed = 0.0003f;
    public float maxSpeed = 20f;
    private float incrementalAccelerate = 3f;

    public Transform lookAt;

    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Collider _collider;

    private float _mommentSpeed = 0.0f;
    private float _rotationHorizontal = 0.0f;
    private float _rotationVertical = 0.0f;
    private Text _text;

    private bool _isAccelerating = false;
    private bool _controllable = true;

    private void Start()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        InitialOperationsOwner();
    }

    private void InitialOperationsOwner()
    {
        if (!_photonView.IsMine) return;

        _text = FindObjectOfType<Text>();
        var camera = FindObjectOfType<Camera>();
        camera.transform.position = new Vector3(transform.position.x, transform.position.y + 1.3f, transform.position.z - 3.8f);
        camera.transform.rotation = transform.rotation;
        camera.transform.LookAt(lookAt);
        camera.transform.SetParent(gameObject.transform);
    }

    protected override void Execution()
    {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        //incrementalAccelerate /= 100;
    }

    protected override void FixedExecution()
    {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        Move();
    }

    public override void Move()
    {
        _text.text = "VALUES DEBUG!";
        Grabity();
        MoveVertical();
        MoveHorizontal();
        MoveForward();
    }

    private void Grabity()
    {
        var factor = (Vector3.up * 9.8f * _rigidbody.mass) / 2000;
        _rigidbody.MovePosition(transform.position - factor * Time.deltaTime);
    }

    private void MoveVertical()
    {
        var inputRotationV = Input.GetAxisRaw("Vertical");
        if (inputRotationV != 0)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            var eulerAngleVelocity = (-Vector3.right * inputRotationV);
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity * rotationSpeed * Time.deltaTime);
            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
        }
    }

    private void MoveHorizontal()
    {
        var inputRotationH = Input.GetAxisRaw("Horizontal");
        if (inputRotationH != 0)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            Vector3 eulerAngleVelocity = (transform.up * inputRotationH);
            Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * rotationSpeed * Time.deltaTime);
            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
        }
    }

    private void MoveForward()
    {
        _isAccelerating = Input.GetKey(KeyCode.Space);
        if (_isAccelerating)
        {
            _mommentSpeed = (_mommentSpeed < maxSpeed) ? _mommentSpeed + incrementalAccelerate * Time.deltaTime : maxSpeed;
            _text.text += "incremental" + "\n";
            _text.text += "IncrementalValues: " + _mommentSpeed + "\n";
        }
        else
        {
            var decreaseValue = incrementalAccelerate / 2;
            _mommentSpeed = (_mommentSpeed > 0) ? _mommentSpeed - decreaseValue * Time.deltaTime : 0;
            _text.text += "decreases" + "\n";
            _text.text += "IncrementalValues: " + _mommentSpeed + "\n";
        }

        _rigidbody.velocity = transform.forward * _mommentSpeed;
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
