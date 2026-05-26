using UnityEngine;

namespace ElTesoroDeMongli.Config
{
    public class EnvironmentConfigProvider : MonoBehaviour
    {
        [SerializeField] private EnvironmentConfig config;

        private static EnvironmentConfig current;

        public static EnvironmentConfig Current => current;
        public EnvironmentConfig Config => config;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogWarning($"{nameof(EnvironmentConfigProvider)} has no config assigned.", this);
                return;
            }

            current = config;
            Debug.Log($"Environment config loaded: {config.EnvironmentType} | API={config.ApiBaseUrl} | GameServer={config.GameServerUri}", this);
        }
    }
}
