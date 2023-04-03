using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineFreeLookCamController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;    
    
    public void ApplyAxisValues(Vector2 inputCam, float deltaTime)
    {
        if (freeLookCamera != null)
        {
            freeLookCamera.m_XAxis.m_InputAxisValue = inputCam.x;
            freeLookCamera.m_YAxis.m_InputAxisValue = inputCam.y;
        }
    }
}
