
using System.ComponentModel.DataAnnotations;

namespace MarketApi.DTOs.User
{
    public class AddUserDto
    {
        [Required][MinLength(8)] public string Password { get; set; }
        [Required] public string Name { get; set; }
        [Phone] public string? PhoneNumber { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; } = string.Empty;
    }
}
