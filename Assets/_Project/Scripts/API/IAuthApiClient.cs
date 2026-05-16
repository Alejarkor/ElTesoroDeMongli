using System.Threading.Tasks;
using ElTesoroDeMongli.API.Models;

namespace ElTesoroDeMongli.API
{
    public interface IAuthApiClient
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
