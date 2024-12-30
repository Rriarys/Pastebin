using Microsoft.EntityFrameworkCore;
using Pastebin.Data;
using Pastebin.Interfaces;

public class RedisSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisService _redisService;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
    private readonly ILogger<RedisSyncService> _logger;

    public RedisSyncService(IServiceProvider serviceProvider, RedisService redisService, ILogger<RedisSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _redisService = redisService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())  // Создаём scope для Scoped зависимостей
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();  // Получаем BlobService через scope

                    // Получаем топ-10 постов из базы
                    var topPosts = await dbContext.Posts
                        .Include(p => p.PostAuthor)  // Загружаем автора поста
                        .Where(p => p.IsPublic && (p.PostExpirationDate == null))
                        .OrderByDescending(p => p.PostPopularityScore)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    // Получаем хэши топ-10 постов
                    var topPostHashes = topPosts.Select(p => p.PostHash).ToList();

                    // Загружаем из Redis все ключи, которые относятся к постам
                    var redisKeys = await _redisService.GetAllPostHashesAsync();  // Предполагаем, что этот метод существует

                    bool changesMade = false;  // Флаг для отслеживания изменений

                    // Удаляем из Redis все посты, которые больше не попадают в топ-10
                    foreach (var postHash in redisKeys)
                    {
                        if (postHash == "TopPosts") continue;  // Игнорируем ключ TopPosts

                        if (!topPostHashes.Contains(postHash))
                        {
                            await _redisService.DeletePostDataAsync(postHash);
                            _logger.LogInformation("[RedisSyncService] Post with hash {PostHash} was removed from Redis.", postHash);
                            changesMade = true;
                        }
                    }

                    // Добавляем новые посты и обновляем их данные в Redis
                    foreach (var post in topPosts)
                    {
                        var postHash = post.PostHash;

                        // Сохраняем метаданные поста в Redis, если их там нет
                        var cachedPost = await _redisService.GetPostMetadataAsync<object>(postHash);
                        if (cachedPost == null)
                        {
                            await _redisService.SavePostMetadataAsync(postHash, post);
                            _logger.LogInformation("[RedisSyncService] Post with hash {PostHash} was added to Redis.", postHash);
                            changesMade = true;
                        }

                        // Проверяем содержимое файла
                        var cachedFileContent = await _redisService.GetFileContentAsync(postHash);
                        if (cachedFileContent == null)
                        {
                            var fileContent = await blobService.GetBlobContentAsync(post.PostAuthor.UserName.ToLower(), postHash);
                            if (fileContent != null)
                            {
                                await _redisService.SaveFileContentAsync(postHash, System.Text.Encoding.UTF8.GetString(fileContent));
                                _logger.LogInformation("[RedisSyncService] File content for post with hash {PostHash} was added to Redis.", postHash);
                                changesMade = true;
                            }
                        }
                    }

                    // Сохраняем обновлённый список хешей топовых постов
                    await _redisService.SavePostMetadataAsync("TopPosts", topPostHashes);

                    // Логируем информацию о том, были ли изменения
                    if (!changesMade)
                    {
                        _logger.LogInformation("[RedisSyncService] No changes were made to Redis data.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RedisSyncService] Error: {ex.Message}");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }
}
