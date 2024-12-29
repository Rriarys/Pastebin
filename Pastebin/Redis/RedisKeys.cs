namespace Pastebin.Redis
{
    public static class RedisKeys
    {
        public const string TopPosts = "TopPosts"; // Сортированный набор для топовых постов
        public const string TopUsers = "TopUsers"; // Сортированный набор для топовых юзеров

        // Динамический ключ для постов конкретного пользователя
        public static string UserPosts(string username) => $"UserPosts:{username}";

        // Динамический ключ для файлов постов
        public static string File(string postId) => $"File:{postId}";
    }
}
