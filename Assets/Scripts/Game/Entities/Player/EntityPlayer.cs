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
    private const string AnimatorHorizontalMove = "HorizontalMove";
    private const string AnimatorVerticalMove = "VerticalMove";
    private const string AnimatorIdleHorizontalMove = "IdleHorizontalMove";
    private const string AnimatorIdleVerticalMove = "IdleVerticalMove";
    private const string ControllerVertical = "Vertical";
    private const string ControllerHorizontal = "Horizontal";

    public float rotationSpeed = 90f;
    public float maxSpeed = 40f;
    public float incrementalAccelerate = 6f;

    private float _mommentSpeed = 0.0f;
    private bool _isAccelerating = false;
    private bool _controllable = true;
    private float _timeGrabity = 0;

    private Text _text;
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Collider _collider;
    private Animator _animator;
    private TerrainChecker _terrainChecker;
    private CameraControl _cameraControl;

    private void Start()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponent<Animator>();
        _terrainChecker = GetComponentInChildren<TerrainChecker>();
        _text = FindObjectOfType<Text>();

        InitialOperationsOwner();
    }

    private void InitialOperationsOwner()
    {
        if (!_photonView.IsMine) return;
        /*Basic Settings of camera.*/
        /*var camera = FindObjectOfType<Camera>().transform;
        var position = transform.position;
        
        camera.position = new Vector3(position.x, position.y + 1.3f, position.z - 5.8f);
        camera.rotation = transform.rotation;
        camera.LookAt(lookAt);
        camera.SetParent(gameObject.transform);*/

        _cameraControl = FindObjectOfType<CameraControl>();
        _cameraControl.target = GetComponentInChildren<LookAt>().transform;
    }

    protected override void Execution()
    {
    }

    protected override void FixedExecution()
    {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        Move();
    }

    public override void Move()
    {
        _text.text = "Player debuging \n";
        MoveVertical();
        MoveHorizontal();
        MoveForward();
        Grabity();
        
        if (_mommentSpeed > 30)
        {
            _cameraControl.SetZollyFX(true);
        }
        else
        {
            _cameraControl.SetZollyFX(false);
        }
    }

    private void Grabity()
    {
        if (!_terrainChecker.isTerrein)
        {
            var factor = Vector3.up * (9.8f * _rigidbody.mass * _timeGrabity) / 2000;
            _rigidbody.MovePosition(transform.position - factor * Time.deltaTime);
            TimeGrabity();
        }
        else
        {
            TimeGrabity();
        }
    }

    private void TimeGrabity()
    {
        if (!_isAccelerating && _mommentSpeed < 10)
        {
            _timeGrabity = _timeGrabity < 2 ? _timeGrabity + Time.deltaTime / 2 : 2;
            //_timeGrabity += Time.deltaTime;
        }
        else
        {
            _timeGrabity = 0;
        }
    }

    private void MoveVertical()
    {
        var inputRotationV = Input.GetAxisRaw(ControllerVertical);
        _animator.SetFloat(AnimatorVerticalMove, inputRotationV);
        if (inputRotationV != 0)
        {
            _animator.SetBool(AnimatorIdleVerticalMove, false);
            MommentMoveVertical(inputRotationV);
        }
        else
        {
            _animator.SetBool(AnimatorIdleVerticalMove, true);
        }
    }

    private void MommentMoveVertical(float input)
    {
        /*Rotation of object*/
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        var eulerAngleVelocity = -Vector3.right * input;
        var deltaRotation = Quaternion.Euler(eulerAngleVelocity * (rotationSpeed * Time.deltaTime));
        _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);

        /*Move of object*/
        var position = transform.position;
        var move = new Vector3(position.x, position.y + (input / 1000) * Time.deltaTime, position.z);
        _rigidbody.MovePosition(move);
    }

    private void MoveHorizontal()
    {
        var inputRotationH = Input.GetAxisRaw(ControllerHorizontal);
        _animator.SetFloat(AnimatorHorizontalMove, inputRotationH);
        if (inputRotationH != 0)
        {
            _animator.SetBool(AnimatorIdleHorizontalMove, false);
            MommentMoveHorizontal(inputRotationH);
        }
        else
        {
            _animator.SetBool(AnimatorIdleHorizontalMove, true);
        }
    }

    private void MommentMoveHorizontal(float input)
    {
        /*Rotation of object*/
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        var eulerAngleVelocity = transform.up * input;
        var deltaRotation = Quaternion.Euler(eulerAngleVelocity * (rotationSpeed * Time.deltaTime));
        _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);

        /*Move of object*/
        var position = transform.position;
        var move = new Vector3(position.x + (input / 1000) * Time.deltaTime, position.y, position.z);
        _rigidbody.MovePosition(move);
    }

    private void MoveForward()
    {
        _isAccelerating = Input.GetKey(KeyCode.Space);
        if (_isAccelerating)
        {
            _mommentSpeed = (_mommentSpeed < maxSpeed)
                ? _mommentSpeed + incrementalAccelerate * Time.deltaTime
                : maxSpeed;
            _text.text += "Increases" + "\n";
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
                    new Hashtable
                    {
                        {UserGame.PLAYER_LIVES, ((int) lives <= 1) ? 0 : ((int) lives - 1)}
                    }
                );

                if (((int) lives) > 1)
                {
                    StartCoroutine("WaitForRespawn");
                }
            }
        }
    }

    #endregion PUN-CALLBACKS
}