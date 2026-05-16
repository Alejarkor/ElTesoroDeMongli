using System.Threading.Tasks;
using ElTesoroDeMongli.API.Models;

namespace ElTesoroDeMongli.API
{
    public interface IUserApiClient
    {
        Task<GetUsersResponse> GetUsersAsync();
        Task<UpdateUsersResponse> UpdateUsersAsync(UpdateUsersRequest request);
    }
}
