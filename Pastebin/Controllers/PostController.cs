using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pastebin.Data;
using Pastebin.DTOs;
using Pastebin.Interfaces;
using Pastebin.Services;

namespace Pastebin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly IBlobService _blobService;
        private readonly ApplicationDbContext _context;

        public PostController(PostService postService, IBlobService blobService, ApplicationDbContext context)
        {
            _postService = postService;
            _blobService = blobService;
            _context = context;
        }

        // POST: api/post
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var post = await _postService.CreatePostAsync(
                createPostDto.PostTitle,
                createPostDto.Text,
                userName,
                createPostDto.PostTTL,
                createPostDto.IsPublic
            );

            if (post == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Генерация ссылки на файл в Blob Storage
            string blobUrl = await _blobService.GetBlobUrlAsync(post.PostAuthor.UserName.ToLower(), post.PostHash);

            var postDto = new PostDto
            {
                PostHash = post.PostHash,
                PostTitle = post.PostTitle,
                PostID = post.PostID,
                PostAuthorId = post.PostAuthorId,
                UserName = userName,
                PostCreationDate = post.PostCreationDate,
                PostTTL = post.PostTTL,
                PostExpirationDate = post.PostExpirationDate,
                PostPopularityScore = post.PostPopularityScore,
                IsPublic = post.IsPublic,
                PostAuthor = new UserDto
                {
                    UserId = post.PostAuthorId,
                    UserName = userName
                },
                FileUrl = blobUrl // Добавление ссылки на файл
            };

            return CreatedAtAction(nameof(GetPost), new { postHash = post.PostHash }, postDto);
        }

        // GET: api/post/{postHash}
        [HttpGet("{postHash}")]
        public async Task<IActionResult> GetPost(string postHash)
        {
            var post = await _postService.GetPostByHashAsync(postHash);

            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }
            
            // Disposable - удаляем сразу
            if (post.PostTTL == TimeSpan.Zero && post.PostExpirationDate == null)
            {
                // Возвращаем данные поста пользователю
                string blobUrlDisposable = await _blobService.GetBlobUrlAsync(post.PostAuthor.UserName.ToLower(), post.PostHash);

                var postDtoDisposable = new PostDto
                {
                    PostHash = post.PostHash,
                    PostTitle = post.PostTitle,
                    PostID = post.PostID,
                    PostAuthorId = post.PostAuthorId,
                    UserName = post.PostAuthor.UserName,
                    PostCreationDate = post.PostCreationDate,
                    PostTTL = post.PostTTL,
                    PostExpirationDate = post.PostExpirationDate,
                    PostPopularityScore = post.PostPopularityScore,
                    IsPublic = post.IsPublic,
                    PostAuthor = new UserDto
                    {
                        UserId = post.PostAuthorId,
                        UserName = post.PostAuthor.UserName
                    },
                    FileUrl = blobUrlDisposable
                };

                // Обновляем PostExpirationDate
                post.PostExpirationDate = post.PostCreationDate;

                // Сохраняем изменения в базе данных
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                return Ok(postDtoDisposable);
            }

            // Проверяем истек ли TTL
            if (post.PostExpirationDate.HasValue && post.PostExpirationDate <= DateTime.UtcNow)
            {
                return NotFound(new { message = "Post has expired and was deleted" }); // Мы говорим что пост удален, но по факту он будет удалян фоновым сервисом при следующей итерации :)
            }


            // Получаем ссылку на файл в Blob Storage
            string blobUrl = await _blobService.GetBlobUrlAsync(post.PostAuthor.UserName.ToLower(), post.PostHash);

            var postDto = new PostDto
            {
                PostHash = post.PostHash,
                PostTitle = post.PostTitle,
                PostID = post.PostID,
                PostAuthorId = post.PostAuthorId,
                UserName = post.PostAuthor.UserName,
                PostCreationDate = post.PostCreationDate,
                PostTTL = post.PostTTL,
                PostExpirationDate = post.PostExpirationDate,
                PostPopularityScore = post.PostPopularityScore,
                IsPublic = post.IsPublic,
                PostAuthor = new UserDto
                {
                    UserId = post.PostAuthorId,
                    UserName = post.PostAuthor.UserName
                },
                FileUrl = blobUrl // Добавление ссылки на файл
            };

            return Ok(postDto);
        }

        // PATCH: api/post/{postHash}/popularity
        [HttpPatch("{postHash}/popularity")]
        public async Task<IActionResult> IncrementPostPopularity(string postHash)
        {
            // Проверяем, что пользователь аутентифицирован и имеем доступ к имени
            var userId = User?.Identity?.Name;
            if (userId == null)
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            // Получаем пост по хэшу
            var post = await _postService.GetPostByHashAsync(postHash);
            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }

            // Проверяем, является ли пост публичным
            if (!post.IsPublic)
            {
                return BadRequest(new { message = "Cannot like a private post." });
            }

            // Получаем пользователя из базы данных
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            // Проверяем баланс лайков
            if (user.LikesBalance <= 0)
            {
                return BadRequest(new { message = "Not enough likes available." });
            }

            // Проверяем, не ставит ли пользователь лайк на свой собственный пост
            if (post.PostAuthor.UserId == user.UserId)
            {
                return BadRequest(new { message = "Cannot like your own post." });
            }

            try
            {
                // Уменьшаем баланс лайков пользователя
                user.LikesBalance -= 1;

                // Увеличиваем популярность поста
                post.PostPopularityScore += 1;

                // Обновляем данные в базе
                _context.Users.Update(user);
                _context.Posts.Update(post);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Post popularity incremented successfully." });
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Error incrementing post popularity: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while incrementing post popularity." });
            }
        }

        // DELETE: api/post/{postHash}
        [HttpDelete("{postHash}")]
        public async Task<IActionResult> DeletePost(string postHash)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var post = await _postService.GetPostByHashAsync(postHash);
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            if (post.PostAuthor.UserName != userName)
            {
                return Forbid("You are not authorized to delete this post.");
            }

            string containerName = post.PostAuthor.UserName.ToLower();

            try
            {
                await _blobService.DeleteBlobAsync(containerName, post.PostHash);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Error deleting file from Blob Storage" });
            }

            var success = await _postService.DeletePostAsync(postHash);

            if (!success)
            {
                return NotFound(new { message = "Post not found" });
            }

            return Ok(new { message = "Post successfully deleted" });
        }
    }
}
