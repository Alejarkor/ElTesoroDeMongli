using UnityEngine;
using System;


public class LoginGrabber : Singleton<LoginGrabber>
{
    string[] queryParams;
    public bool fake;
    public int user;
    public string token;
    
    public  Action<MongliNetworkAuthenticator.AuthRequestMessage> OnPlayerAuthenticated;
    MongliNetworkAuthenticator.AuthRequestMessage authData = new MongliNetworkAuthenticator.AuthRequestMessage();
    public MongliNetworkAuthenticator.AuthRequestMessage AuthData => authData;

    private void Start()
    {    
        authData = new MongliNetworkAuthenticator.AuthRequestMessage();
        if (!fake)
        {            
            queryParams = Application.absoluteURL.Split('?')[1].Split('&');

            foreach (string queryParam in queryParams)
            {
                string[] keyValue = queryParam.Split('=');
                if (keyValue[0] == "user_id")
                {
                    authData.user_id = int.Parse(keyValue[1]);
                    Debug.Log("user_id: " + authData.user_id);
                }
                else if (keyValue[0] == "token")
                {
                    authData.access_token = keyValue[1];
                    Debug.Log("acces_token: " + authData.access_token);
                }
            }
        }
        else 
        {
            authData.user_id = user;
            authData.access_token = token;
        }


        if (authData.user_id != 0 && !string.IsNullOrEmpty(authData.access_token))
        {
            OnPlayerAuthenticated?.Invoke(authData);

        }
        else 
        {
            //TODO: REDIRECCIONAR O MOSTRAR UN MENSAJE DE ERROR
        }
    }

}


