namespace Pastebin.DTOs
{
    public class UserDto
    {
        public required int UserId { get; set; }  // ID пользователя
        public required string UserName { get; set; }  // Имя пользователя
        public int LikesBalance { get; set; }  // Значение лайков, не забыть запрашивать дял клиентской части
    }
}
