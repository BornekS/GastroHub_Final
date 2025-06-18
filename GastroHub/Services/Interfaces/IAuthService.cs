using System.Threading.Tasks;
using GastroHub.Dtos.Auth;
using GastroHub.Dtos.Users;

namespace GastroHub.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<UserDto> LoginAsync(LoginDto dto);
        Task<UserDto> GetUserAsync(string userId);
        Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    }
}
