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
                PostHash = post.PostHash,   // Нее забыть вырезать это
                PostTitle = post.PostTitle, 
                PostID = post.PostID, // это
                PostAuthorId = post.PostAuthorId, // это
                UserName = userName,
                PostCreationDate = post.PostCreationDate,
                PostTTL = post.PostTTL,
                PostPopularityScore = post.PostPopularityScore,
                IsPublic = post.IsPublic,
                PostAuthor = new UserDto // это
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
            // Получаем пост по хешу через сервис
            var post = await _postService.GetPostByHashAsync(postHash);

            // Если пост не найден, возвращаем 404
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            // Создаем DTO для ответа
            var postDto = new PostDto
            {
                PostHash = post.PostHash,
                PostTitle = post.PostTitle,
                PostID = post.PostID,
                PostAuthorId = post.PostAuthorId,
                UserName = post.PostAuthor.UserName, // Имя автора
                PostCreationDate = post.PostCreationDate,
                PostTTL = post.PostTTL,
                PostPopularityScore = post.PostPopularityScore,
                IsPublic = post.IsPublic,
                PostAuthor = new UserDto
                {
                    UserId = post.PostAuthorId,
                    UserName = post.PostAuthor.UserName
                }
            };

            // Возвращаем информацию о посте
            return Ok(postDto);
        }


        // DELETE: api/posts/{postHash}
        [HttpDelete("{postHash}")]
        public async Task<IActionResult> DeletePost(string postHash)
        {
            // Используем сервис для удаления поста
            var success = await _postService.DeletePostAsync(postHash);

            // Если пост не был найден или не удалось удалить, возвращаем 404
            if (!success)
            {
                return NotFound("Post not found or could not be deleted.");
            }

            // Возвращаем успешный ответ
            return NoContent();
        }

    }
}
