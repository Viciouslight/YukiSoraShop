using Application.Services.Interfaces;
using Application.DTOs;
using Domain.Entities;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<bool> RegisterAsync(AccountRegistrationDTO model)
        {
            try
            {
                var existing = await _uow.AccountRepository.GetByEmailAsync(model.Email);
                if (existing != null) return false;

                var account = new Account
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RoleId = 1,
                    Status = "Active",
                    IsExternal = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = "System",
                    IsDeleted = false,
                    Password = model.Password // plain text for now; replace with hashing
                };

                await _uow.AccountRepository.AddAsync(account);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
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
                return password == account.Password ? account : null;
            }
            catch
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
                account.Password = newPassword;
                account.ModifiedAt = DateTime.UtcNow;
                account.ModifiedBy = "User";
                _uow.AccountRepository.Update(account);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<Account?> GetAccountByEmailAsync(string email)
        {
            return _uow.AccountRepository.GetByEmailAsync(email);
        }

        public Task LogoutAsync()
        {
            // Domain-level logout hook (no-op). Web layer handles cookie sign-out.
            return Task.CompletedTask;
        }
    }
}

