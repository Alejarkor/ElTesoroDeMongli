using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ApiCaller : MonoBehaviour
{



    public static ApiCaller Instance;
    public GlobalConf globalConf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfiguracion();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LoadConfiguracion()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            globalConf = JsonUtility.FromJson<GlobalConf>(jsonData);
            Debug.Log("Configuración cargada correctamente. IP: " + globalConf.ip);
        }
        else
        {

            Debug.LogError("No se pudo cargar el archivo de configuración.");
        }
    }





    public IEnumerator RequestAPI(string url, string jsonBody, System.Action<string> callback)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(null);
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }
    }

    public void LoginRequest(string mail, string password)
    {
        string apiUrl = "/login/";
        apiUrl = "http:/" + globalConf.ip + apiUrl;
        string jsonBody = "{'mail':'"+ mail+ "','password':'" + password + "'}";
        Debug.Log("Colling to " + apiUrl + ": " + jsonBody);
        StartCoroutine(RequestAPI(apiUrl, jsonBody, (response) =>
        {
            if (response != null)
            {
                Debug.Log("Respuesta API: " + response);
                // Procesar la respuesta aquí
            }
            else
            {
                Debug.LogError("Error al obtener respuesta de la API.");
            }
        }));
    }

 
}
