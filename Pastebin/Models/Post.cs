namespace Pastebin.Models
{
    public class Post
    {
        public int PostID { get; set; }

        public required string PostHash { get; set; }

        public required string PostTitle { get; set; }  // Название поста

        public int PostAuthorId { get; set; } //Внешний ключ

        public DateTime PostCreationDate { get; set; }

        public int PostTTLSeconds { get; set; }  // Время жизни в секундах

        public int PostPopularityScore { get; set; }

        public bool IsPublic { get; set; }

        // Навигационное свойство для связи с User
        public required User PostAuthor { get; set; }

        // Дата, когда пост должен быть удален (для TTL)
        public DateTime? PostExpirationDate { get; set; } // Nullable, так как может быть отсутствовать для вечных постов
    }
}
