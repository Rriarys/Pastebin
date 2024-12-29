using StackExchange.Redis;
using System.Text.Json;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    // Сохранение метаданных поста в Redis
    public async Task SavePostMetadataAsync(string postHash, object metadata, TimeSpan? expiry = null)
    {
        string metadataJson = JsonSerializer.Serialize(metadata);
        await _db.StringSetAsync($"Post:{postHash}", metadataJson, expiry);
    }

    // Получение метаданных поста из Redis
    public async Task<T> GetPostMetadataAsync<T>(string postHash)
    {
        string metadataJson = await _db.StringGetAsync($"Post:{postHash}");
        return metadataJson != null ? JsonSerializer.Deserialize<T>(metadataJson) : default;
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
