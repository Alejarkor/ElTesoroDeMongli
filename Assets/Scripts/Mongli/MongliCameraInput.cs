using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;

public class MongliCameraInput : MonoBehaviour
{
    //public PlayerInput pInput;
    public MongliTouchManager touchManager;
    public CinemachineFreeLookCamController camController;
    public string currentControlScheme;    
    public Vector2 swipeStartPosition;

    private Queue<Vector2> deltaPositions= new Queue<Vector2>();
    private Vector2 currentSmoothDelta;  

    private int filterWindowSize = 10; // El tamaño de la ventana del filtro, ajusta según la suavidad deseada
    private bool newValue;

    //private int cameraFingerID;
    public string GetCurrentControlScheme()
    {
        return currentControlScheme;
    }

    private void Update()
    {       
        if (DetectMobileDevice.Instance.IsOnMobileDeviceRunning)
        {            
            if (newValue)
            {                
                newValue = false;
            }
            else
            {
                deltaPositions.Clear();
                camController.SetInput(Vector2.zero, Time.deltaTime);
            }
        }
        else
        {
            if (currentControlScheme == "Keyboard") 
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) 
                {
                    deltaPositions.Clear();
                    return;
                }
                if (Mouse.current.leftButton.isPressed)
                {
                    Vector2 rawDeltaPosition = Mouse.current.delta.ReadValue();
                    currentSmoothDelta = ApplyMovingAverageFilter(rawDeltaPosition);
                    camController.SetInput(currentSmoothDelta * new Vector2(0.5f, 0.5f), Time.deltaTime);
                    
                }
                else
                {
                    camController.SetInput(Vector2.zero, Time.deltaTime);
                }
            }
        }
    }

    private Vector2 ApplyMovingAverageFilter(Vector2 rawDeltaPosition)
    {
        if (deltaPositions.Count >= filterWindowSize)
        {
            deltaPositions.Dequeue();
        }

        deltaPositions.Enqueue(rawDeltaPosition);
        Vector2 sum = Vector2.zero;

        foreach (Vector2 deltaPosition in deltaPositions)
        {
            sum += deltaPosition;
        }

        return sum / deltaPositions.Count;
    }


    private void OnCamMovement(InputValue value)
    {
        camController.SetInput(value.Get<Vector2>(), Time.deltaTime);        
    }

    private void OnTouchDelta1(InputValue value) 
    {
        touchManager.UpdateValidFingerIDs(0);
        if(touchManager.IsFingerIDValid(0))ProcessInputTouchDeltaValue(value.Get<TouchState>().delta);
    }
    private void OnTouchDelta2(InputValue value)
    {
        touchManager.UpdateValidFingerIDs(1);
        if (touchManager.IsFingerIDValid(1)) ProcessInputTouchDeltaValue(value.Get<TouchState>().delta);
    }
    private void OnTouchDelta3(InputValue value)
    {
        touchManager.UpdateValidFingerIDs(2);
        if (touchManager.IsFingerIDValid(2)) ProcessInputTouchDeltaValue(value.Get<TouchState>().delta);
    }
    private void OnTouchDelta4(InputValue value)
    {
        touchManager.UpdateValidFingerIDs(3);
        if (touchManager.IsFingerIDValid(3)) ProcessInputTouchDeltaValue(value.Get<TouchState>().delta);
    }

    private void ProcessInputTouchDeltaValue(Vector2 deltaValue)
    {
        currentSmoothDelta = ApplyMovingAverageFilter(deltaValue);
        camController.SetInput(currentSmoothDelta * 0.1f, Time.deltaTime);
        newValue = true;        
    }

    

    private void OnControlsChanged(PlayerInput playerInput) 
    {
        currentControlScheme = playerInput.currentControlScheme;
        Debug.Log("Current control scheme: " + currentControlScheme);
    }
}
