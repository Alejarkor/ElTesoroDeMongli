using Mirror;
using UnityEngine;


public class MongliEntitySpawner : Singleton<MongliEntitySpawner>
{
    internal MongliUser? SpawnDummy(UserFetchData userFetchData)
    {
        if (userFetchData.transform.position == new Vector3(-1, -1, -1)) return null;

        GameObject prefab = EntitiesGlobalData.Instance.GetDummyNetIdByUserId(userFetchData.id);
        NetworkIdentity netId = MongliSpawn(prefab, userFetchData.transform.position, userFetchData.transform.rotation);

        netId.gameObject.name = userFetchData.nickname + "_Dummy";

        MongliUser newMongliUser = new MongliUser();
        newMongliUser.net_id = netId;
        newMongliUser.transform = netId.GetComponent<MongliUserEntity>().CharacterTransform;
        newMongliUser.nickName = userFetchData.nickname;
        newMongliUser.conn = null;
        newMongliUser.isConnected = false;

        netId.GetComponent<MongliUserEntity>().nickname = newMongliUser.nickName;

        return newMongliUser;                
    }
    internal MongliUser SpawnDummy(uint userId, MongliUser mUser, Vector3 pos, Quaternion rot)
    {
        GameObject prefab = EntitiesGlobalData.Instance.GetDummyNetIdByUserId(userId);
        NetworkIdentity netId = MongliSpawn(prefab, pos, rot);

        netId.gameObject.name = mUser.nickName + "_Dummy";        

        mUser.net_id = netId;
        mUser.transform = netId.GetComponent<MongliUserEntity>().CharacterTransform;
        mUser.isConnected = false;
        mUser.conn = null;

        netId.GetComponent<MongliUserEntity>().nickname = mUser.nickName;

        return mUser;
    }
    internal MongliUser SpawnClient(uint user_id, MongliUser mUser, Vector3 pos, Quaternion rot, NetworkConnectionToClient conn)
    {
        GameObject prefab = EntitiesGlobalData.Instance.GetClientPrefab(user_id);
        NetworkIdentity netId = MongliSpawn(prefab, pos, rot,conn);        
        
        netId.gameObject.name = mUser.nickName + "_Client";

        mUser.net_id = netId;
        mUser.transform = netId.GetComponent<MongliUserEntity>().CharacterTransform;        
        mUser.isConnected = true;
        mUser.conn = conn;

        netId.GetComponent<MongliUserEntity>().nickname = mUser.nickName;

        return mUser;
    }
    internal MongliUser SpawnClient(UserData usrData, Vector3 pos, Quaternion rot, NetworkConnectionToClient conn)
    {
        GameObject prefab = EntitiesGlobalData.Instance.GetClientPrefab(usrData.user_id);
        NetworkIdentity netId = MongliSpawn(prefab, pos, rot, conn);
        netId.gameObject.name = usrData.nickname + "_Client";

        MongliUser newMongliUser = new MongliUser();

        newMongliUser.net_id = netId;
        newMongliUser.transform = netId.GetComponent<MongliUserEntity>().CharacterTransform;
        newMongliUser.nickName = usrData.nickname;
        newMongliUser.conn = conn;
        newMongliUser.isConnected = true;

        netId.GetComponent<MongliUserEntity>().nickname = newMongliUser.nickName;

        return newMongliUser;
    }
    private NetworkIdentity MongliSpawn(GameObject go, Vector3 position, Quaternion rotation)
    {
        GameObject networkInstance = Instantiate(go, position, rotation);
        networkInstance.GetComponent<MongliUserEntity>().CharacterTransform.rotation = rotation;
        NetworkServer.Spawn(networkInstance);
        return networkInstance.GetComponent<NetworkIdentity>();
    }
    private NetworkIdentity MongliSpawn(GameObject go, Vector3 position, Quaternion rotation, NetworkConnectionToClient conn)
    {
        GameObject networkInstance = Instantiate(go, position, Quaternion.identity);
        networkInstance.GetComponent<MongliUserEntity>().CharacterTransform.rotation = rotation;
        NetworkServer.AddPlayerForConnection(conn, networkInstance);
        return networkInstance.GetComponent<NetworkIdentity>();
    }    
}
