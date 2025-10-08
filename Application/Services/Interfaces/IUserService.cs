using Application.Models;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        User? GetUserById(int id);
        List<User> GetAllUsers();
        Task<bool> RegisterAsync(Application.Models.RegisterModel model);
        Task<Account?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(string email, string newPassword);
        Task<Account?> GetAccountByEmailAsync(string email);
    }
}
