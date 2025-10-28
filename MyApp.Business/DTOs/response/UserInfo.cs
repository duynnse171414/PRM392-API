namespace MyApp.Business.DTOs.response
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string Role { get; set; } = null!;
    }
}
