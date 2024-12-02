using Microsoft.AspNetCore.Mvc;
using Pastebin.DTOs;
using Pastebin.Interfaces;
using Pastebin.Models;
using Pastebin.Services;

namespace Pastebin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly IBlobService _blobService;

        public PostController(PostService postService, IBlobService blobService)
        {
            _postService = postService;
            _blobService = blobService;
        }

        // POST: api/posts
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

            return CreatedAtAction(nameof(GetPost), new { postHash = post.PostHash }, postDto);
        }

        // GET: api/posts/{postHash}
        [HttpGet("{postHash}")]
        public async Task<IActionResult> GetPost(string postHash)
        {
            var post = await _postService.GetPostByHashAsync(postHash);

            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            var postDto = new PostDto
            {
                PostHash = post.PostHash,
                PostTitle = post.PostTitle,
                PostID = post.PostID,
                PostAuthorId = post.PostAuthorId,
                UserName = post.PostAuthor.UserName,
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

            return Ok(postDto);
        }

        // DELETE: api/posts/{postHash}
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
