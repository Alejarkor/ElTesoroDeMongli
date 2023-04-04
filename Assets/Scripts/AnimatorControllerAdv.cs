using Mirror;
using System;
using UnityEngine;

public class AnimatorControllerAdv: NetworkBehaviour
{    
    public Animator anim;       

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
}
