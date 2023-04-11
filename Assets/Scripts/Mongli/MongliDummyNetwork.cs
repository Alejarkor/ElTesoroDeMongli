using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MongliDummyNetwork : NetworkBehaviour
{
    public Animator anim;
    public string wakeUpTrigger;
    public string sleepTrigger;
    public float transitionTime = 1f;
    public void WakeUpBitch() 
    {
        anim.SetTrigger(wakeUpTrigger);
    }

    public void GoToSleepBitch() 
    {
        anim.SetTrigger(sleepTrigger);
    }

    public void SetStartState(bool isSleeping)
    {
        if (isSleeping)
        {
            anim.Play("Lay");
            Invoke("WakeUpBitch", transitionTime);
        }
        else 
        {
            anim.Play("Idle");
            Invoke("GoToSleepBitch", transitionTime);
        }
    }
}
