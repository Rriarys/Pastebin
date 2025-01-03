﻿using Pastebin.Data;

public class UserPopularityCalculationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserPopularityCalculationService> _logger;

    public UserPopularityCalculationService(IServiceProvider serviceProvider, ILogger<UserPopularityCalculationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Небольшая задержка перед запуском

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var users = dbContext.Users.ToList();

                foreach (var user in users)
                {
                    // Фильтруем только публичные посты
                    var userPosts = dbContext.Posts
                        .Where(p => p.PostAuthorId == user.UserId && p.IsPublic && p.PostTTLSeconds == 3596400)
                        .ToList();

                    if (userPosts.Count > 0)
                    {
                        var now = DateTime.UtcNow;

                        // Вычисляем популярность, уменьшая влияние старых постов
                        user.UserPopularityScore = userPosts
                            .Select(p => p.PostPopularityScore * (1 / (1 + (now - p.PostCreationDate).Days))) // Уменьшаем популярность старых постов
                            .Sum() / userPosts.Count;
                    }
                    else
                    {
                        user.UserPopularityScore = 0; // Нет постов, значит популярность = 0
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("User popularity scores recalculated at {Time}.", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user popularity score calculation.");
            }

            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
           /* await Task.Delay(TimeSpan.FromHours(6), stoppingToken); // Интервал между выполнениями*/
        }
    }
}
