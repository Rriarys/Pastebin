using Microsoft.EntityFrameworkCore;
using Pastebin.Data;
using Pastebin.Interfaces;
using Pastebin.Models;

namespace Pastebin.Services
{
    public class PostService
    {
        private readonly IBlobService _blobService;
        private readonly IHashService _hashService;
        private readonly ApplicationDbContext _context;

        public PostService(IBlobService blobService, IHashService hashService, ApplicationDbContext context)
        {
            _blobService = blobService;
            _hashService = hashService;
            _context = context;
        }

        // Метод для создания поста
        public async Task<Post?> CreatePostAsync(string title, string content, string userName, TimeSpan ttl, bool isPublic)
        {
            string originalFileName = $"{Guid.NewGuid()}.txt";
            string hashedFileName = _hashService.GenerateHash(originalFileName).Substring(0, 12) + ".txt";

            string containerName = userName.ToLower();
            await _blobService.UploadTextAsync(containerName, hashedFileName, content);

            var postAuthor = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (postAuthor == null)
            {
                return null;  // Возвращаем null, если пользователь не найден
            }

            var post = new Post
            {
                PostHash = hashedFileName,
                PostTitle = title,
                PostAuthorId = postAuthor.UserId,
                PostAuthor = postAuthor,
                PostCreationDate = DateTime.UtcNow,
                PostTTL = ttl,
                PostPopularityScore = 0,
                IsPublic = isPublic
            };

            // Если TTL задан, вычисляем дату истечения
            if (ttl != TimeSpan.Zero && ttl != TimeSpan.FromHours(999))
            {
                post.PostExpirationDate = post.PostCreationDate + ttl;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        // Метод для получения поста по хешу
        public async Task<Post?> GetPostByHashAsync(string postHash)
        {
            return await _context.Posts
                .Include(p => p.PostAuthor)
                .FirstOrDefaultAsync(p => p.PostHash == postHash);
        }

        // Метод для удаления поста по хешу
        public async Task<bool> DeletePostAsync(string postHash)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostHash == postHash);
            if (post == null)
            {
                return false;
            }

            string containerName = post.PostAuthor.UserName.ToLower();

            try
            {
                await _blobService.DeleteBlobAsync(containerName, post.PostHash);
            }
            catch (Exception)
            {
                return false;  // Если ошибка при удалении из Blob Storage, возвращаем false
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
