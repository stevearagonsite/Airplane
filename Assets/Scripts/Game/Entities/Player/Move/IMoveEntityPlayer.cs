using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveEntityPlayer
{
    float MoveForward(float speed);
    float MoveHorizontal(float speed);
    float MoveVertical(float speed);
    float MoveRotation(float speed);
}
