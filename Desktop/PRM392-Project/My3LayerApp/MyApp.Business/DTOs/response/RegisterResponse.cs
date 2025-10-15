namespace MyApp.Business.DTOs.response
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserInfo? User { get; set; }
    }
}
