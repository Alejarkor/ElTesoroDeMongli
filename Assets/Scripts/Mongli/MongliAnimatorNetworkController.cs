using Mirror;
using System;
using UnityEngine;

public class MongliAnimatorNetworkController: NetworkBehaviour
{    
    public Animator anim;

    #region Generic Methods

    internal void SetState(string paramName, bool onLocal)
    {
        if (isServer)
        {
            SetStateToClients(paramName, onLocal);
        }
        else if (isLocalPlayer)
        {
            anim.Play(paramName);
        }
    }

    [ClientRpc]
    private void SetStateToClients(string paramName, bool onLocal)
    {
        if (isLocalPlayer && !onLocal) return;
        anim.Play(paramName);
    }
    internal void SetFloat(string paramName, float value) 
    {
        if (isServer)
        {
            SetFloatToClients(paramName, value);
        }
        else
        {
            anim.SetFloat(paramName, value);
        }
    }

    [ClientRpc]
    private void SetFloatToClients(string paramName, float value)
    {
        if (isLocalPlayer) return;
        anim.SetFloat(paramName, value);
    }

    internal void SetBool(string paramName, bool value)
    {
        if (isServer)
        {
            SetBoolToClients(paramName, value);
        }
        else if (isLocalPlayer)
        {
            anim.SetBool(paramName, value);
        }
    }

    [ClientRpc]
    private void SetBoolToClients(string paramName, bool value)
    {
        if (isLocalPlayer) return;
        anim.SetBool(paramName, value);
    }

    internal void SetInteger(string paramName, int value)
    {
        if (isServer)
        {
            SetIntegerToClients(paramName, value);
        }
        else if (isLocalPlayer)
        {
            anim.SetInteger(paramName, value);
        }
    }

    [ClientRpc]
    private void SetIntegerToClients(string paramName, int value)
    {
        if (isLocalPlayer) return;
        anim.SetTrigger(paramName);
    }

    internal void SetTrigger(string paramName, bool onLocal)
    {
        if (isServer)
        {
            SetTriggerToClients(paramName, onLocal);
        }
        else if (isLocalPlayer)
        {
            anim.SetTrigger(paramName);
        }
    }

    [ClientRpc]
    private void SetTriggerToClients(string paramName, bool onLocal)
    {
        if (isLocalPlayer && !onLocal) return;
        anim.SetTrigger(paramName);
    }

    internal void WakeUpBitch(float v)
    {        
        SetState("Lay", true);
        SetTrigger("WakeUp", true);
    }

    #endregion

    #region Old Methods

    internal void SetFreeFall(bool v)
    {
        if (isServer) 
        {
            SetFreeFallToClients(v);
        }
        else if(isLocalPlayer) 
        {
            anim.SetBool("FreeFall", v);            
        }        
    }
    [ClientRpc]
    private void SetFreeFallToClients(bool v)
    {
        if (isLocalPlayer) return;
        anim.SetBool("FreeFall", v);
    }

    internal void SetGrounded(bool grounded)
    {
        if (isServer)
        {
            SetGroundedToClients(grounded);
        }
        else if (isLocalPlayer)
        {
            anim.SetBool("Grounded", grounded);            
        }
    }
    [ClientRpc]
    private void SetGroundedToClients(bool grounded)
    {
        if (isLocalPlayer) return;
        anim.SetBool("Grounded", grounded);
    }

    internal void SetJump(bool v)
    {
        if (isServer)
        {
            SetJumpToClients(v);
        }
        else if (isLocalPlayer)
        {
            anim.SetBool("Jump", v);            
        }
    }

    [ClientRpc]
    private void SetJumpToClients(bool v)
    {
        if (isLocalPlayer) return;
        anim.SetBool("Jump", v);
    }

    internal void SetMotionSpeed(float motionSpeed)
    {
        if (isServer)
        {
            SetMotionSpeedToClients(motionSpeed);
        }
        else if (isLocalPlayer)
        {
            anim.SetFloat("Speed", motionSpeed);            
        }
    }

    [ClientRpc]
    private void SetMotionSpeedToClients(float motionSpeed)
    {
        if (isLocalPlayer) return;
        anim.SetFloat("Speed", motionSpeed);
    }

    internal void SetSpeed(float animationBlend)
    {
        if (isServer)
        {
            SetSpeedToClients(animationBlend);
        }
        else if (isLocalPlayer)
        {
            anim.SetFloat("MotionSpeed", animationBlend);           
        }
    }
    [ClientRpc]
    private void SetSpeedToClients(float animationBlend)
    {
        if (isLocalPlayer) return;
        anim.SetFloat("MotionSpeed", animationBlend);
    }


    internal void SetSlide(bool v)
    {
        if (isServer)
        {
            SetSlideToClients(v);
        }
        else if (isLocalPlayer)
        {
            anim.SetBool("Slide", v);
        }
    }

    [ClientRpc]
    internal void SetSlideToClients(bool v)
    {
        if (isLocalPlayer) return;
        anim.SetBool("Slide", v);
    }


    #endregion
}
