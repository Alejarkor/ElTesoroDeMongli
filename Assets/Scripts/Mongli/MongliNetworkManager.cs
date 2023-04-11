using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class MongliNetworkManager : NetworkManager
{
    private const string API_URL = "http://127.0.0.1/ElTesoroDeMongliAPI/validate_login/";
    private const string MONGLI_KEY = "9b60821d-e697-43c0-9222-f4e78be037b8";    

    public override void Start()
    {
        base.Start();
        (authenticator as MongliNetworkAuthenticator).OnClientAccepted += OnClientAccepted;        
    } 
    public override void OnStartServer()
    {
        base.OnStartServer();        
        Debug.Log("SERVER STARTED"); 
    }
    public override void OnStopServer()
    {
        base.OnStopServer();        
        Debug.Log("SERVER END");
    }
    private void OnClientAccepted(NetworkConnectionToClient conn, UserData usrData)
    {
        MongliUser mUser;        

        if (UserSynchronizer.Instance.mongliUsersDictionary.TryGetValue(usrData.user_id, out mUser))
        {
            MongliUser mongliUser = (MongliUser)mUser;
            if (mongliUser.isConnected)
            {
                Debug.Log(mongliUser.nickName + " is connected jet but is traying to do again");
            }
            else
            {
                Debug.Log(mongliUser.nickName + " is sleeping so wakeUp");
                //Destruir el dummy durmiente
                NetworkServer.Destroy(mongliUser.net_id.gameObject);

                //Instanciar jugador                
                Transform playerTransform = UserSynchronizer.Instance.mongliUsersDictionary[usrData.user_id].transform;   

                NetworkIdentity netId = MongliSpawn(playerPrefab, playerTransform.position, playerTransform.rotation, conn);
                UserSynchronizer.Instance.UpdateUser(usrData.user_id, netId, conn, playerTransform, true);
                netId.GetComponent<MongliPlayerNetwork>().AwakePlayer(0.5f);
            }
        }
        else
        {

            Debug.Log(usrData.nickname + " is first time connecting");
            Transform startPos = GetStartPosition();            

            NetworkIdentity netId = MongliSpawn(playerPrefab, Vector3.zero, Quaternion.identity, conn);
            MongliUser mongliUser = new MongliUser();
            mongliUser.net_id = netId;
            mongliUser.nickName = usrData.nickname;
            mongliUser.isConnected = true;
            mongliUser.conn = conn;
            mongliUser.transform = netId.GetComponent<MongliPlayerNetwork>().playerTransform;
            UserSynchronizer.Instance.mongliUsersDictionary.TryAdd(usrData.user_id, mongliUser);
            netId.GetComponent<MongliPlayerNetwork>().AwakePlayer(0.5f);
        }
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        //Instanciar al dummy
        //Actualizar la lista
        //Destruir al player
        //Enviar a la base de datos los datos de este jugador

        Debug.Log("Empieza la desconexion");
        uint? ur_id = UserSynchronizer.Instance.GetMongliUserByConn(conn);

        if (ur_id != null)
        {
            uint user_id = (uint)ur_id;
            if (UserSynchronizer.Instance.mongliUsersDictionary[user_id].isConnected)
            {
                Debug.Log("A dormir");
                //Destruir el dummy durmiente
                Vector3 pos = UserSynchronizer.Instance.mongliUsersDictionary[user_id].transform.position;
                Quaternion rot = UserSynchronizer.Instance.mongliUsersDictionary[user_id].transform.rotation;
                NetworkServer.Destroy(UserSynchronizer.Instance.mongliUsersDictionary[user_id].net_id.gameObject);
                
                NetworkIdentity netId = MongliSpawn(UserSynchronizer.Instance.GetDummyNetIdByUserId(user_id), pos, rot);
                UserSynchronizer.Instance.UpdateUser(user_id, netId, null, netId.transform, false);
                netId.GetComponent<MongliDummyNetwork>().GoToSleepBitch();
            }
            else
            {
                Debug.Log(UserSynchronizer.Instance.mongliUsersDictionary[user_id].nickName + " is not connected but is traying to disconnect again");
            }
        }
        else
        {
            Debug.Log("Algo va mal, este jugador aun no existe");          
        }
    }   


    private NetworkIdentity MongliSpawn(GameObject go, Vector3 position, Quaternion rotation) 
    {
        GameObject networkInstance = Instantiate(go, position, rotation);
        NetworkServer.Spawn(networkInstance);       
        return networkInstance.GetComponent<NetworkIdentity>();
    }

    private NetworkIdentity MongliSpawn(GameObject go, Vector3 position, Quaternion rotation, NetworkConnectionToClient conn)
    {
        GameObject networkInstance = Instantiate(go, position, rotation);
        NetworkServer.AddPlayerForConnection(conn, networkInstance);        
        return networkInstance.GetComponent<NetworkIdentity>();
    }
}
