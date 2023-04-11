using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class MongliNetworkAuthenticator : NetworkAuthenticator
{
    #region Messages

    public struct AuthRequestMessage : NetworkMessage 
    {
        public int user_id;
        public string access_token;
    }

    public struct AuthResponseMessage : NetworkMessage 
    {
        public bool accepted;
    }

    #endregion

    #region Server

    public Action<NetworkConnectionToClient, UserData> OnClientAccepted;

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) 
    {
        Debug.Log("on server: cliente quiere autenticarse");
    }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {    
        //Debug.Log("on server: Mensaje recibido");
        Debug.Log("userID: " + msg.user_id + " AccesToken: " + msg.access_token);       
        StartCoroutine(ValidateMongliLoginCoroutine(conn, msg));
    }
       
    private IEnumerator ValidateMongliLoginCoroutine(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        AuthResponseMessage authResponseMessage = new AuthResponseMessage();

        // Inicia la corutina ValidateLoginCoroutine y proporciona un m�todo de devoluci�n de llamada (callback) para manejar el resultado
        yield return StartCoroutine(MongliAPIConnector.ValidateLoginCoroutine(msg.user_id, msg.access_token, (LoginResponse loginResponse) =>
        {
            if (loginResponse.error_code == 0)
            {
                Debug.Log("Env�o al cliente y trato de instanciar");
                authResponseMessage.accepted = true;
                conn.Send(authResponseMessage);
                ServerAccept(conn);
                UserData userData = new UserData(loginResponse.user_id, loginResponse.nickname);
                OnClientAccepted?.Invoke(conn, userData);
            }
            else
            {
                authResponseMessage.accepted = false;
                conn.Send(authResponseMessage);
                ServerReject(conn);
            }
        }));
    }
            

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate()
    {                
        AuthRequestMessage authRequestMessage = LoginGrabber.Instance.AuthData;
        Debug.Log("ON CLIENT AUTHENTICATE: " + authRequestMessage.user_id + " " + authRequestMessage.access_token);
        NetworkClient.Send(authRequestMessage);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        // Authentication has been accepted
        Debug.Log("OnCLient: " + msg.accepted.ToString());
        ClientAccept();
    }

    #endregion
}
