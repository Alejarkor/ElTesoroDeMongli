using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class DetectMobileDevice : Singleton<DetectMobileDevice>
{
    [DllImport("__Internal")]
    private static extern int IsMobileDevice();
    public enum deviceType 
    {
        None = 0,
        mobile = 1,
        desktop = 2
    }
    private deviceType m_deviceType;
    
    public bool IsOnMobileDeviceRunning 
    {
        get { return m_deviceType==deviceType.mobile?true:false; }
    }
    public Action<deviceType> OnDeviceRunningDetected;   


    void Start()
    {
        if (IsOnMobileDevice())
        {
            Debuger.Instance.DebugMsg("Running on a mobile device", Debuger.debugType.normal);
            Debug.Log("Running on a mobile device");
            OnDeviceRunningDetected?.Invoke(deviceType.mobile);
            m_deviceType = deviceType.mobile;
        }
        else
        {
            Debuger.Instance.DebugMsg("Not running on a mobile device", Debuger.debugType.normal);
            Debug.Log("Not running on a mobile device");
            OnDeviceRunningDetected?.Invoke(deviceType.desktop);
            m_deviceType = deviceType.desktop;
        }
    }

    private bool IsOnMobileDevice()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobileDevice() == 1;
#else
        // Use Unity's built-in platform detection for non-WebGL platforms
        return Application.isMobilePlatform;
#endif
    }
}
