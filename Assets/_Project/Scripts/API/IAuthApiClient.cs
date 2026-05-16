using System.Threading.Tasks;
using ElTesoroDeMongli.API.Models;
using LoginApiResponse = ElTesoroDeMongli.API.Models.LoginResponse;
using RegisterApiResponse = ElTesoroDeMongli.API.Models.RegisterResponse;

namespace ElTesoroDeMongli.API
{
    public interface IAuthApiClient
    {
        Task<LoginApiResponse> LoginAsync(LoginRequest request);
        Task<RegisterApiResponse> RegisterAsync(RegisterRequest request);
    }
}
