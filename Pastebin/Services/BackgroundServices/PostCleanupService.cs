using Pastebin.Data;
using Pastebin.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PostCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostCleanupService> _logger;

    public PostCleanupService(IServiceProvider serviceProvider, ILogger<PostCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Стартует не сразу

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

                var now = DateTime.UtcNow;

                // Получаем посты с истекшим сроком действия, с явной загрузкой PostAuthor
                var expiredPosts = dbContext.Posts
                    .Include(p => p.PostAuthor)  // Явно загружаем пользователя
                    .Where(p => p.PostExpirationDate != null && p.PostExpirationDate <= now)
                    .ToList();

                foreach (var post in expiredPosts)
                {
                    if (post.PostAuthor == null)
                    {
                        _logger.LogError("Post {PostHash} does not have an associated PostAuthor.", post.PostHash);
                        continue;
                    }

                    // Удаление из базы и Blob Storage
                    dbContext.Posts.Remove(post);
                    await blobService.DeleteBlobAsync(post.PostAuthor.UserName.ToLower(), post.PostHash);
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("{Count} expired posts deleted at {Time}.", expiredPosts.Count, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during post cleanup.");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Пауза перед следующей итерацией
        }
    }
}
