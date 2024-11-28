using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pastebin.DTOs;
using Pastebin.Services;

namespace Pastebin.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public AuthController(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUserAsync(registerDto);

            if (result.Succeeded)
            {
                return Ok("Регистрация прошла успешно!");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.AuthenticateUserAsync(loginDto);

            if (user == null)
            {
                return Unauthorized("Неверные логин или пароль.");
            }

            var token = _tokenService.GenerateToken(user);

            // Устанавливаем токен в куки
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,  // Защищаем от доступа через JavaScript
                Secure = true,    // Только для https
                SameSite = SameSiteMode.Strict // Защищаем от атак Cross-Site Request Forgery
            });

            return Ok(new { Token = token });
        }

        // Защищенный эндпоинт для проверки авторизации
        [Authorize]
        [HttpGet("secure-endpoint")]
        public IActionResult GetSecureData()
        {
            return Ok("Доступ к защищенному ресурсу разрешен!");
        }

    }

}
