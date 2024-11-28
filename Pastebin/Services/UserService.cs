using Microsoft.AspNetCore.Identity;
using Pastebin.Data;
using Pastebin.Models;
using Pastebin.DTOs;
using Microsoft.EntityFrameworkCore;
using Pastebin.Helpers;

namespace Pastebin.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher; // Хешировщик паролей

        public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
        {
            // Проверка, существует ли уже пользователь с таким именем или почтой
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == registerDto.UserName || u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь с таким именем или почтой уже существует" });
            }

            // Хеширование пароля
            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

            // Создание нового пользователя
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                UserPopularityScore = 0 // Начальный рейтинг
            };

            // Добавление пользователя в базу данных
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}
