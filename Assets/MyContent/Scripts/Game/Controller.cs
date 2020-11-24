using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller Instance;
    private const string InputNameForward = "Forward";
    private const string InputNameVertical = "Vertical";
    private const string InputNameHorizontal = "Horizontal";
    private const string InputNameRotation = "Rotation";

    public float ForwardValue { get; private set; }
    public float HorizontalValue { get; private set; }
    public float VerticalValue { get; private set; }
    public float RotationValue { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            Instance = this;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ManagerUpdate.Instance.Execute += Execute;
    }

    void Execute()
    {
        //TODO: Chance forward value for a toggle input
        ForwardValue = Input.GetKey(KeyCode.Space) ? 1 : 0;
        HorizontalValue = Input.GetAxisRaw(InputNameHorizontal);
        VerticalValue = Input.GetAxisRaw(InputNameVertical);
        RotationValue = Input.GetAxisRaw(InputNameRotation);
    }
}