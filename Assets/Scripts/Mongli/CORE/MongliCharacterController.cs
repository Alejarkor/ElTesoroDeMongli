using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MongliCharacterController : MonoBehaviour
{
    public virtual void SetInput(Vector2 moveInput, bool jumpDown, bool jumpPressed, bool actionDown,
        bool actionPressed, bool slideDown, bool slidePressed, bool crouchDown, bool crouchPressed){}
}
