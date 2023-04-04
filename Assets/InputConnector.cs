using SUPERCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConnector : MonoBehaviour
{
    public SUPERCharacterAIO characterController;
    public float slide;

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 camInput = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        float camZoom = 0f;
        bool jumpDown = Input.GetKeyDown(KeyCode.Space);
        bool jumpPressed = Input.GetKey(KeyCode.Space);
        bool action = Input.GetKeyDown(KeyCode.E);
        bool crouchDown = Input.GetKeyDown(KeyCode.LeftControl);
        bool crouchPressed = Input.GetKey(KeyCode.LeftControl);
        bool slideDown = Input.GetKeyDown(KeyCode.V);
        bool slidePressed = Input.GetKey(KeyCode.V);

        characterController.SetInput(moveInput* slide, camInput, camZoom, jumpDown, jumpPressed, action, crouchDown, crouchPressed, slideDown, slidePressed);
    }

    #region InputFunctions

    //public void SetInput(Vector2 moveInput, Vector2 camInput, float camZoom, bool jumpDown, bool jumpPressed, bool action, bool crouchDown, bool crouchPressed)
    //{
    //    if (!controllerPaused)
    //    {
    //        MouseXY = camInput;
    //        mouseScrollWheel = camZoom;
    //        //perspecTog = Input.GetKeyDown(perspectiveSwitchingKey_L);
    //        interactInput = action;
    //        //movement

    //        jumpInput_Momentary = jumpPressed;
    //        jumpInput_FrameOf = jumpDown;
    //        crouchInput_Momentary = crouchPressed;
    //        crouchInput_FrameOf = crouchDown;
    //        sprintInput_Momentary = Input.GetKey(sprintKey_L);
    //        sprintInput_FrameOf = Input.GetKeyDown(sprintKey_L);
    //        MovInput = moveInput;
    //    }
    //    else
    //    {
    //        jumpInput_FrameOf = false;
    //        jumpInput_Momentary = false;
    //    }
    //}


    #endregion
}
