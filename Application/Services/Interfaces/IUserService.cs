using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> RegisterAsync(RegisterModel model);
        Task<Account?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(string email, string newPassword);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<int> GetTotalUsersAsync();
    }
}
