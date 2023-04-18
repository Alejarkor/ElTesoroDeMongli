using Mirror;
using UnityEngine;

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
        _ = MongliGameEntityInitializer.Instance.Init();

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

        if (MongliEntitiesGlobalData.Instance.mongliUsersDictionary.TryGetValue(usrData.user_id, out mUser))
        {
           
            if (mUser.isConnected)
            {
                Debug.Log(mUser.nickName + " is connected jet but is traying to do again");
            }
            else
            {                
                //Guardar valores de transform antes de destruir el objeto
                Vector3 pos = mUser.transform.position;
                Quaternion rot = mUser.transform.rotation;
                Vector3 scale = mUser.transform.localScale;

                //Destruir el dummy durmiente
                NetworkServer.Destroy(mUser.net_id.gameObject);

                //Instanciar jugador  
                MongliUser mongliUser = MongliEntitySpawner.Instance.SpawnClient(usrData.user_id, mUser,  pos, rot, conn); 
                MongliEntitiesGlobalData.Instance.UpdateUser(usrData.user_id, mongliUser);                
            }
        }
        else
        {

            Debug.Log(usrData.nickname + " is first time connecting");
                   

            mUser = MongliEntitySpawner.Instance.SpawnClient(usrData, Vector3.zero, Quaternion.identity, conn);
            Debug.Log("MongliUser antes del DummySpawn: " + usrData.user_id+", " + mUser.net_id + ", " + mUser.isConnected + ", " + mUser.nickName + ", " + mUser.transform.position);
            MongliEntitiesGlobalData.Instance.mongliUsersDictionary.TryAdd(usrData.user_id, mUser);            
        }
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {        
        Debug.Log("Empieza la desconexion");
        uint? ur_id = MongliEntitiesGlobalData.Instance.GetMongliUserByConn(conn);
        if (ur_id != null)
        {
            //Pillar posicion del client
            MongliUser mUser = MongliEntitiesGlobalData.Instance.mongliUsersDictionary[(uint)ur_id];

            Debug.Log("MongliUser antes del DummySpawn: " + mUser.net_id + ", " + mUser.isConnected + ", " + mUser.nickName + ", " + mUser.transform.position);

            Vector3 pos = mUser.transform.position;
            Quaternion rot = mUser.transform.rotation;
            Vector3 scale = mUser.transform.localScale;

            //Destruir al player
            //NetworkServer.Destroy(EntitiesGlobalData.Instance.mongliUsersDictionary[(uint)ur_id].net_id.gameObject);
            NetworkServer.DestroyPlayerForConnection(conn);
            //Instanciar al dummy
            mUser = MongliEntitySpawner.Instance.SpawnDummy((uint)ur_id,mUser, pos, rot);

            Debug.Log("MongliUser despues del DummySpawn: " + mUser.net_id + ", " + mUser.isConnected + ", " + mUser.nickName + ", " + mUser.transform.position);
            //Actualizar la lista
            MongliEntitiesGlobalData.Instance.UpdateUser((uint)ur_id, mUser);
            //Enviar a la base de datos los datos de este jugador
            UsersUpdateData updateData = MongliAPIConnector.UsersDataBase((uint)ur_id, mUser);

            StartCoroutine(MongliAPIConnector.SendUpdateUserDataCoroutine(updateData));
        }
        else
        {
            Debug.Log("Algo va mal, este jugador aun no existe");
        }       
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        foreach (MongliUser mUser in MongliEntitiesGlobalData.Instance.mongliUsersDictionary.Values) 
        {
            //mUser.net_id.GetComponent<MongliUserEntity>().InitUser(mUser);
        }
    }
}
