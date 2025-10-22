using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    public class AccountManagerModel : PageModel
    {
        public class AccountViewModel
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public bool IsAdmin { get; set; }
        }
        public IList<AccountViewModel> Accounts { get; set; }

        public void OnGet()
        {
            Accounts = new List<AccountViewModel>
            {
                new AccountViewModel { Id = "1", Username = "admin01", Email = "admin01@shop.com", IsAdmin = true },
                new AccountViewModel { Id = "2", Username = "managerA", Email = "managerA@shop.com", IsAdmin = true },
                new AccountViewModel { Id = "3", Username = "supportB", Email = "supportB@shop.com", IsAdmin = false }
            };
        }
    }
}
