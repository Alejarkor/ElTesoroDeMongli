using Mirror;
using Mirror.Discovery;
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
   
    //public static async Task<LoginResponse> ValidateLoginAsync(int userId, string token)
    //{
    //    // Crear el objeto JSON para enviar al servidor
    //    string json = $"{{\r\n    \"user_id\": {userId},\r\n    \"token\": \"{token}\"\r\n}}";

    //    using (var client = new HttpClient())
    //    {
    //        // Configurar la solicitud
    //        var request = new HttpRequestMessage(HttpMethod.Post, API_URL+ "validate_login/");
    //        request.Headers.Add("mongli_key", MONGLI_KEY);
    //        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    //        // Enviar la solicitud y obtener la respuesta
    //        HttpResponseMessage response = await client.SendAsync(request);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var responseContent = await response.Content.ReadAsStringAsync();
    //            var loginResponse = JsonUtility.FromJson<LoginResponse>(responseContent);

    //            if (loginResponse.error_code == 0)
    //            {
    //                Debug.Log("userID: " + userId + " cohincide con la base de datos: " + (userId == loginResponse.user_id ? "TRUE" : "FALSE"));
    //                // La validación es correcta
    //                //clientPlayerIds[NetworkServer.connections[NetworkServer.connections.Count - 1]] = jsonObject.user_id;
    //                return loginResponse;
    //            }
    //            else
    //            {
    //                Debug.Log("Error al validar el token: " + loginResponse.error_code);
    //                return loginResponse;
    //            }
    //        }
    //        else
    //        {
    //            var loginResponse = new LoginResponse();
    //            loginResponse.error_code = 15;
    //            Debug.Log("Error al hacer la petición HTTP: " + response.StatusCode);
    //            return loginResponse;
    //        }
    //    }
    //}

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

    public static async Task<List<GetUserData>> FetchUsersAsync()
    {
        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_URL + "get_users/");
        request.Headers.Add("mongli_key", MONGLI_KEY);
        var content = new StringContent("", null, "text/plain");
        request.Content = content;
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();

        UsersAPIResponse serverResponse = JsonUtility.FromJson<UsersAPIResponse>(jsonResponse);
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
                UsersAPIResponse serverResponse = JsonUtility.FromJson<UsersAPIResponse>(jsonResponse);
                List<GetUserData> users = serverResponse.content;

                // Do something with the users array
            }
        }
    }
        //public static async Task SendUpdateUserDataAsync(UsersUpdateData usersUpdateData)
        //{
        //    var client = new HttpClient();
        //    var request = new HttpRequestMessage(HttpMethod.Post, API_URL + "update_users/");
        //    request.Headers.Add("mongli_key", MONGLI_KEY);

        //    // Convierte la lista de objetos UpdateData en una cadena JSON
        //    var json = JsonUtility.ToJson(usersUpdateData);

        //    var content = new StringContent(json, Encoding.UTF8, "application/json");
        //    request.Content = content;

        //    var response = await client.SendAsync(request);
        //    response.EnsureSuccessStatusCode();

        //    Console.WriteLine(await response.Content.ReadAsStringAsync());
        //}

    //public static IEnumerator SendUpdateUserDataCoroutine(UsersUpdateData usersUpdateData)
    //{
    //    var json = JsonUtility.ToJson(usersUpdateData);

    //    string url = API_URL + "update_users/";       

    //    var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
    //    request.SetRequestHeader("mongli_key", MONGLI_KEY);
    //    request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    request.SetRequestHeader("Content-Type", "application/json");

    //    Debug.Log(json);

    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        Debug.Log("Response: " + request.downloadHandler.text);
    //    }
    //    else
    //    {
    //        Debug.LogError("Request failed: " + request.error);
    //    }
    //}
    public static IEnumerator SendUpdateUserDataCoroutine(UsersUpdateData data)
    {       
        string jsonContent = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "update_users/", "POST"))
        {
            request.SetRequestHeader("mongli_key", MONGLI_KEY);
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonContent);
            string base64 = Convert.ToBase64String(bodyRaw);
            Debug.Log(base64);            
            byte[] otravezBytes = Encoding.UTF8.GetBytes(base64);
            Debug.Log(otravezBytes);
            request.uploadHandler = new UploadHandlerRaw(otravezBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
            Debug.Log(request.result);
            if (request.result == UnityWebRequest.Result.Success)
            {
                
                var responseContent = request.downloadHandler.text;
                Debug.Log(responseContent);
                var loginResponse = JsonUtility.FromJson<LoginResponse>(responseContent);

                if (loginResponse.error_code == 0)
                {
                    Debug.Log("OK");
                    
                }
                else
                {
                    Debug.Log("Error");
                    
                }
            }           
        }
    }

}
public struct LoginResponse
{
    public int error_code;
    public uint user_id;
    public string nickname;
    public object transform;
}

[System.Serializable]
public class GetUserData
{
    public uint id;
    public string nickname;
    public string last_login;
    public TransformData transform;
}

[Serializable]
public class UsersAPIResponse
{
    public int error_code;
    public List<GetUserData> content;
}

[System.Serializable]
public class UpdateData
{
    public uint id;
    public TransformData transform;
}

[System.Serializable]
public class UsersUpdateData 
{
    public List<UpdateData> usersUpdateData;
}

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

