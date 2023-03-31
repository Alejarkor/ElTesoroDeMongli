using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerLocal : MonoBehaviour
{
    public Transform playerTransform;
    public CharacterController characterController;
    public Animator anim;
    public Transform playerCamera;
    //public PlayerInput playerInput;
    public ThirdPersonController1 playerMovement;
    public GameObject character;
    public GameObject playerCanvas;
    public float playerSpeed;

    
    private Vector3 moveInput = Vector3.zero;
    private bool jumpInput = false;

    
    Vector3 networkPlayerPosition;
    Quaternion networkPlayerRotation;

    Vector3 networkPlayerInputDirectionMovement;
    bool networkPlayerInputJump;
    public bool isGrounded;

    
    private void ProcessInput()
    {       
        moveInput = playerCamera.TransformDirection(moveInput);
        float magnitude = moveInput.magnitude;
        moveInput.y = 0.0f; // Prevents unwanted vertical movement
        moveInput.Normalize();
        moveInput *= magnitude;
    }

    // Update is called once per frame
    void Update()
    {      
        if (playerCamera == null) return;
        //if (playerInput == null) return;
        else
        {
            ProcessInput();   

            //SendInputToServer(playerInput.GetButton1(), moveInput);
            MoveCharacter(jumpInput, moveInput, Time.deltaTime);
            jumpInput = false;
            //playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.1f);
            //playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.1f);
        }
    }
    void MoveCharacter(bool button1, Vector3 moveDir, float deltaTime)
    {
        playerMovement.UpdatePlayer(moveDir, button1, deltaTime);
    }



    private void OnMovement(InputValue value)
    {
        moveInput = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
    }

    private void OnJump(InputValue value) 
    {
        jumpInput = true;
    }    
}
