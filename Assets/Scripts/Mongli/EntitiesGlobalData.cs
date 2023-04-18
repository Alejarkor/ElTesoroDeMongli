using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;


public class EntitiesGlobalData : Singleton<EntitiesGlobalData>
{
    public GameObject prefabDummy;
    public GameObject prefabClient;

    [SerializeField]
    public Dictionary<uint, MongliUser> mongliUsersDictionary = new Dictionary<uint, MongliUser>();

    public void Start()
    {        
        mongliUsersDictionary.Clear();
    }


    public GameObject GetDummyNetIdByUserId(uint userId) 
    {
        return prefabDummy;
       
    }
      
    public GameObject GetClientPrefab(uint user_id) 
    {
        return prefabClient;
    }

   

    internal void UpdateUser(uint user_id, MongliUser mUser)
    {        
        mongliUsersDictionary[user_id] = mUser;       
    }

    internal uint? GetMongliUserByConn(NetworkConnectionToClient conn)
    {
        if (conn == null)
        {
            throw new ArgumentNullException(nameof(conn), "Connection parameter cannot be null.");
            return null;
        }

        foreach (KeyValuePair<uint, MongliUser> entry in mongliUsersDictionary)
        {
            if (entry.Value.conn == conn)
            {
                return entry.Key;
            }
        }
        return null;
        Debug.Log("No MongliUser found with the provided connection.");
    }

    internal void AddUser(MongliUser mongliUser, uint id)
    {
        mongliUsersDictionary.TryAdd(id, mongliUser);
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

[Serializable]
public class MongliUser 
{  
    public NetworkIdentity net_id;
    public NetworkConnectionToClient conn;
    public string nickName;
    public Transform transform;
    public bool isConnected;
}