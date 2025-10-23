using Domain.Enums;
using YukiSoraShop.Services.Interfaces;

namespace YukiSoraShop.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAdmin()
        {
            var roleId = GetCurrentUserRoleId();
            return roleId == 2; // Administrator role ID in database
        }

        public bool IsStaff()
        {
            var roleId = GetCurrentUserRoleId();
            return roleId == 3; // Moderator role ID in database
        }

        public bool IsCustomer()
        {
            var roleId = GetCurrentUserRoleId();
            return roleId == 1; // Customer role ID in database
        }

        public bool HasRole(AccountRole role)
        {
            var roleId = GetCurrentUserRoleId();
            return roleId == (int)role;
        }

        public AccountRole GetCurrentUserRole()
        {
            var roleId = GetCurrentUserRoleId();
            return (AccountRole)roleId;
        }

        public string GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserEmail") ?? string.Empty;
        }

        public string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? string.Empty;
        }

        public int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            return int.TryParse(userIdString, out var userId) ? userId : 0;
        }

        private int GetCurrentUserRoleId()
        {
            var roleIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            return int.TryParse(roleIdString, out var roleId) ? roleId : (int)AccountRole.Customer;
        }
    }
}
