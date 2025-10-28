using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        public UserService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var account = await _uow.AccountRepository.GetByIdAsync(id);
            if (account == null) return null;

            return new UserDto
            {
                Id = account.Id,
                Username = account.UserName,
                Email = account.Email,
                FullName = account.FullName ?? "",
                PhoneNumber = account.PhoneNumber ?? "",
                CreatedAt = account.CreatedAt
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var accounts = await _uow.AccountRepository.GetAllAsync();
            return accounts.Select(account => new UserDto
            {
                Id = account.Id,
                Username = account.UserName,
                Email = account.Email,
                FullName = account.FullName ?? "",
                PhoneNumber = account.PhoneNumber ?? "",
                CreatedAt = account.CreatedAt
            }).ToList();
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                // Kiểm tra email đã tồn tại chưa
                var existingAccount = await _uow.AccountRepository.GetByEmailAsync(model.Email);
                if (existingAccount != null)
                {
                    return false; // Email đã tồn tại
                }

                // Tạo account mới
                var account = new Account
                {
                    UserName = model.Email, // Sử dụng email làm username
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RoleId = 1, // Default to Customer role
                    Status = "Active",
                    IsExternal = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = "System",
                    IsDeleted = false
                };

                // Lưu password dạng plain text (không hash)
                account.Password = model.Password;

                // Lưu vào database
                await _uow.AccountRepository.AddAsync(account);
                await _uow.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Account?> LoginAsync(string email, string password)
        {
            try
            {
                var account = await _uow.AccountRepository.GetByEmailAsync(email);
                if (account == null) return null;

                if (password == account.Password)
                {
                    return account;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string email, string newPassword)
        {
            try
            {
                var account = await _uow.AccountRepository.GetByEmailAsync(email);
                if (account == null) return false;

                // Update password (plain text, no hashing as per user request)
                account.Password = newPassword;
                account.ModifiedAt = DateTime.UtcNow;
                account.ModifiedBy = "User";

                _uow.AccountRepository.Update(account);
                await _uow.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            try
            {
                return await _uow.AccountRepository.GetByEmailAsync(email);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
