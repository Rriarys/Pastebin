namespace Pastebin.DTOs
{
    public class PostDto
    {
        public required int PostID { get; set; }  // ID поста
        public required string PostHash { get; set; }  // Хэш поста
        public required int PostAuthorId { get; set; }  // ID автора поста
        public required string UserName { get; set; }  // Имя автора (из User)
        public DateTime PostCreationDate { get; set; }  // Дата создания поста
        public TimeSpan PostTTL { get; set; }  // Время жизни поста (TTL)
        public int PostPopularityScore { get; set; }  // Рейтинг поста
        public bool IsPublic { get; set; }  // Публичность поста

        // Можно также добавить информацию об авторе, если это важно
        public required UserDto PostAuthor { get; set; }  // Информация о пользователе (навигационное свойство)
    }
}
