using Application.Models;
using Application.DTOs;
using Application.Services.Interfaces;
using Application.IRepository;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepository;
        public UserService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public UserDto? GetUserById(int id)
        {
            var account = _accountRepository.GetByIdAsync(id).Result;
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

        public List<UserDto> GetAllUsers()
        {
            var accounts = _accountRepository.GetAllAsync().Result;
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
                var existingAccount = await _accountRepository.GetByEmailAsync(model.Email);
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
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "System",
                    IsDeleted = false
                };

                // Lưu password dạng plain text (không hash)
                account.Password = model.Password;

                // Lưu vào database
                await _accountRepository.AddAsync(account);
                await _accountRepository.SaveChangesAsync();

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
                var account = await _accountRepository.GetByEmailAsync(email);
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
                var account = await _accountRepository.GetByEmailAsync(email);
                if (account == null) return false;

                // Update password (plain text, no hashing as per user request)
                account.Password = newPassword;
                account.ModifiedAt = DateTime.Now;
                account.ModifiedBy = "User";

                _accountRepository.Update(account);
                await _accountRepository.SaveChangesAsync();

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
                return await _accountRepository.GetByEmailAsync(email);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
