using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[Serializable]
public struct ObjectSpawnedMessage : NetworkMessage
{
    public uint objectId;
}