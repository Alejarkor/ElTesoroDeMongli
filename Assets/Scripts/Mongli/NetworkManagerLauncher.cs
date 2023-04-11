using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerLauncher : MonoBehaviour
{
    public NetworkManager networkManager;

    public void LaunchServer() 
    {
        networkManager.StartServer();
    }
    public void LaunchClient() 
    {
        networkManager.StartClient();
    }
}
