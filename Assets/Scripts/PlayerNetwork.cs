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

    //public float groundDistance = 0.5f;
    //public float moveSpeed = 3.0f;
    //public float rotationSpeed = 300.0f;
    //public float jumpForce = 8.0f;
    //public float gravity = 30.0f;

    private Vector3 moveInput = Vector3.zero;
       
    //public float stepHeight;


    Vector3 networkPlayerPosition;    
    Quaternion networkPlayerRotation;
    
    Vector3 networkPlayerInputDirectionMovement;    
    bool networkPlayerInputJump;
    public bool isGrounded;

    // Start is called before the first frame update

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

    //Funcion para determinar la direccion a la que mira la camara. 
   
    private void ProcessInput() 
    {
        //Guardamos el valor de entrada horizontal y vertical para el movimiento
        moveInput = new Vector3(playerInput.GetJoyData().x, 0, playerInput.GetJoyData().y); //los almacenamos en un Vector3
        moveInput = playerCamera.TransformDirection(moveInput);
        moveInput.y = 0.0f; // Prevents unwanted vertical movement
        moveInput.Normalize();
        moveInput *= playerInput.GetJoyData().magnitude;
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
                    //if (moveInput.magnitude > 0.01f)
                    //{
                    //    SendAnimatorStateRun(true);
                    //    anim.SetBool("run", true);

                    //    SendAnimatorStateSpeed(moveInput.magnitude * playerSpeed / 2f);
                    //    anim.SetFloat("speed", moveInput.magnitude * playerSpeed / 2f);
                    //}
                    //else
                    //{
                    //    SendAnimatorStateRun(false);
                    //    anim.SetBool("run", false);

                    //    SendAnimatorStateSpeed(1f);
                    //    anim.SetFloat("speed", 1f);
                    //}

                    

                    SendInputToServer(playerInput.GetButton1(), moveInput);
                    MoveCharacter(playerInput.GetButton1(), moveInput, Time.deltaTime);
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
    //[Command]
    //private void SendAnimatorStateRun(bool isRun) 
    //{
    //    SendAnimatorStateRunClient(isRun);
    //}
    //[ClientRpc]
    //private void SendAnimatorStateRunClient(bool isRun)
    //{
    //    if (isLocalPlayer) return;
    //    anim.SetBool("run", isRun);
    //}
    //[Command]
    //private void SendAnimatorStateJump(bool isJump) 
    //{
    //    SendAnimatorStateJumpClient(isJump);
    //}
    //[ClientRpc]
    //private void SendAnimatorStateJumpClient(bool isJump)
    //{
    //    if (isLocalPlayer) return;
    //    anim.SetBool("jump", isJump);
    //}
    //[Command]
    //private void SendAnimatorStateAir(bool isAir) 
    //{
    //    SendAnimatorStateAirClient(isAir);
    //}
    //[ClientRpc]
    //private void SendAnimatorStateAirClient(bool isAir)
    //{
    //    if (isLocalPlayer) return;
    //    anim.SetBool("air", isAir);
    //}

    //[Command]
    //private void SendAnimatorStateSpeed(float speed)
    //{
    //    SendAnimatorStateSpeedClient(speed);
    //}
    //[ClientRpc]
    //private void SendAnimatorStateSpeedClient(float speed)
    //{
    //    if (isLocalPlayer) return;
    //    anim.SetFloat("speed", speed);
    //}


    void MoveCharacter(bool button1, Vector3 moveDir, float deltaTime) 
    {
        playerMovement.UpdatePlayer(moveDir, button1, deltaTime);
    }   
}
