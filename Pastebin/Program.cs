using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Pastebin.Data;
using Pastebin.Services;
using StackExchange.Redis;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Pastebin.Models;
using Microsoft.OpenApi.Models;
using Azure.Storage.Blobs;
using Pastebin.Interfaces;

namespace Pastebin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter token in the format: 'Bearer <your-token>'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Redis
            var redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Получаем строку подключения для Azure Storage Blob
            string connectionString = builder.Configuration["AzureStorage:ConnectionString"];

            // Регистрация BlobServiceClient для DI
            builder.Services.AddSingleton(new BlobServiceClient(connectionString));

            // Регистрация BlobService (вместо Singleton — Scoped)
            builder.Services.AddScoped<IBlobService, BlobService>();

            // Регистрация хешировщика паролей
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Регистрация HashService с параметрами из конфигурации
            builder.Services.AddScoped<IHashService>(serviceProvider =>
            {
                var salt = builder.Configuration["HashService:Salt"];
                var minHashLength = int.Parse(builder.Configuration["HashService:MinHashLength"]);
                return new HashService(salt, minHashLength);
            });

            // Регистрация сервисов
            builder.Services.AddScoped<PostService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddHostedService<PostCleanupService>();
            builder.Services.AddHostedService<LikesBalanceResetService>();

            // Регистрация RedisService
            builder.Services.AddSingleton<RedisService>();
            // Регистрация фона сервиса RedisSyncService
            builder.Services.AddHostedService<RedisSyncService>();

            // Настройка аутентификации
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            // Добавляем политику CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    policy.WithOrigins("https://localhost:5500")  // или другой адрес клиента
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Применяем CORS
            app.UseCors("AllowLocalhost");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
