using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;


public class UserSynchronizer : Singleton<UserSynchronizer>
{
    public GameObject prefabDummy;

    //public MongliNetworkManager networkManager;
    public Dictionary<uint, MongliUser> mongliUsersDictionary = new Dictionary<uint, MongliUser>();
    //private Dictionary<uint, UserData> dummiesData = new Dictionary<uint, UserData>();

    
    public GameObject GetDummyNetIdByUserId(uint userId) 
    {
        return prefabDummy;
        //foreach (KeyValuePair<uint, UserData> entry in dummiesData)
        //{
        //    if (entry.Value.user_id == userId)
        //    {
        //        Debug.Log("Dummy con user_id= " + userId + " encontraod!");
        //        return entry.Key;
        //    }
        //}
        //Debug.Log("Dummy con user_id= " + userId + " NO encontraod!");
        //return null; // Retorna null si no se encuentra una entrada con el user_id especificado        
    }
      
    public GameObject GetDummyPrefab(uint user_id) 
    {
        return prefabDummy;
    }

    //internal uint? GetUserId(NetworkConnectionToClient conn)
    //{
    //    return mongliUsersDictionary[conn].user_id;
    //}

    internal void UpdateUser(uint user_id, NetworkIdentity netId, NetworkConnectionToClient conn, Transform transform, bool v)
    {
        MongliUser mongliUser = mongliUsersDictionary[user_id];

        mongliUser.conn = conn;
        mongliUser.net_id = netId;
        mongliUser.isConnected = v;
        mongliUser.transform = transform;

        mongliUsersDictionary[user_id]= mongliUser;
    }

    internal uint GetMongliUserByConn(NetworkConnectionToClient conn)
    {
        if (conn == null)
        {
            throw new ArgumentNullException(nameof(conn), "Connection parameter cannot be null.");
        }

        foreach (KeyValuePair<uint, MongliUser> entry in mongliUsersDictionary)
        {
            if (entry.Value.conn == conn)
            {
                return entry.Key;
            }
        }

        throw new KeyNotFoundException("No MongliUser found with the provided connection.");
    }
}
public struct UserData 
{
    public uint user_id;
    public string nickname;    

    public UserData(uint user_id, string nickname)
    {
        this.user_id = user_id;
        this.nickname = nickname;        
    }
}

public struct MongliUser 
{    
    public NetworkIdentity net_id;
    public NetworkConnectionToClient conn;
    public string nickName;
    public Transform transform;
    public bool isConnected;
}