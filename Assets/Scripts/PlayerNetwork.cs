using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerNetwork : NetworkBehaviour
{    
    public Transform playerTransform; 
    public CharacterController characterController;
    public Animator anim;
    public Transform playerCamera;
    public PlayerInput playerInput;
    public ThirdPersonController1 playerMovement;
    public GameObject character;
    public GameObject playerCanvas;
    public float playerSpeed;

    private Vector3 moveInput = Vector3.zero;      
    

    Vector3 networkPlayerPosition;    
    Quaternion networkPlayerRotation;
    
    Vector3 networkPlayerInputDirectionMovement;    
    bool networkPlayerInputJump;
    public bool isGrounded;   

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            Destroy(playerCamera.gameObject);
            Destroy(characterController);
            Destroy(playerCamera.gameObject);
            Destroy(playerCanvas.gameObject);            
        }
    }
    

    public override void OnStartServer()
    {
        base.OnStartServer();
        Destroy(playerCamera.gameObject);
        Destroy(character);
        Destroy(playerCamera.GetComponent<Camera>());
        Destroy(playerCamera.GetComponent<AudioListener>());
        Destroy(playerCamera.GetComponent<TactilCamera>());
        Destroy(playerCanvas.gameObject);       
    }

   
    private void ProcessInput() 
    {
        //Guardamos el valor de entrada horizontal y vertical para el movimiento
        moveInput = new Vector3(playerInput.GetMoveInput().x, 0, playerInput.GetMoveInput().y); //los almacenamos en un Vector3
        moveInput = playerCamera.TransformDirection(moveInput);
        moveInput.y = 0.0f; // Prevents unwanted vertical movement
        moveInput.Normalize();
        moveInput *= playerInput.GetMoveInput().magnitude;
    }

    // Update is called once per frame
    void Update()
    {        
        if (isServer)
        {
            MoveCharacter(networkPlayerInputJump, networkPlayerInputDirectionMovement, Time.deltaTime);
            SendTransformToClient(playerTransform.position, playerTransform.rotation);           
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                if (playerCamera == null) return;
                if (playerInput == null) return;
                else
                {                    
                    ProcessInput();     

                    SendInputToServer(playerInput.GetJump(), moveInput);
                    MoveCharacter(playerInput.GetJump(), moveInput, Time.deltaTime);
                    playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.1f);
                    playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.1f);
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
    private void SendInputToServer(bool jumpAction, Vector3 moveDir)
    {
        networkPlayerInputDirectionMovement = moveDir;
        networkPlayerInputJump = jumpAction;        
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
}
