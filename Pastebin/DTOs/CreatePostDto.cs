namespace Pastebin.DTOs
{
    public class CreatePostDto
    {
        public required string PostTitle { get; set; }  // Название поста

        public required string Text { get; set; }  // Текст поста
        public int PostTTLSeconds { get; set; }  // Время жизни поста (TTL)
        public bool IsPublic { get; set; }  // Публичность поста
    }
}
