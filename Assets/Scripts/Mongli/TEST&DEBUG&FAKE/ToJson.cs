using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;


public class ToJson : MonoBehaviour
{
    public string message;
    public uint id;
    public string nickname = "Alejarkor";
    public List<UserFetchData> usersData = new List<UserFetchData>();
    public UsersUpdateData dataToSend; 

    // Start is called before the first frame update
    void Start()
    {
        //SendData();
        FetchUsers();
    }

    private void SendData() 
    {        
        List<UserUpdateData> updateDATA = new List<UserUpdateData>();
        UserUpdateData upd = new UserUpdateData();
        upd.id = id;
        upd.transform = new TransformData(transform);
        updateDATA.Add(upd); 
       
        dataToSend.usersUpdateData = updateDATA;
        message = JsonUtility.ToJson(dataToSend); 
        StartCoroutine(MongliAPIConnector.SendUpdateUserDataCoroutine(dataToSend));
    }

    private async void FetchUsers()
    {
        usersData = await MongliAPIConnector.FetchUsersAsync();        
    }
}
