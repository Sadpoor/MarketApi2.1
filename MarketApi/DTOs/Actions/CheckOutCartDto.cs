using ali = MarketApi.Models.Products;
using ali2 =MarketApi.Models.DiscountCode; // that is the worse way but i must do it 
namespace MarketApi.DTOs.Cart
{
    public class CheckOutCartDto
    {
        public List<ali.Product> ProductsInCart;
        public decimal TotalPrice;
        public ali2.DiscountCode? AppliedDiscountCode;
    }
}
