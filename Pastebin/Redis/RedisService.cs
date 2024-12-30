using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    // Сериализация с учетом цикличности
    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,  // Эта настройка решает проблему цикличности
        MaxDepth = 64  // Максимальная глубина сериализации (можно настроить по необходимости)
    };

    // Сохранение метаданных поста в Redis
    public async Task SavePostMetadataAsync(string postHash, object metadata, TimeSpan? expiry = null)
    {
        string metadataJson = JsonSerializer.Serialize(metadata, _jsonSerializerOptions);  // Используем настроенную сериализацию
        await _db.StringSetAsync($"Post:{postHash}", metadataJson, expiry);
    }

    // Получение метаданных поста из Redis
    public async Task<T> GetPostMetadataAsync<T>(string postHash)
    {
        string metadataJson = await _db.StringGetAsync($"Post:{postHash}");
        return metadataJson != null ? JsonSerializer.Deserialize<T>(metadataJson, _jsonSerializerOptions) : default;
    }

    // Получение хешей для обновления списка
    public async Task<List<string>> GetAllPostHashesAsync()
    {
        var server = _redis.GetServer("localhost", 6379);  // Указываем сервер Redis
        var keys = server.Keys(pattern: "Post:*");  // Получаем все ключи с префиксом "Post:"
        var postHashes = keys.Select(k => k.ToString().Split(':')[1]).ToList();  // Извлекаем хеши из ключей "Post:"

        // Теперь получаем все ключи с префиксом "FileContent:" и извлекаем хеши из них
        var fileContentKeys = server.Keys(pattern: "FileContent:*");
        var fileContentHashes = fileContentKeys.Select(k => k.ToString().Split(':')[1]).ToList();  // Извлекаем хеши из ключей "FileContent:"

        // Возвращаем объединённый список хешей
        return postHashes.Concat(fileContentHashes).ToList();
    }

    // Сохранение содержимого файла в Redis
    public async Task SaveFileContentAsync(string postHash, string content, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync($"FileContent:{postHash}", content, expiry);
    }

    // Получение содержимого файла из Redis
    public async Task<string> GetFileContentAsync(string postHash)
    {
        return await _db.StringGetAsync($"FileContent:{postHash}");
    }

    // Удаление данных из Redis
    public async Task DeletePostDataAsync(string postHash)
    {
        await _db.KeyDeleteAsync(new RedisKey[] { $"Post:{postHash}", $"FileContent:{postHash}" });
    }
}
