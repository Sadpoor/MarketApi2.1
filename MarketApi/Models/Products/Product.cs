using MarketApi.Models.Rating;
using MarketApi.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.Models.Products
{
    public class Product
    {

            [Key] 
            public int ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; } = string.Empty;
            public CategoryEnum Category { get; set; }


            public int Sales { get; set; } = 0;
            public ProductRate? Rate { get; set; }
            public int DiscountPrecent { get; set; } = 0;


            public decimal Price { get; set; }
            public int Quantity { get; set; } = 0;

            public List<Cart> Carts { get; set; } = new();

        }

        public enum CategoryEnum
        {
            Mobile,
            Laptop,
            TV,
            Gaming,
            Accessory
        }
    
}
