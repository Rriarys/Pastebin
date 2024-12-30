using System;
using System.Text.Json.Serialization;

namespace Pastebin.DTOs
{
    public class PostDto
    {
        public required int PostID { get; set; }  // ID поста
        public required string PostHash { get; set; }  // Хэш поста
        public required string PostTitle { get; set; } // Тайтл
        public required int PostAuthorId { get; set; }  // ID автора поста

        [JsonIgnore] // Игнорируем это поле, так как оно не приходит из Redis
        public string UserName { get; set; }  // Имя автора (из User)

        public DateTime PostCreationDate { get; set; }  // Дата создания поста
        public int PostTTLSeconds { get; set; }  // Время жизни поста (TTL)
        public int PostPopularityScore { get; set; }  // Рейтинг поста
        public bool IsPublic { get; set; }  // Публичность поста

        // Можно также добавить информацию об авторе, если это важно
        public required UserDto PostAuthor { get; set; }  // Информация о пользователе (навигационное свойство)

        [JsonIgnore] // Игнорируем это поле, так как оно не приходит из Redis
        public string FileUrl { get; set; } // Ссылка на файл

        // Дата, когда пост должен быть удален (для TTL)
        public DateTime? PostExpirationDate { get; set; } // Nullable, так как может быть отсутствовать для вечных постов
    }
}
