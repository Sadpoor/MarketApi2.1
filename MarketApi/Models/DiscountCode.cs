using System.ComponentModel.DataAnnotations;

namespace MarketApi.models
{
    public class DiscountCode
    {
        [Required] public string Code { get; set; }
        [Required] public decimal DiscountPrecent { get; set; }
    }
}