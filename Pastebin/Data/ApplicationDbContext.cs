using Microsoft.EntityFrameworkCore;
using Pastebin.Models;

namespace Pastebin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        // Доп натсройка модели
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка отношений между постами и юзерами
            modelBuilder.Entity<Post>()
            .HasOne(p => p.PostAuthor) // Связь с моделью User
            .WithMany(u => u.Posts) // Коллекция постов в User
            .HasForeignKey(p => p.PostAuthorId) // Внешний ключ
            .OnDelete(DeleteBehavior.Restrict); // Рекомендуется использовать Restrict для предотвращения каскадных удалений
        }
    }
}
