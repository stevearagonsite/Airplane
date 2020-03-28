using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Memento;
using Consts;
using Utils;

public class EntityPlayer : Entity, IApplyMemento<Tuple<Vector3, Quaternion>>
{
    private const string AnimatorHorizontalMove = "HorizontalMove";
    private const string AnimatorVerticalMove = "VerticalMove";
    private const string AnimatorIdleHorizontalMove = "IdleHorizontalMove";
    private const string AnimatorIdleVerticalMove = "IdleVerticalMove";
    private const string PathExplotion = "Prefabs/WFXMR_Nuke";
    private const string MoveBehaviorPlayerNormal = "normal";

    [Header("Player parameters")]
    public GameObject trailControl;
    public GameObject model;
    
    private bool _controllable = true;
    private float _timeGrabity;
    private bool _isAccelerating;
    private float _mommentSpeed;

    private Rigidbody _rigidbody;
    private List<Collider> _colliders;
    private Animator _animator;
    private TerrainChecker _terrainChecker;
    private CameraControl _cameraControl;

    // Memento pattern
    private Caretaker<Memento<Tuple<Vector3, Quaternion>>> caretaker =
        new Caretaker<Memento<Tuple<Vector3, Quaternion>>>(6);

    private Originator<Tuple<Vector3, Quaternion>> originator =
        new Originator<Tuple<Vector3, Quaternion>>();

    private int _currentArticle = 0;
    private const float TimeToSaveState = 0.5f;
    private float _currentTimeToSaveState;

    private void Start()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponent<Animator>();
        _colliders = gameObject.GetComponents<Collider>().ToList();

        if (!_photonView.IsMine) return;
        _moveBehaviors.Add(MoveBehaviorPlayerNormal, new MoveEntityPlayerNormal());
        _currentMove = _moveBehaviors[MoveBehaviorPlayerNormal];
        _terrainChecker = GetComponentInChildren<TerrainChecker>();
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

    private void SaveState()
    {
        _currentTimeToSaveState -= Time.deltaTime;
        if (_currentTimeToSaveState >= 0) return;

        _currentTimeToSaveState += TimeToSaveState;
        Save(new Tuple<Vector3, Quaternion>(transform.position, transform.rotation));
    }

    public override void Move()
    {
        Grabity();
        MoveForward(Controller.Instance.ForwardValue);
        MoveVertical(Controller.Instance.VerticalValue);
        MoveHorizontal(Controller.Instance.HorizontalValue);
        MoveRotation(Controller.Instance.RotationValue);
        SaveState();

        if (_mommentSpeed > 35)
        {
            if (!_cameraControl.zollyView) _cameraControl.SetZollyFX(true);
        }
        else
        {
            if (_cameraControl.zollyView) _cameraControl.SetZollyFX(true);
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

    private void MoveForward(float input)
    {
        var toMove = _currentMove.MoveForward(input);

        _mommentSpeed = toMove;
        _rigidbody.velocity = transform.forward * toMove;
    }

    private void MoveHorizontal(float input)
    {
        _animator.SetFloat(AnimatorHorizontalMove, input);
        var toMove = _currentMove.MoveHorizontal(input);

        if (input != 0)
        {
            _animator.SetBool(AnimatorIdleHorizontalMove, false);
            var eulerAngleVelocity = transform.rotation.eulerAngles + Vector3.up * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else
        {
            _animator.SetBool(AnimatorIdleHorizontalMove, true);
        }
    }

    private void MoveVertical(float input)
    {
        _animator.SetFloat(AnimatorVerticalMove, input);
        var toMove = _currentMove.MoveVertical(input);
        if (input != 0)
        {
            _animator.SetBool(AnimatorIdleVerticalMove, false);

            /*rotation object*/
            var eulerAngleVelocity = transform.rotation.eulerAngles - Vector3.right * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else
        {
            _animator.SetBool(AnimatorIdleVerticalMove, true);
        }
    }

    private void MoveRotation(float input)
    {
        //_animator.SetFloat(AnimatorVerticalMove, input);
        var toMove = _currentMove.MoveRotation(input);
        if (input != 0)
        {
            //_animator.SetBool(AnimatorIdleVerticalMove, false);
            var eulerAngleVelocity = transform.rotation.eulerAngles + Vector3.forward * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else
        {
            //_animator.SetBool(AnimatorIdleVerticalMove, true);
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        var layer = c.gameObject.layer;
        if (layer == Layers.TERRAIN_NUM_LAYER || layer == Layers.WALLS_NUM_LAYER)
        {
            var force = Vector3.Magnitude(c.impulse); //Force crash
            if (force > UserGame.PLAYER_FORCE_TO_EXPLOTION)
            {
                _photonView.RPC("DestroyAirplane", RpcTarget.AllViaServer);
            }
        }
    }

    #region MEMENTO

    public void Save(Tuple<Vector3, Quaternion> state)
    {
        originator.Set(state);
        caretaker.Add(originator.StoreInMemento());
        _currentArticle = caretaker.Count;
    }

    public Tuple<Vector3, Quaternion> UnDo()
    {
        if (_currentArticle > 0) _currentArticle -= 1;

        var prev = caretaker.Get(_currentArticle);
        var prevArticle = originator.RestoreFromMemento(prev);
        return prevArticle;
    }

    public Tuple<Vector3, Quaternion> ReDo()
    {
        if (_currentArticle < caretaker.Count) _currentArticle += 1;

        var next = caretaker.Get(_currentArticle);
        var nextArticle = originator.RestoreFromMemento(next);
        return nextArticle;
    }

    public Tuple<Vector3, Quaternion> LastDo()
    {
        var next = caretaker.Get(0);
        var nextArticle = originator.RestoreFromMemento(next);
        return nextArticle;
    }

    #endregion MEMENTO

    #region PUN-CALLBACKS

    [PunRPC]
    public void RespawnAirplane()
    {
        _cameraControl.target = transform;
        var savedData = LastDo();
        trailControl.SetActive(true);
        model.SetActive(true);
        foreach (var collider in _colliders)
        {
            collider.enabled = true;
        }
        _controllable = false;
        transform.position = savedData.Item1;
        transform.rotation = savedData.Item2;
    }

    [PunRPC]
    public void DestroyAirplane()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _currentMove.ResetMomments();
        model.SetActive(false);
        _cameraControl.target = null;

        PhotonNetwork.Instantiate(PathExplotion, transform.position, transform.rotation);

        trailControl.SetActive(false);
        foreach (var collider in _colliders)
        {
            collider.enabled = false;
        }
        _controllable = false;

        if (_photonView.IsMine)
        {
            object lives;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(UserGame.PLAYER_LIVES, out lives))
            {
                /*PhotonNetwork.LocalPlayer.SetCustomProperties(
                    new Hashtable
                    {
                        {UserGame.PLAYER_LIVES, ((int) lives <= 1) ? 0 : ((int) lives - 1)}
                    }
                );*/

                if (((int) lives) > 1)
                {
                    StartCoroutine("WaitForRespawn");
                }
                else
                {
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.LoadLevel("MenuLobby");
                }
            }
        }
    }

    private IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(UserGame.PLAYER_RESPAWN_TIME);

        _photonView.RPC("RespawnAirplane", RpcTarget.AllViaServer);
    }
    #endregion PUN-CALLBACKS
}