using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerNetwork : NetworkBehaviour
{
    //[SerializeField] private GameObject networkCameraPrefab;
    //[SerializeField] private GameObject networkCanvasPrefab;

    //private NetworkCamera netCamera;
    //private NetworkCanvas netCanvas;
    public Transform playerTransform; 
    public CharacterController characterController;
    public Animator anim;
    public Transform playerCamera;
    public PlayerInput playerInput;
    public GameObject character;
    public GameObject playerCanvas;
    public float groundDistance = 0.5f;
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 300.0f;
    public float jumpForce = 8.0f;
    public float gravity = 30.0f;
    private Vector3 moveDirection = Vector3.zero;
    
    private Vector3 dir;
    private bool lastButton1Value;
    private bool jump;

    
    Vector3 networkPlayerPosition;    
    Quaternion networkPlayerRotation;
    
    Vector2 networkPlayerInputJoystick;    
    bool networkPlayerInputButton;
    public bool isGrounded;

    // Start is called before the first frame update

    public override void OnStartClient()
    {
        base.OnStartClient();                
        
        if (!isLocalPlayer)
        {   
            Destroy(characterController);
            Destroy(playerCamera.gameObject);
            Destroy(playerCanvas.gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Destroy(character);
        Destroy(playerCamera.GetComponent<Camera>());
        Destroy(playerCamera.GetComponent<AudioListener>());
        Destroy(playerCamera.GetComponent<TactilCamera>());
        Destroy(playerCanvas.gameObject);       
    }   

    // Update is called once per frame
    void Update()
    {
        isGrounded = IsGrounded();
        if (isServer)
        {
            MoveCharacter(networkPlayerInputButton, networkPlayerInputJoystick);
            SendTransformToClient(playerTransform.position, playerTransform.rotation);           
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                if (playerInput == null) return;
                else
                {
                    SendInputToServer(playerInput.GetButton1(), playerInput.GetJoyData());
                    SendCameraTransformToServer(playerCamera.position, playerCamera.rotation);
                    ManageInputClient();
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
    

    private void ManageInputClient()
    {
        MoveCharacter(playerInput.GetButton1(), playerInput.GetJoyData());
    }   

    [Command]
    private void SendInputToServer(bool button1, Vector2 joyData)
    {
        networkPlayerInputJoystick = joyData;
        networkPlayerInputButton = button1;        
    }

    [Command]
    private void SendCameraTransformToServer(Vector3 pos, Quaternion rot) 
    {
        playerCamera.position = Vector3.Lerp(playerCamera.position, pos,0.5f);
        playerCamera.rotation = Quaternion.Lerp(playerCamera.rotation, rot,0.5f);
    }

    [ClientRpc]
    private void SendTransformToClient(Vector3 position, Quaternion rotation)
    {   
        networkPlayerPosition = position;
        networkPlayerRotation = rotation;        
    }    

    void MoveCharacter(bool button1, Vector2 joy )
    {        
        if (button1) 
        {
            jump = button1 != lastButton1Value;
        }
        lastButton1Value = button1;       

        if (characterController.isGrounded)
        {
            if (playerCamera == null) return;           
            else 
            {
                moveDirection = new Vector3( joy.x, 0f, joy.y);
                moveDirection = playerCamera.TransformDirection(moveDirection);
                moveDirection.y = 0.0f; // Prevents unwanted vertical movement
                moveDirection.Normalize();
                dir = moveDirection;
                FaceCharacter(moveDirection);
                moveDirection *= moveSpeed;
            }
           
        }

        if (isGrounded)
        {
            if (jump)
            {
                moveDirection.y = jumpForce;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);        
    }

    void FaceCharacter(Vector3 dir)
    {
        transform.forward = Vector3.Lerp(transform.forward, dir, 0.5f);
    }

    private bool IsGrounded()
    {
        if (Physics.Raycast(transform.position - Vector3.down * groundDistance, Vector3.down, groundDistance * 2f))
        {
            return true;
        }
        return false;
    }

}
