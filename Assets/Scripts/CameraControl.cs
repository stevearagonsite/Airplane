using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;
using Utils;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    [Range(0, 20)] public float speedPosition = 20f;
    [Range(0, 20)] public float speedRotation = 5f;
    [Range(0, 20)] public float smootZolly = 16;
    
    [Header("Zolly FX")]
    public Vector3 defaultOffset = new Vector3(0, 1f, 1f);
    public Vector3 zollyOffSet = new Vector3(0, 0.5f, 0.5f);

    [Range(0, 180)] public float defaultFieldView = 60f;
    [Range(0, 180)] public float zollyFieldOfView = 110f;
    
    private Vector3 _currentOffSet = new Vector3();

    
    public float currentFieldOfView { get; private set; }
    public bool updateIsPlaying { get; private set; }
    public bool zollyView { get; private set; }
    public bool _wallDetection { get; private set; }

    Camera _camera;

    void Start()
    {
        //Set initals state.
        _camera = GetComponent<Camera>();
        _camera.fieldOfView = defaultFieldView;
        _currentOffSet = defaultOffset;
        smootZolly = 1 / smootZolly;
        ManagerUpdate.Instance.ExecuteLate += ExecuteLate;
    }

    void OnDestroy()
    {
        ManagerUpdate.Instance.ExecuteLate -= ExecuteLate;
    }

    void ExecuteLate()
    {
        if (target)
        {
            TrackingCamera();
            WallDetection();
        }
    }

    ///<summary> With this function the camera can not cross walls. </summary> 
    void WallDetection()
    {
        RaycastHit rch;
        var dirToTarget = target.transform.position - transform.position;
        _wallDetection = Physics.Raycast(transform.position, dirToTarget, out rch, _currentOffSet.z,
            1 << Layers.WALLS_NUM_LAYER);

        if (_wallDetection)
        {
            _currentOffSet.z = Mathf.Abs(_currentOffSet.z + 0.3f - rch.distance);
        }
        else if (Vector3.Distance(transform.position, target.position) > 3)
        {
            _wallDetection = false;
        }
    }

    /// <summary> Keep the direccion and offset in rotation. </summary>
    void TrackingCamera()
    {
        //Smoot position
        var forwardP = target.forward * _currentOffSet.z;
        var rightP = target.right * _currentOffSet.x;
        var topP = target.up * _currentOffSet.y;
        var positionB = target.position - forwardP - rightP + topP;
        transform.position = Vector3.Slerp(transform.position, positionB, Time.deltaTime * speedPosition);

        //Smoot direction
        var targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speedRotation);
    }

    #region ZollyView

    /// <summary> Set zolly state. </summary> 
    public void SetZollyFX(bool state)
    {
        if (state)
        {
            updateIsPlaying = zollyView = true;
            StartCoroutine(EnableZollyView());
        }
        else if (!state)
        {
            updateIsPlaying = true;
            zollyView = false;
            StartCoroutine(DisableZollyView());
        }
    }

    IEnumerator DisableZollyView()
    {
        while (_camera.fieldOfView != defaultFieldView || _currentOffSet != defaultOffset)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, defaultFieldView, smootZolly);
            _currentOffSet = Vector3.Slerp(_currentOffSet, defaultOffset, smootZolly);
            if (zollyView)
                break;
            yield return null;
        }

        updateIsPlaying = false;
    }

    IEnumerator EnableZollyView()
    {
        while (_camera.fieldOfView != zollyFieldOfView || _currentOffSet != zollyOffSet)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, zollyFieldOfView, smootZolly);
            _currentOffSet = Vector3.Slerp(_currentOffSet, zollyOffSet, smootZolly);
            if (!zollyView)
                break;
            yield return null;
        }

        updateIsPlaying = false;
    }

    #endregion ZollyView
    void OnDrawGizmos()
    {
        Gizmos.color = _wallDetection ? Color.green : Color.yellow;
        Gizmos.DrawLine(transform.position, target.transform.position);
        Gizmos.color = updateIsPlaying ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}