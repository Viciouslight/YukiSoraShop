using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        UserDto? GetUserById(int id);
        List<UserDto> GetAllUsers();
        Task<bool> RegisterAsync(RegisterModel model);
        Task<Account?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(string email, string newPassword);
        Task<Account?> GetAccountByEmailAsync(string email);
    }
}
