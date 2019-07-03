using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEntityPlayerNormal : IMoveEntity
{
    private float _mommentForwardSpeed;
    private float _mommentHorizontalSpeed;
    private float _mommentVerticalSpeed;
    private float _mommentRotationSpeed;

    private float _forwardMaxSpeed = 90f;
    private float _horizontalMaxSpeed = 3f;
    private float _verticalMaxSpeed = 3f;
    private float _rotationMaxSpeed = 3f;

    private float _incrementalForwardAccelerate = 6f;
    private float _incrementalHorizontalAccelerate = 0.1f;
    private float _incrementalVerticalAccelerate = 0.05f;
    private float _incrementalRotationAccelerate = 0.03f;

    public void ResetMomments()
    {
        _mommentForwardSpeed = _mommentHorizontalSpeed = _mommentVerticalSpeed = _mommentRotationSpeed = 0;
    }

    public float MoveForward(float speed)
    {
        if (speed > 0)
        {
            _mommentForwardSpeed = _mommentForwardSpeed < _forwardMaxSpeed
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
                ? _mommentHorizontalSpeed + _incrementalHorizontalAccelerate
                : _horizontalMaxSpeed;
        }

        if (speed < 0)
        {
            _mommentHorizontalSpeed = _mommentHorizontalSpeed > -_horizontalMaxSpeed
                ? _mommentHorizontalSpeed - _incrementalHorizontalAccelerate
                : -_horizontalMaxSpeed;
        }

        if (speed == 0 && _mommentHorizontalSpeed > 0)
        {
            _mommentHorizontalSpeed = _mommentHorizontalSpeed < 0.3f ? 0.3f: _mommentHorizontalSpeed;
            MoveHorizontal(-0.3f);
        }else if (speed == 0 && _mommentHorizontalSpeed < 0)
        {
            _mommentHorizontalSpeed = _mommentHorizontalSpeed > -0.3f ? -0.3f: _mommentHorizontalSpeed;
            MoveHorizontal(0.3f);
        }

        return _mommentHorizontalSpeed;
    }

    public float MoveVertical(float speed)
    {
        if (speed > 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed < _verticalMaxSpeed
                ? _mommentVerticalSpeed + _incrementalVerticalAccelerate
                : _verticalMaxSpeed;
        }

        if (speed < 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed > -_verticalMaxSpeed
                ? _mommentVerticalSpeed - _incrementalVerticalAccelerate
                : -_verticalMaxSpeed;
        }

        if (speed == 0 && _mommentVerticalSpeed > 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed < 0.3f ? 0.3f: _mommentVerticalSpeed;
            MoveVertical(-0.3f);
        }else if (speed == 0 && _mommentVerticalSpeed < 0)
        {
            _mommentVerticalSpeed = _mommentVerticalSpeed > -0.3f ? -0.3f: _mommentVerticalSpeed;
            MoveVertical(0.3f);
        }

        return _mommentVerticalSpeed;
    }

    public float MoveRotation(float speed)
    {
        if (speed > 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed < _rotationMaxSpeed
                ? _mommentRotationSpeed + _incrementalRotationAccelerate
                : _rotationMaxSpeed;
        }

        if (speed < 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed > -_rotationMaxSpeed
                ? _mommentRotationSpeed - _incrementalRotationAccelerate
                : -_rotationMaxSpeed;
        }

        if (speed == 0 && _mommentRotationSpeed > 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed < 0.3f ? 0.3f: _mommentRotationSpeed;
            MoveRotation(-0.3f);
        }else if (speed == 0 && _mommentRotationSpeed < 0)
        {
            _mommentRotationSpeed = _mommentRotationSpeed > -0.3f ? -0.3f: _mommentRotationSpeed;
            MoveRotation(0.3f);
        }

        return _mommentRotationSpeed;
    }
}