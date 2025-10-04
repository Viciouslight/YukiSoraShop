using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        User? GetUserById(int id);
        List<User> GetAllUsers();
    }
}
