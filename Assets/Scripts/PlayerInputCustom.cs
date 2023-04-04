//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerInputCustom : MonoBehaviour
//{    
    
//    public InputConnector mobileInput;
//    public InputConnector desktopInput;

//    public Vector2 GetMoveInput() 
//    {
//        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
//        {
//            return mobileInput.GetMoveInput();
//        }
//        else 
//        {
//            return desktopInput.GetMoveInput();
//        }
//    }

//    public Vector2 GetCameraMoveInput() 
//    {
//        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
//        {
//            return mobileInput.GetCameraMoveInput();
//        }
//        else
//        {
//            return desktopInput.GetCameraMoveInput();
//        }
//    }

//    public bool GetAction() 
//    {
//        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
//        {
//            return mobileInput.GetAction();
//        }
//        else
//        {
//            return desktopInput.GetAction();
//        }
//    }

//    public bool GetJump() 
//    {
//        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
//        {
//            return mobileInput.GetJump();
//        }
//        else
//        {
//            return desktopInput.GetJump();
//        }
//    }   
//}
