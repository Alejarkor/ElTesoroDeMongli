using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MongliAnimatorLocalController : MonoBehaviour
{
    public Animator anim;

    #region Generic Methods

    internal void SetState(string paramName, bool onLocal)
    {
            anim.Play(paramName);
        
    }

    
    internal void SetFloat(string paramName, float value)
    {
       
        {
            anim.SetFloat(paramName, value);
        }
    }


    internal void SetBool(string paramName, bool value)
    {
        {
            anim.SetBool(paramName, value);
        }
    }

  

    internal void SetInteger(string paramName, int value)
    {
        {
            anim.SetInteger(paramName, value);
        }
    }

   

    internal void SetTrigger(string paramName, bool onLocal)
    {
        {
            anim.SetTrigger(paramName);
        }
    }

   
    internal void WakeUpBitch(float v)
    {
        SetState("Lay", true);
        SetTrigger("WakeUp", true);
    }

    #endregion

   
}
