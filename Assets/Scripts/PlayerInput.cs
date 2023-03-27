using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public VirtualButton vButton;
    public VirtualJoystick vJoy;

    public Vector2 GetJoyData() 
    {
        return vJoy.value;
    }

    public bool GetButton1() 
    {
        return vButton.value;
    }   
}
