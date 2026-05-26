using System;
using UnityEngine;

namespace ElTesoroDeMongli.Config
{
    [CreateAssetMenu(
        fileName = "EnvironmentConfig",
        menuName = "El Tesoro de Mongli/Config/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [Header("Environment")]
        [SerializeField] private EnvironmentType environmentType = EnvironmentType.Localhost;

        [Header("API")]
        [SerializeField] private string apiBaseUrl = "http://localhost/ElTesoroDeMongliAPI/";

        [Header("Game Server")]
        [SerializeField] private string gameServerHost = "localhost";
        [SerializeField] private ushort gameServerPort = 7778;
        [SerializeField] private bool useSecureWebSocket;

        public EnvironmentType EnvironmentType => environmentType;
        public string ApiBaseUrl => NormalizeBaseUrl(apiBaseUrl);
        public string GameServerHost => gameServerHost;
        public ushort GameServerPort => gameServerPort;
        public bool UseSecureWebSocket => useSecureWebSocket;
        public string WebSocketScheme => useSecureWebSocket ? "wss" : "ws";

        public string GameServerUri
        {
            get
            {
                if (string.IsNullOrWhiteSpace(gameServerHost))
                    return string.Empty;

                return $"{WebSocketScheme}://{gameServerHost}:{gameServerPort}";
            }
        }

        private static string NormalizeBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            return url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";
        }
    }
}
