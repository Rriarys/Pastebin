using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
        public async Task<Post> CreatePostAsync(string title, string content, string userName, TimeSpan ttl, bool isPublic)
        {
            // 1. Генерация имени файла и его хеширование
            string originalFileName = $"{Guid.NewGuid()}.txt"; // Генерация уникального имени файла
            string hashedFileName = _hashService.GenerateHash(originalFileName).Substring(0, 12); // Хешируем имя файла и обрезаем до 12 символов - это 4,738,381,338,321,616,896 столько комбинаций

            // 2. Используем захешированное имя файла для загрузки в Blob Storage
            string containerName = userName.ToLower(); // Имя контейнера - имя пользователя
            await _blobService.UploadTextAsync(containerName, hashedFileName, content); // Не сохраняем ссылку
            //string fullBlobUrl = await _blobService.GetBlobUrlAsync(containerName, post.PostHash); потом так восстановлю


            // 3. Получаем пользователя из базы данных по имени
            var postAuthor = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (postAuthor == null)
            {
                throw new Exception("User not found");
            }

            // 4. Создание объекта Post для базы данных
            var post = new Post
            {
                PostHash = hashedFileName, // В PostHash записываем хешированное имя файла
                PostTitle = title,
                PostAuthorId = postAuthor.UserId, // Используем UserId найденного пользователя
                PostAuthor = postAuthor, // Инициализируем навигационное свойство
                PostCreationDate = DateTime.UtcNow,
                PostTTL = ttl,
                PostPopularityScore = 0, // Пока 0, популярность будет рассчитываться позже
                IsPublic = isPublic
            };

            // 5. Сохранение поста в базе данных
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        // Метод для получения поста по хешу
        public async Task<Post> GetPostByHashAsync(string postHash)
        {
            var post = await _context.Posts
                .Include(p => p.PostAuthor)  // Подключаем информацию о пользователе
                .FirstOrDefaultAsync(p => p.PostHash == postHash);

            // Если пост не найден, выбрасываем исключение или возвращаем null, в зависимости от логики
            return post ?? throw new Exception("Post not found");
        }

        // Метод для удаления поста по хешу
        public async Task<bool> DeletePostAsync(string postHash)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostHash == postHash);

            if (post == null)
            {
                return false; // Если пост не найден
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true; // Удаление прошло успешно
        }

    }
}
