using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerNetwork : NetworkBehaviour
{    
    public Transform playerTransform; 
    public CharacterController characterController;
    public Animator anim;
    public Transform playerCamera;
    
    public ThirdPersonController1 playerMovement;
    public GameObject character;
    public GameObject playerCanvas;
    public float playerSpeed;

    private Vector2 moveInput;
    private Vector2 moveInputProcessed;
    
    private bool jumpInput;
    private bool actionInput;

    Vector3 networkPlayerPosition;    
    Quaternion networkPlayerRotation;
    
    Vector3 networkPlayerInputDirectionMovement;    
    bool networkPlayerInputJump;
    bool networkPlayerInputAction;
    public bool isGrounded;

    //public CameraInput camInput;
    private string currentControlScheme;

    

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            Destroy(playerCamera.gameObject);
            Destroy(playerCanvas.gameObject);
            Destroy(characterController.transform.GetComponent<ThirdPersonController1>());
            Destroy(characterController);
            Destroy(GetComponent<PlayerInput>());
            Destroy(GetComponent<InputSwitcher>());
            Destroy(GetComponentInChildren<CinemachineFreeLook>().gameObject);
        }
        //else 
        //{
        //    camInput.OnUpdateCameraInput += OnUpdateCamInput;
        //}
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        //if (isLocalPlayer) 
        //{
        //    camInput.OnUpdateCameraInput -= OnUpdateCamInput;
        //}
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        Destroy(playerCamera.gameObject);
        Destroy(playerCanvas.gameObject);
        Destroy(character);
        Destroy(GetComponent<PlayerInput>());        
        Destroy(GetComponent<AnimatorController>());
        Destroy(GetComponent<InputSwitcher>());
        Destroy(GetComponentInChildren<CinemachineFreeLook>().gameObject);
    }

   
    private void ProcessInput() 
    {        
        Vector3 moveInputVector3 = playerCamera.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
        moveInputProcessed = new Vector2(moveInputVector3.x, moveInputVector3.z);        
        moveInputProcessed.Normalize();
        moveInputProcessed *= moveInput.magnitude;
    }

    // Update is called once per frame
    void Update()
    {        
        if (isServer)
        {
            MoveCharacter(networkPlayerInputJump, networkPlayerInputDirectionMovement, Time.deltaTime);
            SendTransformToClient(playerTransform.position, playerTransform.rotation);
            networkPlayerInputJump = false;
            networkPlayerInputAction = false;
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                if (playerCamera == null) return;                
                else
                {                    
                    ProcessInput();
                    SendMoveInputToServer(moveInputProcessed);
                    MoveCharacter(jumpInput, moveInputProcessed, Time.deltaTime);
                    playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.1f);
                    playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.1f);
                    jumpInput = false;
                }
            }
            else 
            {
                playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.2f);
                playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.2f);
            }
        }
    }
    

    
    [Command]
    private void SendJumpInputToServer(bool jumpAction)
    {        
        networkPlayerInputJump = jumpAction;
    }

    [Command]
    private void SendMoveInputToServer(Vector3 moveDir)
    {
        networkPlayerInputDirectionMovement = moveDir;        
    }

    [Command]
    private void SendActionInputToServer(bool actionInput)
    {
        networkPlayerInputAction = actionInput;
    }



    [ClientRpc]
    private void SendTransformToClient(Vector3 position, Quaternion rotation)
    {   
        networkPlayerPosition = position;
        networkPlayerRotation = rotation;        
    }  

    void MoveCharacter(bool button1, Vector3 moveDir, float deltaTime) 
    {
        playerMovement.UpdatePlayer(moveDir, button1, deltaTime);
    }



    private void OnMovement(InputValue value) 
    {
        moveInput = value.Get<Vector2>();
        ProcessInput();
        if (isLocalPlayer) SendMoveInputToServer(moveInputProcessed);
    }
    private void OnJump(InputValue value)
    {
        jumpInput = value.Get<float>()==1?true:false;
        if (isLocalPlayer) SendJumpInputToServer(jumpInput);
    }
    private void OnAction(InputValue value)
    {
        actionInput = value.Get<float>() == 1 ? true : false;
        if (isLocalPlayer) SendActionInputToServer(actionInput);
    }
    //private void OnUpdateCamInput()
    //{
    //    ProcessInput();
    //    if (isLocalPlayer) SendMoveInputToServer(moveInputProcessed);
    //}
}
