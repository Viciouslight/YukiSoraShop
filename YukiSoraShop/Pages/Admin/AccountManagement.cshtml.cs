using Application;
using Application.DTOs;
using Application.IRepository;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AccountManagementModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;

        public AccountManagementModel(IUserService userService, IUnitOfWork uow)
        {
            _userService = userService;
            _uow = uow;
        }

        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public string? Message { get; set; }
        public string? MessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Users = await _userService.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                Message = "Có lỗi xảy ra khi tải danh sách người dùng.";
                MessageType = "danger";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // Xử lý xóa mềm trực tiếp trong admin page, không thay đổi service
                var account = await _uow.AccountRepository.GetByIdAsync(id);
                if (account == null)
                {
                    Message = "Không thể xóa tài khoản. Tài khoản không tồn tại.";
                    MessageType = "danger";
                }
                else
                {
                    _uow.AccountRepository.SoftDelete(account);
                    await _uow.SaveChangesAsync();
                    Message = "Xóa tài khoản thành công.";
                    MessageType = "success";
                }
            }
            catch (Exception ex)
            {
                Message = $"Có lỗi xảy ra: {ex.Message}";
                MessageType = "danger";
            }

            Users = await _userService.GetAllUsersAsync();
            return Page();
        }
    }
}
