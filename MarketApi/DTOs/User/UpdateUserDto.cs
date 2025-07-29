
using System.ComponentModel.DataAnnotations;
//using System.Runtime.CompilerServices;

namespace MarketApi.DTOs.User
{
    public class UpdateUserDto
    {
        [MinLength(8)] public string? Password { get; set; }
        [Phone] public string? PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;

    }
}
