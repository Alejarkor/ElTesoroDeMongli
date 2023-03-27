using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetData : NetworkBehaviour
{
    public Vector3 position;
    public Quaternion rotation;

    private void Update()
    {
        if(isServer) SetPosition(transform.position);
        if(isClient)SetRotation(transform.rotation);
    }

    [Command]
    private void SetRotation(Quaternion rot)
    {
        rotation = rot;
    }

    [ClientRpc]
    private void SetPosition(Vector3 pos)
    {
        position = pos;
    }
}
