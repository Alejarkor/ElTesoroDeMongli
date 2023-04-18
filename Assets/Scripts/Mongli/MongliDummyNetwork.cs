using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MongliDummyNetwork : MongliUserEntity
{
    public Animator anim;       
    public string wakeUpTrigger;
    public string sleepTrigger;
    public float transitionTime = 1f;   

    public override void OnStartClient()
    {
        if (isLocalPlayer) return;
        GoToSleepBitch();        
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetupServer();
        SetUserInfo(nickname);
    }

    private void SetupServer()
    {
        SkinnedMeshRenderer[] skns = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer sk in skns)
        {
            Destroy(sk);
        }
    }   

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

    [ClientRpc]
    public override void SetUserInfo(string nkName)
    {
        nickname = nkName;
        if (!isLocalPlayer)
        {
            boxName.SetName(nickname);
        }
    }
}
