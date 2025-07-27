using System.ComponentModel.DataAnnotations;

namespace MarketApi.models
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string Password { get; set; }  // also we can hash it
        public string Name { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public RoleEnum Role { get; set; } = RoleEnum.User;
        public Cart UserCart { get; set; } = new();

    }

    public enum RoleEnum
    {
        Admin,
        User
    }

    public class Cart
    {
        [Key] public int ID { get; set; }
        public List<Product> Products = new();
        public DiscountCode? ApplyedDiscountCode { get; set; } = null;
    }
}
