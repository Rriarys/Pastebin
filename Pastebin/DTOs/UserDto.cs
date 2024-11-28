namespace Pastebin.DTOs
{
    public class UserDto
    {
        public required int UserId { get; set; }  // ID пользователя
        public required string UserName { get; set; }  // Имя пользователя
        public int UserPopularityScore { get; set; }  // Рейтинг пользователя
    }
}
