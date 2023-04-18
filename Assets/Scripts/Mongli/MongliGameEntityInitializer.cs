using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MongliGameEntityInitializer : Singleton<MongliGameEntityInitializer>
{   
    // Start is called before the first frame update
    public async Task Init()
    {        
        SpawnEntities(await GetEntityData());
    }

    private void SpawnEntities(List<UserFetchData> usersFetchData)
    {
        for (int i = 0; i < usersFetchData.Count; ++i) 
        {
            MongliUser? mongliUser = MongliEntitySpawner.Instance.SpawnDummy(usersFetchData[i]);
            if (mongliUser != null) 
            {
                MongliEntitiesGlobalData.Instance.AddUser((MongliUser)mongliUser, usersFetchData[i].id);
            }
        }
    }

    private async Task<List<UserFetchData>>  GetEntityData()
    {
        List<UserFetchData>  usersFetchData = await MongliAPIConnector.FetchUsersAsync();
        return usersFetchData;
    }

    
}
