using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pastebin.DTOs;
using Pastebin.Services;
using System.Threading.Tasks;

namespace Pastebin.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public LoginModel(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [BindProperty]
        public LoginDto LoginDto { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userService.AuthenticateUserAsync(LoginDto);

            if (user == null)
            {
                ErrorMessage = "�������� ����� ��� ������.";
                return Page();
            }

            var token = _tokenService.GenerateToken(user);

            // ����� ����� ������������ ��������� ��������� ��� ���� ��� �������� ������
            Response.Cookies.Append("AuthToken", token, new CookieOptions { HttpOnly = true });

            return RedirectToPage("/Index");
        }
    }
}
