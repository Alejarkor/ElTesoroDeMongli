using System.Text;
using System.Threading.Tasks;
using ElTesoroDeMongli.API.Models;
using ElTesoroDeMongli.Config;
using UnityEngine;
using UnityEngine.Networking;

namespace ElTesoroDeMongli.API
{
    public class PhpApiClient : IAuthApiClient, IUserApiClient
    {
        private readonly EnvironmentConfig config;

        public PhpApiClient(EnvironmentConfig config)
        {
            this.config = config;
        }

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return PostAsync<LoginRequest, LoginResponse>("login/", request);
        }

        public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return PostAsync<RegisterRequest, RegisterResponse>("register/", request);
        }

        public Task<GetUsersResponse> GetUsersAsync()
        {
            return PostAsync<object, GetUsersResponse>("get_users/", new EmptyRequest());
        }

        public Task<UpdateUsersResponse> UpdateUsersAsync(UpdateUsersRequest request)
        {
            return PostAsync<UpdateUsersRequest, UpdateUsersResponse>("update_users/", request);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
            where TResponse : ApiResponse
        {
            if (config == null)
                throw new ApiException("API config is null.");

            string url = config.ApiBaseUrl + endpoint;
            string json = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");

                await webRequest.SendWebRequest();

                string responseBody = webRequest.downloadHandler?.text;

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new ApiException(
                        $"API request failed: {webRequest.error}",
                        webRequest.responseCode,
                        responseBody: responseBody);
                }

                TResponse response = JsonUtility.FromJson<TResponse>(responseBody);

                if (response == null)
                {
                    throw new ApiException(
                        "API returned an empty or invalid JSON response.",
                        webRequest.responseCode,
                        responseBody: responseBody);
                }

                return response;
            }
        }

        [System.Serializable]
        private class EmptyRequest
        {
        }
    }
}
