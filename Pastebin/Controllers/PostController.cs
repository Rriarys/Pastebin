using Microsoft.AspNetCore.Mvc;
using Pastebin.DTOs;
using Pastebin.Services;

namespace Pastebin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        // Внедрение зависимости
        public PostController(PostService postService)
        {
            _postService = postService;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
        {

            // Проверяем, аутентифицирован ли пользователь
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User is not authenticated.");
            }

            // Использование PostService для создания поста
            var post = await _postService.CreatePostAsync(
                createPostDto.PostTitle,        // Название поста
                createPostDto.Text,             // Текст поста 
                userName,                       // Имя пользователя из токена 
                createPostDto.PostTTL,          // TTL
                createPostDto.IsPublic          // Публичность
            );

            // Создаем DTO для ответа
            var postDto = new PostDto
            {
                PostHash = post.PostHash,
                PostTitle = post.PostTitle,
                PostID = post.PostID,
                PostAuthorId = post.PostAuthorId,
                UserName = userName,
                PostCreationDate = post.PostCreationDate,
                PostTTL = post.PostTTL,
                PostPopularityScore = post.PostPopularityScore,
                IsPublic = post.IsPublic,
                PostAuthor = new UserDto
                {
                    UserId = post.PostAuthorId,
                    UserName = userName
                }
            };

            // Возвращаем успешный ответ с PostDto
            return CreatedAtAction(nameof(GetPost), new { postHash = post.PostHash }, postDto);
        }

        // GET: api/posts/{postHash}
        [HttpGet("{postHash}")]
        public async Task<IActionResult> GetPost(string postHash)
        {
            // Заглушка для получения поста
            // Например, вы можете использовать _postService для получения поста
            // var post = await _postService.GetPostAsync(postHash);

            // Пока возвращаем пустой объект
            return Ok(new { PostHash = postHash });
        }

        // DELETE: api/posts/{postHash}
        [HttpDelete("{postHash}")]
        public async Task<IActionResult> DeletePost(string postHash)
        {
            // Заглушка для удаления поста
            // var success = await _postService.DeletePostAsync(postHash);

            // Возвращаем успешный ответ
            return NoContent();
        }
    }
}
