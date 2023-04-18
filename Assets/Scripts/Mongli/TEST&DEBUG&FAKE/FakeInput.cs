using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

public class FakeInput : MonoBehaviour
{
    public MongliCharacterController characterController;
    public Transform playerCamera;
    private Vector2 moveInputPricessed;

    // Update is called once per frame
    void Update()
    {
        Vector2 input;
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        bool jump = Input.GetKey(KeyCode.Space);

        ProcessInput(input);
        characterController.SetInput(moveInputPricessed, jump, jump, jump, jump, jump, jump, jump, jump);

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

    private void ProcessInput(Vector2 moveInput)
    {
        Vector3 moveInputVector3 = playerCamera.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
        moveInputPricessed = new Vector2(moveInputVector3.x, moveInputVector3.z);
        moveInputPricessed.Normalize();
        moveInputPricessed *= moveInput.magnitude;
    }


    //public PlayerInput pInput;    
    public CinemachineFreeLookCamController camController;    
    public Vector2 swipeStartPosition;

    private Queue<Vector2> deltaPositions = new Queue<Vector2>();
    private Vector2 currentSmoothDelta;

    private int filterWindowSize = 10; // El tamaño de la ventana del filtro, ajusta según la suavidad deseada       

    
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

   
}
