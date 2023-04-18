using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MongliUserEntity : NetworkBehaviour
{
    [SerializeField]
    private Transform characterTransform;
    public string nickname;    
    public MongliBoxName boxName;    
    
    public Transform CharacterTransform { get { return characterTransform; } }

    [ClientRpc]
    public virtual void SetUserInfo(string nkname) { }   
}
