using MarketApi.Models.Products;
using ali= MarketApi.Models.DiscountCode;
using MarketApi.Models.Rating;
using System.ComponentModel.DataAnnotations;
//using System.Diagnostics.Contracts;

namespace MarketApi.Models.Users
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        [Phone]
        public string PhoneNumber { get; set; } // is unique for logging in
        [MinLength(5)]
        public string Password { get; set; }  // also we can hash it
        public RoleEnum Role { get; set; } = RoleEnum.User;

        //fk to cart
        public int CartID { get; set; } = 0; 
        public Cart UserCart { get; set; } = new();

        public List<CustomerRate> Rates { get; set; } = new();
    }

    public class Cart
    {
        [Key]
        public int ID { get; set; }

        
        public List<Product> Products { get; set; } = new();

        //fk to discount 
        public int? DiscountCodeID { get; set; } =null;
        public ali.DiscountCode? ApplyedDiscountCode { get; set; } = null;

        //fk to user
        //public int UserID { get; set; }
        public User User { get; set; }
    }

    public enum RoleEnum
    {
        Admin,
        User
    }
}
