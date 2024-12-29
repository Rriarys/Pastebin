using StackExchange.Redis;

namespace Pastebin.Redis
{
    /// <summary>
    /// Интерфейс для работы с Redis в проекте Pastebin.
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// Добавляет пост в список топовых постов, упорядоченных по популярности.
        /// </summary>
        /// <param name="postId">Идентификатор поста.</param>
        /// <param name="popularityScore">Популярность поста (числовое значение).</param>
        Task AddPostToTopAsync(string postId, double popularityScore);

        /// <summary>
        /// Получает список идентификаторов топовых постов.
        /// </summary>
        /// <param name="count">Количество постов для возврата (по умолчанию 10).</param>
        /// <returns>Массив идентификаторов постов.</returns>
        Task<RedisValue[]> GetTopPostsAsync(int count = 10);

        /// <summary>
        /// Добавляет пользователя в список топовых пользователей, упорядоченных по популярности.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="popularityScore">Популярность пользователя (числовое значение).</param>
        Task AddUserToTopAsync(string username, double popularityScore);

        /// <summary>
        /// Получает список имен топовых пользователей.
        /// </summary>
        /// <param name="count">Количество пользователей для возврата (по умолчанию 10).</param>
        /// <returns>Массив имен пользователей.</returns>
        Task<RedisValue[]> GetTopUsersAsync(int count = 10);

        /// <summary>
        /// Сохраняет идентификатор поста в список постов, принадлежащих конкретному пользователю.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="postId">Идентификатор поста.</param>
        Task SaveUserPostAsync(string username, string postId);

        /// <summary>
        /// Получает список идентификаторов постов конкретного пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <returns>Массив идентификаторов постов.</returns>
        Task<RedisValue[]> GetUserPostsAsync(string username);

        /// <summary>
        /// Сохраняет содержимое файла поста в Redis.
        /// </summary>
        /// <param name="postId">Идентификатор поста.</param>
        /// <param name="content">Содержимое поста (текст или бинарные данные).</param>
        Task SaveFileContentAsync(string postId, string content);

        /// <summary>
        /// Получает содержимое файла поста по его идентификатору.
        /// </summary>
        /// <param name="postId">Идентификатор поста.</param>
        /// <returns>Содержимое поста в виде строки или null, если пост не найден.</returns>
        Task<string?> GetFileContentAsync(string postId);
    }


}
