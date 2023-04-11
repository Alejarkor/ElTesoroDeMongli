using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSwitcher : MonoBehaviour
{
    public GameObject MobileCanvasInput;


    public void Awake()
    {
        DetectMobileDevice.Instance.OnDeviceRunningDetected += OnDeviceRunningDetected;
    }

    private void OnDeviceRunningDetected(DetectMobileDevice.deviceType dtype)
    {
        switch (dtype) 
        {
            case DetectMobileDevice.deviceType.mobile:
                OnMobileDeviceRunning();
                break;
            case DetectMobileDevice.deviceType.desktop:
                OnDesktopDeviceRunning();
                break;
        }
    }

    public void Start()
    {
        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
        {
            OnMobileDeviceRunning();
        }
        else 
        {
            OnDesktopDeviceRunning();
        }
    }

    public void OnMobileDeviceRunning()
    {
        MobileCanvasInput.SetActive(true);
    }
    public void OnDesktopDeviceRunning()
    {
        MobileCanvasInput.SetActive(false);
    }
}
