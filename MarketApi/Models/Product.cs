using System.ComponentModel.DataAnnotations;

namespace MarketApi.models
{
    public class Product
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; } = string.Empty;
        [Required] public decimal Price { get; set; }
        [Required] public CategoryEnum Category { get; set; }
        public int? Sales { get; set; } = 0;
        public RateClass? Rates { get; set; }
        [Required] public decimal Discount { get; set; }
        [Required] public List<InventoryClass>? Inventory { get; set; }
    }

    public enum CategoryEnum
    {
        Mobile,
        Laptop,
        TV,
        Gaming,
        Accessory
    }

    public class RateClass
    {
        public int Number { get; set; } = 0;
        public float Average { get; set; } = 0;
    }

    public class InventoryClass
    {
        public int Quantity { get; set; } = 0;
        public int Price { get; set; } = 0;
    }
}



