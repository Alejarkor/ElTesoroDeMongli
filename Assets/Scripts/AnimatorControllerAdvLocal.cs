using Mirror;
using UnityEngine;

public class AnimatorControllerAdvLocal : MonoBehaviour
{
    public Animator anim;

    internal void SetFreeFall(bool v)
    {        
        anim.SetBool("FreeFall", v);        
    }
   

    internal void SetGrounded(bool grounded)
    {
        anim.SetBool("Grounded", grounded);
    }
  

    internal void SetJump(bool v)
    {
        anim.SetBool("Jump", v);
    }

   

    internal void SetMotionSpeed(float motionSpeed)
    {
        anim.SetFloat("Speed", motionSpeed);
    }

    

    internal void SetSpeed(float animationBlend)
    {
        anim.SetFloat("MotionSpeed", animationBlend);
    }
   
}
