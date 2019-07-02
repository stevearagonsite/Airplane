using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRigibody : IMoveEntityPlayer
{
    private float _mommentForwardSpeed;
    private float _mommentHorizontalSpeed;
    private float _mommentVerticalSpeed;
    private float _mommentRotationSpeed;

    private float _forwardMaxSpeed = 90f;
    private float _horizontalMaxSpeed = 100f;
    private float _verticalMaxSpeed = 80f;
    private float _rotationMaxSpeed = 80f;

    private float _incrementalForwardAccelerate = 6f;
    private float _incrementalVerticalAccelerate = 6f;
    private float _incrementalHorizontalAccelerate = 6f;
    private float _incrementalRotationAccelerate = 6f;

    public float MoveForward(float speed)
    {
        if (speed > 0)
        {
            _mommentForwardSpeed = (_mommentForwardSpeed < _forwardMaxSpeed)
                ? _mommentForwardSpeed + _incrementalForwardAccelerate * Time.deltaTime
                : _forwardMaxSpeed;
        }
        else
        {
            var decreaseValue = _incrementalForwardAccelerate * 0.6f;
            _mommentForwardSpeed = _mommentForwardSpeed > 0
                ? _mommentForwardSpeed - decreaseValue * Time.deltaTime
                : 0;
        }

        return _mommentForwardSpeed;
    }

    public float MoveHorizontal(float speed)
    {
        if (speed > 0)
        {
            _mommentHorizontalSpeed = _mommentHorizontalSpeed < _horizontalMaxSpeed
                ? _mommentHorizontalSpeed + _incrementalHorizontalAccelerate * Time.deltaTime
                : _horizontalMaxSpeed;
        }
        
        if (speed < 0)
        {
            _mommentHorizontalSpeed = _mommentHorizontalSpeed > -_horizontalMaxSpeed
                ? _mommentHorizontalSpeed - _incrementalHorizontalAccelerate * Time.deltaTime
                : -_horizontalMaxSpeed;
        }
        
        if (speed == 0)
        {
            var decreaseValue = _incrementalHorizontalAccelerate * 0.6f;
            _mommentHorizontalSpeed = _mommentHorizontalSpeed > 0
                ? _mommentHorizontalSpeed - decreaseValue * Time.deltaTime
                : 0;
        }

        return _mommentHorizontalSpeed;
    }

    public float MoveVertical(float speed)
    {
        if (speed > 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed < _verticalMaxSpeed
                ? _mommentVerticalSpeed + _incrementalVerticalAccelerate * Time.deltaTime
                : _verticalMaxSpeed;
        }
        
        if (speed < 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed > -_verticalMaxSpeed
                ? _mommentVerticalSpeed - _incrementalVerticalAccelerate * Time.deltaTime
                : -_verticalMaxSpeed;
        }
        
        if (speed == 0)
        {
            var decreaseValue = _incrementalVerticalAccelerate * 0.6f;
            _mommentVerticalSpeed = _mommentVerticalSpeed > 0
                ? _mommentVerticalSpeed - decreaseValue * Time.deltaTime
                : 0;
        }

        return _mommentVerticalSpeed;
    }

    public float MoveRotation(float speed)
    {
        if (speed > 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed < _rotationMaxSpeed
                ? _mommentRotationSpeed + _incrementalRotationAccelerate * Time.deltaTime
                : _rotationMaxSpeed;
        }
        
        if (speed < 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed > -_rotationMaxSpeed
                ? _mommentRotationSpeed - _incrementalRotationAccelerate * Time.deltaTime
                : -_rotationMaxSpeed;
        }
        
        if (speed == 0)
        {
            var decreaseValue = _incrementalRotationAccelerate * 0.6f;
            _mommentRotationSpeed = _mommentRotationSpeed > 0
                ? _mommentRotationSpeed - decreaseValue * Time.deltaTime
                : 0;
        }

        return _mommentRotationSpeed;
    }
}