namespace MarketApi.DTOs.User
{
    public class LoginUserDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
