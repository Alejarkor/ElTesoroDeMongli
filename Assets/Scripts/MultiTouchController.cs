using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class MultiTouchController : MonoBehaviour
{
    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleMultiTouch();
    }

    private void HandleMultiTouch()
    {
        if (Touchscreen.current == null) return;

        IReadOnlyList<Touch> activeTouches = Touch.activeTouches;

        for (int i = 0; i < activeTouches.Count; i++)
        {
            Touch touch = activeTouches[i];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Acci�n al comenzar el toque
                    Debug.Log("Touch " + i + " began at " + touch.screenPosition);
                    break;

                case TouchPhase.Moved:
                    // Acci�n al mover el toque
                    Debug.Log("Touch " + i + " moved at " + touch.screenPosition);
                    break;

                case TouchPhase.Stationary:
                    // Acci�n cuando el toque no se mueve
                    Debug.Log("Touch " + i + " is stationary at " + touch.screenPosition);
                    break;
                
                case TouchPhase.Ended:
                    // Acci�n al finalizar el toque
                    Debug.Log("Touch " + i + " ended at " + touch.screenPosition);
                    break;

                case TouchPhase.Canceled:
                    // Acci�n cuando el toque es cancelado (por ejemplo, cuando el usuario levanta varios dedos al mismo tiempo)
                    Debug.Log("Touch " + i + " canceled at " + touch.screenPosition);
                    break;
            }
        }
    }
}
