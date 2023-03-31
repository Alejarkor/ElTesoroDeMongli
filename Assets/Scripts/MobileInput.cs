using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInput : InputConnector
{
    public VirtualButton vButton;
    public VirtualJoystick vJoy;
    internal override bool GetAction()
    {
        throw new NotImplementedException();
    }   

    internal override Vector2 GetCameraMoveInput()
    {
        throw new NotImplementedException();
    }    

    internal override bool GetJump()
    {
        return vButton.value;
    }

    internal override Vector2 GetMoveInput()
    {
        return vJoy.value;
    }
}
