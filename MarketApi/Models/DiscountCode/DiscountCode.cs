using System.ComponentModel.DataAnnotations;

namespace MarketApi.Models.DiscountCode
{
    public class DiscountCode
    {
        [Key] 
        public int ID { get; set; }
        public string Code { get; set; }
        public int DiscountPrecent { get; set; }

    }
}