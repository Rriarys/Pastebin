using Microsoft.AspNetCore.Mvc;
using Pastebin.Redis;

namespace Pastebin.Controllers
{
    [ApiController]
    [Route("api/redis")]
    public class RedisController : ControllerBase
    {
        private readonly RedisService _redisService;

        public RedisController(RedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpGet("top-posts")]
        public async Task<IActionResult> GetTopPosts()
        {
            var topPosts = await _redisService.GetTopPostsAsync();
            return Ok(topPosts);
        }

        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers()
        {
            var topUsers = await _redisService.GetTopUsersAsync();
            return Ok(topUsers);
        }

        [HttpGet("user-posts/{username}")]
        public async Task<IActionResult> GetUserPosts(string username)
        {
            var userPosts = await _redisService.GetUserPostsAsync(username);
            return Ok(userPosts);
        }

        [HttpGet("file/{postId}")]
        public async Task<IActionResult> GetFileContent(string postId)
        {
            var content = await _redisService.GetFileContentAsync(postId);
            if (content == null)
                return NotFound();

            return Ok(content);
        }
    }
}
