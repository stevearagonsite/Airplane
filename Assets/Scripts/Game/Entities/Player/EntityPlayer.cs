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
using Boo.Lang;
using Memento;

public class EntityPlayer : Entity, IApplyMemento<Tuple<Vector3, Quaternion>>
{
    private const string AnimatorHorizontalMove = "HorizontalMove";
    private const string AnimatorVerticalMove = "VerticalMove";
    private const string AnimatorIdleHorizontalMove = "IdleHorizontalMove";
    private const string AnimatorIdleVerticalMove = "IdleVerticalMove";
    private const string ControllerVertical = "Vertical";
    private const string ControllerHorizontal = "Horizontal";
    private const string ControllerRotation = "Rotation";
    private const string PathExplotion = "Prefabs/WFXMR_Nuke";

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

    private Quaternion _factorRotation = new Quaternion();
    private Vector3 _factorPosition = Vector3.zero;


    // Memento pattern
    private Caretaker<Memento<Tuple<Vector3, Quaternion>>> caretaker = new Caretaker<Memento<Tuple<Vector3, Quaternion>>>(3);

    private Originator<Tuple<Vector3, Quaternion>> originator = new Originator<Tuple<Vector3, Quaternion>>();
    private int currentArticle = 0;
    private const float TimeToSaveState = 3;
    private float _currentTimeToSaveState;

    private void Start()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponent<Animator>();
        _terrainChecker = GetComponentInChildren<TerrainChecker>();
        _text = FindObjectOfType<Text>();

        InitialOperationsOwner();
    }

    #region MEMENTO
    public void Save(Tuple<Vector3, Quaternion> state)
    {
        originator.Set(state);
        caretaker.Add(originator.StoreInMemento());
        currentArticle = caretaker.Count;
    }

    public Tuple<Vector3, Quaternion> UnDo()
    {
        if (currentArticle > 0) currentArticle -= 1;

        var prev = caretaker.Get(currentArticle);
        var prevArticle = originator.RestoreFromMemento(prev);
        return prevArticle;
    }

    public Tuple<Vector3, Quaternion> ReDo()
    {
        if (currentArticle < caretaker.Count) currentArticle += 1;

        var next = caretaker.Get(currentArticle);
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


    private void InitialOperationsOwner()
    {
        if (!_photonView.IsMine) return;

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
        _text.text = "Player debuging \n";
        _text.text += "Framerate;" + 1.0f / Time.deltaTime + "  \n";
        _text.text += "Player states \n";
        _text.text += "count" + caretaker.Count + "  \n";
        MoveVertical();
        MoveHorizontal();
        MoveForward();
        Grabity();
        SaveState();
        
        if (_mommentSpeed > 35)
        {
            if (!_cameraControl.zollyView) _cameraControl.SetZollyFX(true);
            //RotationForward();
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
            var decreaseValue = incrementalAccelerate * 1.5f;
            _mommentSpeed = (_mommentSpeed > 0) ? _mommentSpeed - decreaseValue * Time.deltaTime : 0;
            _text.text += "decreases" + "\n";
            _text.text += "IncrementalValues: " + _mommentSpeed + "\n";
        }

        _rigidbody.velocity = transform.forward * _mommentSpeed;
    }

    /*private void RotationForward()
    {
        var inputRotationH = Input.GetAxisRaw(ControllerRotation);
        //_animator.SetFloat(AnimatorHorizontalMove, inputRotationH);
        if (inputRotationH != 0)
        {
            //_animator.SetBool(AnimatorIdleHorizontalMove, false);
            MommentRotationForward(inputRotationH);
        }
        else
        {
            //_animator.SetBool(AnimatorIdleHorizontalMove, true);
        }
    }*/

    /*private void MommentRotationForward(float input)
    {
        var eulerAngleVelocity = transform.forward * input;
        var deltaRotation = Quaternion.Euler(eulerAngleVelocity * ((rotationSpeed / 2)  * Time.deltaTime));
        _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
    }*/

    private void OnCollisionEnter(Collision c)
    {
        var layer = c.gameObject.layer;
        if (layer == Layers.TERRAIN_NUM_LAYER || layer == Layers.WALLS_NUM_LAYER)
        {
            var force = Vector3.Magnitude(c.impulse); //Force crash
            if (force > UserGame.PLAYER_FORCE_TO_EXPLOTION)
            {
                PhotonNetwork.Instantiate(PathExplotion, transform.position, transform.rotation);
                var savedData = LastDo();
                
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _mommentSpeed = 5;
                transform.position = savedData.Item1;
                transform.rotation = savedData.Item2;
            }
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