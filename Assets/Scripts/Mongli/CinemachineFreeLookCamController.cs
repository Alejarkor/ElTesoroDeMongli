using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineFreeLookCamController : MongliCameraController
{
    public CinemachineFreeLook freeLookCamera;

    public override void SetInput(Vector2 cameraInput, float deltaTime)
    {
        if (freeLookCamera != null)
        {
            freeLookCamera.m_XAxis.m_InputAxisValue = cameraInput.x;
            freeLookCamera.m_YAxis.m_InputAxisValue = cameraInput.y;
        }
    }
    //public void ApplyAxisValues(Vector2 inputCam, float deltaTime)
    //{
    //    if (freeLookCamera != null)
    //    {
    //        freeLookCamera.m_XAxis.m_InputAxisValue = inputCam.x;
    //        freeLookCamera.m_YAxis.m_InputAxisValue = inputCam.y;
    //    }
    //}
}
