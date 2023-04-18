using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class MongliAPIConnector 
{
    private const string API_URL = "http://127.0.0.1/ElTesoroDeMongliAPI/";
    private const string MONGLI_KEY = "9b60821d-e697-43c0-9222-f4e78be037b8";   


    public static string MongliToJson<T>(T ob) 
    {
        string message = JsonUtility.ToJson(ob);
        message = JsonConvert.SerializeObject(message);
        return message;
    }
    public static T MongliFromJson<T>(string json) 
    {
        json = JsonConvert.DeserializeObject<string>(json);
        T data = JsonUtility.FromJson<T>(json);
        return data;
    }
    public static IEnumerator ValidateLoginCoroutine(int userId, string token, Action<LoginResponse> onComplete)
    {
        // Crear el objeto JSON para enviar al servidor
        string json = $"{{\r\n    \"user_id\": {userId},\r\n    \"token\": \"{token}\"\r\n}}";

        // Crear una solicitud UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(API_URL+ "validate_login/", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("mongli_key", MONGLI_KEY);

        // Enviar la solicitud y esperar la respuesta
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var responseContent = request.downloadHandler.text;
            var loginResponse = JsonUtility.FromJson<LoginResponse>(responseContent);

            if (loginResponse.error_code == 0)
            {
                Debug.Log("userID: " + userId + " cohincide con la base de datos: " + (userId == loginResponse.user_id ? "TRUE" : "FALSE"));
                // La validación es correcta
                onComplete?.Invoke(loginResponse);
            }
            else
            {
                Debug.Log("Error al validar el token: " + loginResponse.error_code);
                onComplete?.Invoke(loginResponse);
            }
        }
        else
        {
            var loginResponse = new LoginResponse();
            loginResponse.error_code = 15;
            Debug.Log("Error al hacer la petición HTTP: " + request.error);
            onComplete?.Invoke(loginResponse);
        }
    }
    public static async Task<List<UserFetchData>> FetchUsersAsync()
    {
        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_URL + "get_users/");
        request.Headers.Add("mongli_key", MONGLI_KEY);
        var content = new StringContent("", null, "text/plain");
        request.Content = content;
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();
        Debug.Log(jsonResponse);        
        UsersFetchData serverResponse = JsonUtility.FromJson<UsersFetchData>(jsonResponse);
        return serverResponse.content;
    }
    public static IEnumerator FetchUsersCoroutine()
    {
        using (UnityWebRequest request = new UnityWebRequest(API_URL + "get_users/", "POST"))
        {
            request.SetRequestHeader("mongli_key", MONGLI_KEY);
            request.SetRequestHeader("Content-Type", "text/plain");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching users: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                UsersFetchData serverResponse = MongliFromJson<UsersFetchData>(jsonResponse);
                List<UserFetchData> users = serverResponse.content;

                // Do something with the users array
            }
        }
    }        
    public static IEnumerator SendUpdateUserDataCoroutine(UsersUpdateData data)
    {
        string jsonContent = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "update_users/", "POST"))
        {
            request.SetRequestHeader("mongli_key", MONGLI_KEY);
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonContent);
            
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();            
            if (request.result == UnityWebRequest.Result.Success)
            {                
                var responseContent = request.downloadHandler.text;                
                var loginResponse = JsonUtility.FromJson<LoginResponse>(responseContent);
                if (loginResponse.error_code != 0)
                {
                    Debug.Log("Error");
                }                
            }           
        }
    }
    internal static UsersUpdateData UsersDataBase(uint user_id, MongliUser mUser)
    {
        UsersUpdateData usersUpdateData = new UsersUpdateData();
        UserUpdateData userUpdateData = new UserUpdateData(user_id, mUser);
        usersUpdateData.usersUpdateData.Add(userUpdateData);

        return usersUpdateData;
    }
}

#region Mensaje de respuesta del Login
public struct LoginResponse
{
    public int error_code;
    public uint user_id;
    public string nickname;
    public object transform;
}
#endregion

#region Mensaje a recibir de la DB Informacoin de los Usuarios
[Serializable]
public class UsersFetchData
{
    public uint error_code;
    public List<UserFetchData> content;
}

[System.Serializable]
public class UserFetchData
{
    public uint id;
    public string nickname;    
    public TransformData transform;
}
#endregion

#region Mensaje enviar a la DB Informacion de los Usuarios
[System.Serializable]
public class UsersUpdateData
{
    public List<UserUpdateData> usersUpdateData = new List<UserUpdateData>();
}
[System.Serializable]
public class UserUpdateData
{
    public uint id;
    public TransformData transform;

    public UserUpdateData(uint userId, MongliUser mUser) 
    {
        this.id = userId;
        this.transform = new TransformData(mUser.transform);
    }
    public UserUpdateData(){}
}
#endregion

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }
}

