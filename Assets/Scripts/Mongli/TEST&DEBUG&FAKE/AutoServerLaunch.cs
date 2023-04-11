using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoServerLaunch : MonoBehaviour
{
    public NetworkManager networkManager;
    // Start is called before the first frame update
    void Start()
    {
        networkManager.StartServer();
    }
}
