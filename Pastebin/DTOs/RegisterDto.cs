namespace Pastebin.DTOs
{
    public class RegisterDto
    {
        public required string UserName { get; set; } // Имя пользователя
        public required string Email { get; set; } // Почта пользователя
        public required string Password { get; set; } // Пароль пользователя
    }
}
