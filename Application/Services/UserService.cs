using Application.DTOs;
using Application.IRepository;
using Application.Services.Interfaces;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        public UserService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AccountDTO?> GetUserByIdAsync(int id)
        {
            var account = await _uow.AccountRepository.GetByIdAsync(id);
            if (account == null) return null;

            return new AccountDTO
            {
                Id = account.Id,
                Username = account.UserName,
                Email = account.Email,
                FullName = account.FullName ?? "",
                PhoneNumber = account.PhoneNumber ?? "",
                Address = account.Address ?? string.Empty,
                DateOfBirth = account.DateOfBirth,
                Gender = account.Gender ?? string.Empty,
                AvatarUrl = account.AvatarUrl ?? string.Empty,
                RoleId = account.RoleId,
                Status = account.Status,
                IsExternal = account.IsExternal,
                ExternalProvider = account.ExternalProvider,
                CreatedAt = account.CreatedAt,
                ModifiedAt = account.ModifiedAt
            };
        }

        public async Task<List<AccountDTO>> GetAllUsersAsync()
        {
            var accounts = await _uow.AccountRepository.GetAllAsync();
            return accounts.Select(account => new AccountDTO
            {
                Id = account.Id,
                Username = account.UserName,
                Email = account.Email,
                FullName = account.FullName ?? "",
                PhoneNumber = account.PhoneNumber ?? "",
                Address = account.Address ?? string.Empty,
                DateOfBirth = account.DateOfBirth,
                Gender = account.Gender ?? string.Empty,
                AvatarUrl = account.AvatarUrl ?? string.Empty,
                RoleId = account.RoleId,
                Status = account.Status,
                IsExternal = account.IsExternal,
                ExternalProvider = account.ExternalProvider,
                CreatedAt = account.CreatedAt,
                ModifiedAt = account.ModifiedAt
            }).ToList();
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileCommand command)
        {
            try
            {
                var acc = await _uow.AccountRepository.GetByIdAsync(command.AccountId);
                if (acc == null) return false;

                acc.FullName = command.FullName?.Trim() ?? string.Empty;
                acc.PhoneNumber = command.PhoneNumber?.Trim();
                acc.Address = command.Address?.Trim() ?? string.Empty;
                if (command.DateOfBirth.HasValue)
                {
                    acc.DateOfBirth = command.DateOfBirth.Value;
                }
                acc.Gender = command.Gender?.Trim() ?? string.Empty;
                acc.AvatarUrl = string.IsNullOrWhiteSpace(command.AvatarUrl) ? string.Empty : command.AvatarUrl.Trim();
                acc.ModifiedAt = DateTime.UtcNow;
                acc.ModifiedBy = string.IsNullOrWhiteSpace(command.ModifiedBy) ? "user" : command.ModifiedBy;

                _uow.AccountRepository.Update(acc);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Auth-related methods moved to AuthService
    }
}
