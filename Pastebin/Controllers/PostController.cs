using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; 
using Pastebin.DTOs;
using Pastebin.Models;

namespace Pastebin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
            // Заглушки для сервисов
            /*private readonly IPostService _postService;
            private readonly IHashService _hashService;

            public PostController(IPostService postService, IHashService hashService)
            {
                _postService = postService;
                _hashService = hashService;
            }*/

            // POST: api/posts
            [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            // Генерация хэша для поста
            string postHash = "dummyhash"; // Заглушка для хэша

            // Автоматическое определение даты создания
            DateTime postCreationDate = DateTime.Now;

            // Генерация TTL, если не передан (например, 1 день по умолчанию)
            TimeSpan postTTL = createPostDto.PostTTL != TimeSpan.Zero ? createPostDto.PostTTL : TimeSpan.FromDays(1);

            // Рейтинг по умолчанию (пока не учитываем)
            int postPopularityScore = 0;

            // ID пользователя (например, получаем его через текущую сессию или иным способом)
            int postAuthorId = 1; // Заглушка для ID пользователя

            // Создаем PostDto для ответа
            var postDto = new PostDto
            {
                PostHash = postHash,
                PostTitle = createPostDto.PostTitle,  // Здесь передаем название поста
                PostID = 123,  // Заглушка для ID
                PostAuthorId = postAuthorId,  // Устанавливаем обязательное поле
                UserName = createPostDto.UserName,  // Имя пользователя из DTO
                PostCreationDate = postCreationDate,  // Дата создания
                PostTTL = postTTL,  // Время жизни
                PostPopularityScore = postPopularityScore,  // Рейтинг
                IsPublic = createPostDto.IsPublic,  // Публичность из запроса
                PostAuthor = new UserDto  // Заглушка для информации о пользователе
                {
                    UserId = postAuthorId,  // Заглушка для ID пользователя
                    UserName = createPostDto.UserName  // Имя пользователя
                }
            };

            // Возвращаем успешный ответ с PostDto
            return CreatedAtAction(nameof(GetPost), new { postHash = postHash }, postDto);
        }

        // GET: api/posts/{postHash}
        [HttpGet("{postHash}")]
        public async Task<IActionResult> GetPost(string postHash)
        {
            // Заглушка для получения поста
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
