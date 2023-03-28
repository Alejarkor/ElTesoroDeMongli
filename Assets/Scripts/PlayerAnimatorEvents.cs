using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorEvents : MonoBehaviour
{
    public Animator anim;
    public Action OnEndJump;


    public void EndJump() 
    {
        anim.SetBool("jump", false);
        OnEndJump?.Invoke();
    }
}
