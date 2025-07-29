using MarketApi.Models.Users;
using MarketApi.Models.Products;
using System.ComponentModel.DataAnnotations;
namespace MarketApi.Models.Rating
{
    public class ProductRate
    {
        [Key]
        public int ID { get; set; }
        public int Number { get; set; } = 0;
        public float Average { get; set; } = 0;
        public List<CustomerRate> Rates { get; set; } = new();

        //fk to product
        public int ProductID { get; set; }
        public Product Product { get; set; }

    }
}
