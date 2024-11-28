namespace Pastebin.Models
{
    public class User
    {
        public int UserId { get; set; }

        public required string UserName { get; set; }

        public required string Email { get; set; } // Почта пользователя

        public required string PasswordHash { get; set; } // Хэш пароля

        public int UserPopularityScore { get; set; }

        // Коллкция постов юзера
        public ICollection<Post> Posts { get; set; }

        public User()
        {
            Posts = new List<Post>(); // Инициализация коллекции, если конструктор пустой
        }
    }
}
