using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CinemachineFreeLookCamController : MongliCameraController
{
    [SerializeField] private MonoBehaviour freeLookCamera;

    private object xAxisState;
    private object yAxisState;
    private FieldInfo xAxisInputField;
    private FieldInfo yAxisInputField;

    private void Awake()
    {
        CacheAxisBindings();
    }

    public override void SetInput(Vector2 cameraInput, float deltaTime)
    {
        if (freeLookCamera == null && !TryResolveFreeLookCamera())
            return;

        if (xAxisState == null || yAxisState == null)
            CacheAxisBindings();

        xAxisInputField?.SetValue(xAxisState, cameraInput.x);
        yAxisInputField?.SetValue(yAxisState, cameraInput.y);
    }

    private bool TryResolveFreeLookCamera()
    {
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != null && behaviour.GetType().Name == "CinemachineFreeLook")
            {
                freeLookCamera = behaviour;
                CacheAxisBindings();
                return true;
            }
        }

        return false;
    }

    private void CacheAxisBindings()
    {
        xAxisState = null;
        yAxisState = null;
        xAxisInputField = null;
        yAxisInputField = null;

        if (freeLookCamera == null)
            return;

        FieldInfo xAxisField = freeLookCamera.GetType().GetField("m_XAxis");
        FieldInfo yAxisField = freeLookCamera.GetType().GetField("m_YAxis");

        if (xAxisField == null || yAxisField == null)
            return;

        xAxisState = xAxisField.GetValue(freeLookCamera);
        yAxisState = yAxisField.GetValue(freeLookCamera);

        if (xAxisState == null || yAxisState == null)
            return;

        xAxisInputField = xAxisState.GetType().GetField("m_InputAxisValue");
        yAxisInputField = yAxisState.GetType().GetField("m_InputAxisValue");
    }
}
