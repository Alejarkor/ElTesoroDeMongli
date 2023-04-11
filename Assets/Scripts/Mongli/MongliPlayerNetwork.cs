using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class MongliPlayerNetwork : NetworkBehaviour
{
    #region Test&Debug

    //public bool fakeInput;

    #endregion

    #region References
    public Transform playerTransform;     
    public Animator anim;
    public Transform playerCamera;
    public PlayerInput playerInput;
    public InputSwitcher inputSwitcher;
    public MongliCharacterController playerController;
    public GameObject character;
    public GameObject playerCanvas;
    #endregion

    public float playerSpeed;

    #region Sync Variables
    private Vector2 moveInput;
    private Vector2 networkMoveInputProcessed;    
    private bool networkJumpDown;
    private bool networkJumpPressed;
    private bool networkActionDown;
    private bool networkActionPressed;
    private bool networkSlideDown;
    private bool networkSlidePressed;
    private bool networkCrouchDown;
    private bool networkCrouchPressed;
    private Vector3 networkPlayerPosition;
    private Quaternion networkPlayerRotation;
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            Debug.Log("cliente remoto");
            Destroy(playerCamera.gameObject);
            Destroy(playerCanvas.gameObject);
            Destroy(playerTransform.GetComponent<CapsuleCollider>());
            Destroy(playerTransform.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<MongliCameraInput>());
            Destroy(gameObject.GetComponent<InputSwitcher>());
            Destroy(gameObject.GetComponent<PlayerInput>());
        }
        else 
        {
            Debug.Log("cliente local");
            playerInput.enabled = true;
        }
    }
    public override void OnStopClient()
    {
        base.OnStopClient();     
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        Destroy(playerCamera.gameObject);
        Destroy(playerCanvas.gameObject);
        Destroy(character);
        Destroy(gameObject.GetComponent<MongliCameraInput>());
        Destroy(gameObject.GetComponent<InputSwitcher>());
        Destroy(gameObject.GetComponent<PlayerInput>());
    }    
    

    // Update is called once per frame
    void Update()
    {        
        if (isServer)
        {
            MoveCharacter(networkMoveInputProcessed, networkJumpDown, networkJumpDown, networkActionDown, networkActionDown, networkSlideDown, networkSlideDown, networkCrouchDown, networkCrouchDown);
            SendTransformToClient(playerTransform.position, playerTransform.rotation);

            networkJumpDown = false;
            //networkJumpPressed = false;
            networkActionDown = false;
            //networkActionPressed = false;
            networkSlideDown = false;
            //networkSlidePressed = false;
            networkCrouchDown = false;
            //networkCrouchPressed = false;
        }
        else if (isClient)
        {
            if (isLocalPlayer)
            {
                if (playerCamera == null) return;                
                else
                {                    
                    ProcessInput();
                    SendMoveInputToServer(networkMoveInputProcessed);
                    MoveCharacter(networkMoveInputProcessed, networkJumpDown, networkJumpDown, networkActionDown, networkActionDown, networkSlideDown, networkSlideDown, networkCrouchDown, networkCrouchDown);
                    playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.1f);
                    playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.1f);

                    networkJumpDown = false;
                    //networkJumpPressed = false;
                    networkActionDown = false;
                    //networkActionPressed = false;
                    networkSlideDown = false;
                    //networkSlidePressed = false;
                    networkCrouchDown = false;
                    //networkCrouchPressed = false;
                }
            }
            else 
            {
                playerTransform.position = Vector3.Lerp(playerTransform.position, networkPlayerPosition, 0.2f);
                playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, networkPlayerRotation, 0.2f);
            }
        }
    }

    #region Functions

    internal void AwakePlayer(float v)
    {
        MongliAnimatorNetworkController anim = GetComponentInChildren<MongliAnimatorNetworkController>();
        anim.WakeUpBitch(1f);
    }  

    private void ProcessInput()
    {
        Vector3 moveInputVector3 = playerCamera.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
        networkMoveInputProcessed = new Vector2(moveInputVector3.x, moveInputVector3.z);
        networkMoveInputProcessed.Normalize();
        networkMoveInputProcessed *= moveInput.magnitude;
    }
    void MoveCharacter(Vector2 moveInput, bool jumpD, bool jumpP, bool actionD, bool actionP, bool slideD, bool slideP, bool crouchD, bool crouchP)
    {
        playerController.SetInput(moveInput, jumpD, jumpP, actionD, actionP, slideD, slideP, crouchD, crouchP);
    }
    #endregion

    #region MSG TO SERVER
    [Command]
    private void SendMoveInputToServer(Vector3 moveDir)
    {
        networkMoveInputProcessed = moveDir;
    }
    [Command]
    private void SendJumpDownToServer(bool jumpD)
    {        
        networkJumpDown = jumpD;
    }
    [Command]
    private void SendJumpPressedToServer(bool jumpP)
    {
        networkJumpPressed = jumpP;
    }
    [Command]
    private void SendActionDownToServer(bool actionD)
    {
        networkActionDown = actionD;
    }
    [Command]
    private void SendActionPressedToServer(bool actionP)
    {
        networkActionPressed = actionP;
    }
    [Command]
    private void SendSlideDownToServer(bool slideD)
    {
        networkSlideDown = slideD;
    }
    [Command]
    private void SendSlidePressedToServer(bool slideP)
    {
        networkSlidePressed = slideP;
    }
    [Command]
    private void SendCrouchDownToServer(bool crouchD)
    {
        networkCrouchDown = crouchD;
    }
    [Command]
    private void SendCrouchPressedToServer(bool crouchP)
    {
        networkCrouchPressed = crouchP;
    }
    #endregion

    #region MSG TO CLIENT
    [ClientRpc]
    private void SendTransformToClient(Vector3 position, Quaternion rotation)
    {   
        networkPlayerPosition = position;
        networkPlayerRotation = rotation;        
    }
    #endregion

    #region Capture Actions
    private void OnMovement(InputValue value) 
    {
        moveInput = value.Get<Vector2>();
        ProcessInput();
        if (isLocalPlayer) SendMoveInputToServer(networkMoveInputProcessed);
    }
    private void OnJumpDown(InputValue value)
    {        
        networkJumpDown = value.Get<float>() == 1 ? true : false;
        networkJumpPressed = value.Get<float>() == 1 ? true : false;
        if (isLocalPlayer) 
        {
            SendJumpDownToServer(networkJumpDown);
            SendJumpPressedToServer(networkJumpPressed);
        }       
    }   
    private void OnActionDown(InputValue value)
    {       
        networkActionDown = value.Get<float>() == 1 ? true : false;
        networkActionPressed = value.Get<float>() == 1 ? true : false;
        if (isLocalPlayer)
        {
            SendActionDownToServer(networkActionDown);
            SendActionDownToServer(networkActionPressed);
        }       
    } 
    private void OnSlideDown(InputValue value)
    {        
        networkSlideDown = value.Get<float>() == 1 ? true : false;
        networkSlidePressed = value.Get<float>() == 1 ? true : false;
        if (isLocalPlayer)
        {
            SendSlideDownToServer(networkSlideDown);
            SendSlidePressedToServer(networkSlidePressed);
        }       
    }  
    private void OnCrouchDown(InputValue value)
    {        
        networkCrouchDown = value.Get<float>() == 1 ? true : false;
        networkCrouchPressed = value.Get<float>() == 1 ? true : false;
        if (isLocalPlayer)
        {
            SendCrouchDownToServer(networkCrouchDown);
            SendCrouchPressedToServer(networkCrouchPressed);
        }
       
    }  
    internal void SetNickName(string nickname)
    {
        Debug.Log("NICKNAME: " + nickname + " on new player online");
    }    

    #endregion
}
