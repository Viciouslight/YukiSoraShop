using Domain.Enums;

namespace YukiSoraShop.Services.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsAdmin();
        bool IsStaff();
        bool IsCustomer();
        bool HasRole(AccountRole role);
        AccountRole GetCurrentUserRole();
        string GetCurrentUserEmail();
        string GetCurrentUserName();
        int GetCurrentUserId();
    }
}
