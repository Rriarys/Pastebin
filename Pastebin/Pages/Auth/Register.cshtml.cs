using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pastebin.DTOs;
using Pastebin.Services;
using System.Threading.Tasks;

namespace Pastebin.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;

        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegisterDto RegisterDto { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.RegisterUserAsync(RegisterDto);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            ErrorMessage = "Ошибка при регистрации: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return Page();
        }
    }
}
