using YukiSoraShop.Models;

namespace YukiSoraShop.Services
{
    public interface IUserService
    {
        User? GetUserById(int id);
        List<User> GetAllUsers();
    }
}
