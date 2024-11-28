using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs
{
    public class LoginDto
    {
        public required string UserName { get; set; }

        public required string Password { get; set; }
    }
}
