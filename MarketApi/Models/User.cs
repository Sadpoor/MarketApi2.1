using MarketApi.models;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.models
{
    public class User
    {
        public int Id { get; set; }
        public string Password { get; set; }  // also we can hash it
        public string Name { get; set; }
        [Phone] public string? phoneNumber { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public RoleEnum Role { get; set; }
        public List<Product> cart { get; set; } = new(); 

    }

    public enum RoleEnum
    {
        Admin,
        User
    }
}
