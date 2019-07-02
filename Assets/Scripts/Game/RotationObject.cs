using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RotationObject : MonoBehaviour {

    [Range(-100,100)] public float rotationSpeed;
    [Range(0, 1)] public float x,y,z;

    private void Start()
    {
        ManagerUpdate.Instance.Execute += Execute;
    }

    private void OnDestroy()
    {
        ManagerUpdate.Instance.Execute -= Execute;
    }
    
    void Execute()
    {
        transform.Rotate(new Vector3(x, y, z) * (Time.deltaTime * rotationSpeed * 10000));
    }
}
