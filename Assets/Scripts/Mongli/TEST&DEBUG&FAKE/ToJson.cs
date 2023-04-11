using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ToJson : MonoBehaviour
{
    public string message;
    public uint id;
    public string nickname = "Alejarkor";
    public List<GetUserData> usersData = new List<GetUserData>();


    // Start is called before the first frame update
    void Start()
    {
        //SendData();
        FetchUsers();
    }

    private void SendData() 
    {
        
        List<UpdateData> updateDATA = new List<UpdateData>();
        UpdateData upd = new UpdateData();
        upd.id = id;
        upd.transform = new TransformData(transform);
        updateDATA.Add(upd);
        message = JsonUtility.ToJson(updateDATA.ToArray());
        UsersUpdateData dataToSend = new UsersUpdateData();
        dataToSend.usersUpdateData = updateDATA;

        message = JsonUtility.ToJson(dataToSend);
        StartCoroutine(MongliAPIConnector.SendUpdateUserDataCoroutine(dataToSend));
    }

    private async void FetchUsers()
    {
        usersData = await MongliAPIConnector.FetchUsersAsync();
        // Procesa UsersData según sea necesario.
    }
}
