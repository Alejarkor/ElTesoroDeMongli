using System.IO;
using Mirror;
using UnityEngine;

public class MirrorConfigLoader : MonoBehaviour
{
    [System.Serializable]
    public class NetworkConfig
    {
        public bool isServer;
        public string ipAddress;
    }
    public string fileName = "config.json";
    public NetworkManager networkManager;
    private NetworkConfig config;

    void Start()
    {
        string configFilePath = Path.Combine(Application.dataPath, fileName);
        config = LoadNetworkConfig(configFilePath);

        if (config.isServer)
        {
            networkManager.networkAddress = config.ipAddress;
            networkManager.StartServer();
        }
        else
        {
            networkManager.networkAddress = config.ipAddress;
            networkManager.StartClient();
        }
    }

    private NetworkConfig LoadNetworkConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<NetworkConfig>(json);
        }
        else
        {
            Debug.LogError("Archivo de configuración no encontrado: " + filePath);
            return null;
        }
    }
}
