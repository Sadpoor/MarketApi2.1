using System.ComponentModel.DataAnnotations;

namespace MarketApi.models
{
    public class DiscountCode
    {
        [Key] public int ID { get; set; }
        [Required] public string Code { get; set; }
        [Required] public int DiscountPrecent { get; set; }
    }
}