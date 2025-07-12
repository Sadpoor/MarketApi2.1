using MarketApi.models;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.DTOs.Product
{
    public class AddProductDto //برای مقدار دهی اولیه باید همه را وارد کند
    {
        [Required][MinLength(3)] public string Name { get; set; }
        public string? Description { get; set; }
        [Required] public decimal Price { get; set; }
        [Required] [EnumDataType(typeof(CategoryEnum))]public CategoryEnum Category { get; set; }

        public float? Discount { get; set; } = 0;
    }
}
