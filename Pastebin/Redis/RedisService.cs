using StackExchange.Redis;

namespace Pastebin.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task AddPostToTopAsync(string postId, double popularityScore) =>
            await _db.SortedSetAddAsync(RedisKeys.TopPosts, postId, popularityScore);

        public async Task<RedisValue[]> GetTopPostsAsync(int count = 10) =>
            await _db.SortedSetRangeByRankAsync(RedisKeys.TopPosts, 0, count - 1, Order.Descending);

        public async Task AddUserToTopAsync(string username, double popularityScore) =>
            await _db.SortedSetAddAsync(RedisKeys.TopUsers, username, popularityScore);

        public async Task<RedisValue[]> GetTopUsersAsync(int count = 10) =>
            await _db.SortedSetRangeByRankAsync(RedisKeys.TopUsers, 0, count - 1, Order.Descending);

        // Для записи и возврата постов и файлов
        public async Task SaveUserPostAsync(string username, string postId)
        {
            await _db.ListLeftPushAsync(RedisKeys.UserPosts(username), postId);
        }

        public async Task<RedisValue[]> GetUserPostsAsync(string username)
        {
            return await _db.ListRangeAsync(RedisKeys.UserPosts(username));
        }

        public async Task SaveFileContentAsync(string postId, string content)
        {
            await _db.StringSetAsync(RedisKeys.File(postId), content);
        }

        public async Task<string?> GetFileContentAsync(string postId)
        {
            var content = await _db.StringGetAsync(RedisKeys.File(postId));
            return content.IsNullOrEmpty ? null : content.ToString();
        }

    }
}
