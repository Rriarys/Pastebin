namespace Pastebin.Models
{
    public class Post
    {
        public int PostID { get; set; }

        public required string PostHash { get; set; }

        public required string PostTitle { get; set; }  // Название поста

        public int PostAuthorId { get; set; } //Внешний ключ

        public DateTime PostCreationDate { get; set; }

        public TimeSpan PostTTL { get; set; }  // Время жизни

        public int PostPopularityScore { get; set; }

        public bool IsPublic { get; set; }

        // Навигационное свойство для связи с User
        public required User PostAuthor { get; set; }
    }
}
