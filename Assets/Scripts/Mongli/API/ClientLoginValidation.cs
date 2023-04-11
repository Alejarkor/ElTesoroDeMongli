using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ClientLoginValidation : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(SendToken("MiTokenDeValidacion"));
    }

    IEnumerator SendToken(string token)
    {
        string serverUrl = "http://localhost:3000";
        UnityWebRequest request = UnityWebRequest.Post(serverUrl, token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string response = request.downloadHandler.text;
            if (response == "OK")
            {
                Debug.Log("Conexión aceptada");
                // Procesar la conexión exitosa aquí
            }
            else
            {
                Debug.Log("Conexión rechazada");
                // Redirigir a otra página o mostrar un mensaje de error
            }
        }
    }
}
