using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Consts;
using Memento;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PhotonNetwork))]
[RequireComponent(typeof(Rigidbody))]
public class EntityPlayer : Entity, IObservableEventDead,
    IApplyMemento<Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>> {
    
    private const string AnimatorHorizontalMove = "HorizontalMove";
    private const string AnimatorVerticalMove = "VerticalMove";
    private const string AnimatorIdleHorizontalMove = "IdleHorizontalMove";
    private const string AnimatorIdleVerticalMove = "IdleVerticalMove";
    private const string PathVFXExplotion = "Prefabs/VFXExplotion";
    private const string MoveBehaviorPlayerNormal = "normal";
    
    [Header("Player parameters")] 
    public GameObject trailControl;
    public Transform characterPosition;
    public GameObject model;
    
    public CharacterModel Character { get; set; }

    private bool _controllable;
    private float _timeGravity;
    private bool _isAccelerating;
    private float _momentSpeed;

    private Rigidbody _rigidbody;
    private List<Collider> _colliders;
    private Animator _animator;
    private TerrainChecker _terrainChecker;
    private CollisionChecker _collisionChecker;
    private CameraControl _cameraControl;
    private event Action OnEventDead = delegate {};

    // Memento pattern
    private Caretaker<Memento<Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>>> caretaker =
        new Caretaker<Memento<Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>>>(6);

    private Originator<Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>> originator =
        new Originator<Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>>();

    private int _currentArticle = 0;
    private const float TimeToSaveState = 0.5f;
    private float _currentTimeToSaveState;

    public bool Controllable {
        get => _controllable;
        set => _controllable = value;
    }

    public bool isMime {
        get => this._photonView.IsMine;
    }
    
    public Player PhotonPlayer {
        get => this._photonView.Owner;
    }

    private void Start() {
        _photonView = gameObject.GetComponent<PhotonView>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponent<Animator>();
        _colliders = gameObject.GetComponents<Collider>().ToList();

        if (!_photonView.IsMine) return;
        _moveBehaviors.Add(MoveBehaviorPlayerNormal, new MoveEntityPlayerNormal());
        _currentMove = _moveBehaviors[MoveBehaviorPlayerNormal];
        _terrainChecker = GetComponentInChildren<TerrainChecker>();
        _collisionChecker = GetComponentInChildren<CollisionChecker>();
        _cameraControl = FindObjectOfType<CameraControl>();
        _cameraControl.target = GetComponentInChildren<LookAt>().transform;
    }

    protected override void Execution() {
    }

    protected override void FixedExecution() {
        if (!_photonView) return;
        if (!_photonView.IsMine || !_controllable) return;

        Move();
    }

    private void SaveState() {
        _currentTimeToSaveState -= Time.deltaTime;
        if (_currentTimeToSaveState >= 0) return;

        _currentTimeToSaveState += TimeToSaveState;
        Save(
            new Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float>(
                transform.position,
                transform.rotation,
                _rigidbody.velocity,
                _rigidbody.angularVelocity,
                _isAccelerating,
                _momentSpeed
            )
        );
    }

    public override void Move() {
        Gravity();
        MoveForward(Controller.Instance.ForwardValue);
        MoveVertical(Controller.Instance.VerticalValue);
        MoveHorizontal(Controller.Instance.HorizontalValue);
        MoveRotation(Controller.Instance.RotationValue);
        SaveState();

        if (_momentSpeed > 35) {
            if (!_cameraControl.zollyView) _cameraControl.SetZollyFX(true);
        }
        else {
            if (_cameraControl.zollyView) _cameraControl.SetZollyFX(false);
        }
    }

    private void Gravity() {
        if (_terrainChecker.isTerrain) {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
            ForceGravity();
            return;
        }

        ForceGravity();
        _rigidbody.constraints = RigidbodyConstraints.None;
        var factor = Vector3.up * (9.8f * _rigidbody.mass * _timeGravity) / 2000;
        _rigidbody.MovePosition(transform.position - factor * Time.deltaTime);
    }

    private void ForceGravity() {
        if ( !_isAccelerating && _momentSpeed < 3) {
            _timeGravity = _timeGravity < 2 ? _timeGravity + Time.deltaTime : 2;
            return;
        }

        _timeGravity = 0;
    }

    private void MoveForward(float input) {
        if (_collisionChecker.IsColliding) {
            return;
        }

        var toMove = _currentMove.MoveForward(input);
        _momentSpeed = toMove;

        
        if (_terrainChecker.isTerrain) {
            _rigidbody.velocity = (transform.forward * toMove) + (transform.up * toMove/2);
            return;
        }
        
        _rigidbody.velocity = transform.forward * toMove;
    }

    private void MoveHorizontal(float input) {
        _animator.SetFloat(AnimatorHorizontalMove, input);
        var toMove = _currentMove.MoveHorizontal(input);

        if (input != 0) {
            _animator.SetBool(AnimatorIdleHorizontalMove, false);
            var eulerAngleVelocity = transform.rotation.eulerAngles + Vector3.up * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else {
            _animator.SetBool(AnimatorIdleHorizontalMove, true);
        }
    }

    private void MoveVertical(float input) {
        _animator.SetFloat(AnimatorVerticalMove, input);
        var toMove = _currentMove.MoveVertical(input);
        if (input != 0) {
            _animator.SetBool(AnimatorIdleVerticalMove, false);

            /*rotation object*/
            var eulerAngleVelocity = transform.rotation.eulerAngles - Vector3.right * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else {
            _animator.SetBool(AnimatorIdleVerticalMove, true);
        }
    }

    private void MoveRotation(float input) {
        //_animator.SetFloat(AnimatorVerticalMove, input);
        var toMove = _currentMove.MoveRotation(input);
        if (input != 0) {
            //_animator.SetBool(AnimatorIdleVerticalMove, false);
            var eulerAngleVelocity = transform.rotation.eulerAngles + Vector3.forward * toMove;
            var deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            _rigidbody.MoveRotation(deltaRotation);
        }
        else {
            //_animator.SetBool(AnimatorIdleVerticalMove, true);
        }
    }

    private void OnCollisionEnter(Collision c) {
        var layer = c.gameObject.layer;
        if (layer == Layers.TERRAIN_NUM_LAYER || layer == Layers.WALLS_NUM_LAYER) {
            var force = Vector3.Magnitude(c.impulse); //Force crash
            if (force > UserGame.PLAYER_FORCE_TO_EXPLOTION) {
                _photonView.RPC("DestroyAirplane", RpcTarget.AllViaServer);
            }
        }
    }

    #region MEMENTO

    public void Save(Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float> state) {
        originator.Set(state);
        caretaker.Add(originator.StoreInMemento());
        _currentArticle = caretaker.Count;
    }

    public Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float> UnDo() {
        if (_currentArticle > 0) _currentArticle -= 1;

        var prev = caretaker.Get(_currentArticle);
        var prevArticle = originator.RestoreFromMemento(prev);
        return prevArticle;
    }

    public Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float> ReDo() {
        if (_currentArticle < caretaker.Count) _currentArticle += 1;

        var next = caretaker.Get(_currentArticle);
        var nextArticle = originator.RestoreFromMemento(next);
        return nextArticle;
    }

    public Tuple<Vector3, Quaternion, Vector3, Vector3, bool, float> LastDo() {
        var next = caretaker.Get(0);
        var nextArticle = originator.RestoreFromMemento(next);
        return nextArticle;
    }

    #endregion MEMENTO

    #region PUN-CALLBACKS

    [PunRPC]
    public void ReSpawnAirplane() {
        trailControl.SetActive(true);
        Character.gameObject.SetActive(true);
        model.SetActive(true);
        foreach (var collider in _colliders) {
            collider.enabled = true;
        }

        // Is my player ?
        if (!_photonView.IsMine) return;

        _cameraControl.target = transform;
        var (
            position,
            rotation,
            rbVelocity,
            rbAngularVelocity,
            isAccelerating,
            momentSpeed
            ) = LastDo();

        transform.position = position;
        transform.rotation = rotation;
        _rigidbody.velocity = rbVelocity;
        _rigidbody.angularVelocity = rbAngularVelocity;
        _momentSpeed = momentSpeed;
        _isAccelerating = isAccelerating;

        _controllable = true;
    }

    [PunRPC]
    public void DestroyAirplane() {
        model.SetActive(false);
        trailControl.SetActive(false);
        PhotonNetwork.Instantiate(PathVFXExplotion, transform.position, transform.rotation);
        Character.gameObject.SetActive(false);
        foreach (var collider in _colliders) {
            collider.enabled = false;
        }

        // Is my player ?
        if (!_photonView.IsMine) return;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _currentMove.ResetMomments();
        _cameraControl.target = null;

        _controllable = false;


        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(UserGame.PLAYER_LIVES, out var lives)) return;
        //TODO: Fix lives counter
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable {
                {UserGame.PLAYER_LIVES, ((int) lives <= 1) ? 0 : ((int) lives - 1)}
            }
        );

        if ((int) lives > 1) {
            StartCoroutine("WaitForSpawn");
            return;
        }
        
        OnEventDead();
    }
    
    private IEnumerator WaitForSpawn(){ 
        yield return new WaitForSeconds(UserGame.PLAYER_RESPAWN_TIME);

        _photonView.RPC("ReSpawnAirplane", RpcTarget.AllViaServer);
    }

    #endregion PUN-CALLBACKS

    #region OBSERVABLE-EVENT-DIED

    public void SubscribeEventDead(IObserverEventDead observer) {
        OnEventDead += observer.EventDead;
    }

    public void UnSubscribeEventDead(IObserverEventDead observer) {
        OnEventDead -= observer.EventDead;
    }

    #endregion OBSERVABLE-EVENT-DIED
}