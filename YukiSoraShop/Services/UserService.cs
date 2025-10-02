using YukiSoraShop.Models;

namespace YukiSoraShop.Services
{
    public class UserService : IUserService
    {
        private static readonly List<User> _users = new()
        {
            new User
            {
                Id = 1,
                Username = "john_doe",
                Email = "john@example.com",
                FullName = "John Doe",
                PhoneNumber = "0123456789",
                Address = "123 Main Street, Ho Chi Minh City",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "Male",
                AvatarUrl = "https://via.placeholder.com/150/0066cc/ffffff?text=JD",
                CreatedAt = DateTime.Now.AddMonths(-6)
            },
            new User
            {
                Id = 2,
                Username = "jane_smith",
                Email = "jane@example.com",
                FullName = "Jane Smith",
                PhoneNumber = "0987654321",
                Address = "456 Oak Avenue, Ho Chi Minh City",
                DateOfBirth = new DateTime(1985, 8, 20),
                Gender = "Female",
                AvatarUrl = "https://via.placeholder.com/150/ff6666/ffffff?text=JS",
                CreatedAt = DateTime.Now.AddMonths(-3)
            }
        };

        public User? GetUserById(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }
    }
}
