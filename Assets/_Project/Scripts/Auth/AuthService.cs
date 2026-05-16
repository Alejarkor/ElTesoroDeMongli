using System.Threading.Tasks;
using ElTesoroDeMongli.API;
using ElTesoroDeMongli.API.Models;
using LoginApiResponse = ElTesoroDeMongli.API.Models.LoginResponse;
using RegisterApiResponse = ElTesoroDeMongli.API.Models.RegisterResponse;

namespace ElTesoroDeMongli.Auth
{
    public class AuthService
    {
        private readonly IAuthApiClient apiClient;
        private readonly SessionService sessionService;

        public AuthService(IAuthApiClient apiClient, SessionService sessionService)
        {
            this.apiClient = apiClient;
            this.sessionService = sessionService;
        }

        public async Task<LoginApiResponse> LoginAsync(string login, string password)
        {
            LoginApiResponse response = await apiClient.LoginAsync(new LoginRequest(login, password));

            if (response.IsSuccess && response.content != null)
            {
                sessionService.SetSession(new UserSession(
                    response.content.user_id,
                    response.content.token,
                    login));
            }

            return response;
        }

        public Task<RegisterApiResponse> RegisterAsync(string mail, string password, string nickname)
        {
            return apiClient.RegisterAsync(new RegisterRequest(mail, password, nickname));
        }

        public void Logout()
        {
            sessionService.ClearSession();
        }
    }
}
