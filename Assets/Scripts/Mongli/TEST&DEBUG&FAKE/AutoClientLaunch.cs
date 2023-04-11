using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoClientLaunch : MonoBehaviour
{
    public NetworkManager networkManager;
    public LoginGrabber loginGrabber;

    // Start is called before the first frame update
    void Awake()
    {
        loginGrabber.OnPlayerAuthenticated += OnTokenCaptured;        
    }

    void OnTokenCaptured(MongliNetworkAuthenticator.AuthRequestMessage auth) 
    {
        networkManager.StartClient();
    }
}
