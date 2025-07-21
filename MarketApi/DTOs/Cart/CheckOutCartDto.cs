using MarketApi.models;

namespace MarketApi.DTOs.Cart
{
    public class CheckOutCartDto
    {
        public List<MarketApi.models.Product> ProductsInCart;
        public decimal TotalPrice;
        public DiscountCode? AppliedDiscountCode;
    }
}
