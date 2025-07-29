using System.ComponentModel.DataAnnotations;

namespace MarketApi.DTOs.Actions
{
    public class RateProductDto
    {
        [Range(1,5)]
        public int Rate { get; set; }
        public string Discription { get; set; } = string.Empty;

        //fk to user
        public int UserID { get; set; }
        //fk to product
        public int ProductID { get; set; }
    }
}
