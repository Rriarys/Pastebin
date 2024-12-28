using Pastebin.Data;

public class LikesBalanceResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LikesBalanceResetService> _logger;

    public LikesBalanceResetService(IServiceProvider serviceProvider, ILogger<LikesBalanceResetService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Небольшая задержка перед стартом

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Сброс баланса лайков
                var users = dbContext.Users.ToList();
                foreach (var user in users)
                {
                    user.LikesBalance = 10; // Устанавливаем значение
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Likes balance reset for {Count} users at {Time}.", users.Count, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during likes balance reset.");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
           /* await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Интервал между выполнениями*/
        }
    }
}
