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

        public async Task<Post> CreatePostAsync(string title, string content, string userName, TimeSpan ttl, bool isPublic)
        {
            // 1. Загрузка текста поста в Blob Storage
            string fileName = $"{Guid.NewGuid()}.txt"; // Генерируем уникальное имя для файла
            string containerName = userName.ToLower(); // Имя контейнера - имя пользователя
            string blobLink = await _blobService.UploadTextAsync(containerName, fileName, content);


            // 2. Хеширование ссылки
            string postHash = _hashService.GenerateHash(blobLink);

            // 3. Получаем пользователя из базы данных по имени
            var postAuthor = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (postAuthor == null)
            {
                throw new Exception("User not found");
            }

            // 4. Создание объекта Post для базы данных
            var post = new Post
            {
                PostHash = postHash,
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

    }

}
