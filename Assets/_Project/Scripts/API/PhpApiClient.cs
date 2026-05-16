using System.Text;
using System.Threading.Tasks;
using ElTesoroDeMongli.API.Models;
using ElTesoroDeMongli.Config;
using UnityEngine;
using UnityEngine.Networking;
using LoginApiResponse = ElTesoroDeMongli.API.Models.LoginResponse;
using RegisterApiResponse = ElTesoroDeMongli.API.Models.RegisterResponse;
using GetUsersApiResponse = ElTesoroDeMongli.API.Models.GetUsersResponse;
using UpdateUsersApiResponse = ElTesoroDeMongli.API.Models.UpdateUsersResponse;

namespace ElTesoroDeMongli.API
{
    public class PhpApiClient : IAuthApiClient, IUserApiClient
    {
        private readonly EnvironmentConfig config;

        public PhpApiClient(EnvironmentConfig config)
        {
            this.config = config;
        }

        public Task<LoginApiResponse> LoginAsync(LoginRequest request)
        {
            return PostAsync<LoginRequest, LoginApiResponse>("login/", request);
        }

        public Task<RegisterApiResponse> RegisterAsync(RegisterRequest request)
        {
            return PostAsync<RegisterRequest, RegisterApiResponse>("register/", request);
        }

        public Task<GetUsersApiResponse> GetUsersAsync()
        {
            return PostAsync<object, GetUsersApiResponse>("get_users/", new EmptyRequest());
        }

        public Task<UpdateUsersApiResponse> UpdateUsersAsync(UpdateUsersRequest request)
        {
            return PostAsync<UpdateUsersRequest, UpdateUsersApiResponse>("update_users/", request);
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
