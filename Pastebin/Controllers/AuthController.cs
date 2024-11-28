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

        public AuthController(UserService userService)
        {
            _userService = userService;
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
    }

}
