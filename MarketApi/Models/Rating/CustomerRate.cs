using MarketApi.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.Models.Rating
{
    public class CustomerRate
    {
        [Key] 
        public int ID { get; set; }
        [Range(1, 5)] 
        public int? Rate { get; set; } = null;
        public string? Discription { get; set; }

        ///fk to user
        public int UserID { get; set; }
        public User? User { get; set; }

        //fk to RealeaseRate
        public int ProductRateID { get; set; }
        public ProductRate ProductRate { get; set; }
    }
}
