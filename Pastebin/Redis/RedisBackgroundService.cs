using Microsoft.Extensions.Hosting;
using Pastebin.Redis;

namespace Pastebin.Services
{
    public class RedisBackgroundService : BackgroundService
    {
        private readonly RedisService _redisService;

        public RedisBackgroundService(RedisService redisService)
        {
            _redisService = redisService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Здесь логика обновления топов
                await UpdateRedisData();

                // Ждём 10 минут
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task UpdateRedisData()
        {
            // Получить актуальные данные из базы
            // Пример: обновить топовые посты
            // await _redisService.AddPostToTopAsync(...);
        }
    }
}
