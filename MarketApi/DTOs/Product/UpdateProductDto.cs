using MarketApi.models;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.DTOs.Product
{
    public class UpdateProductDto
    { // برای به روز رسانی فقط فیلد های نیازمند تغییر را وارد کن
        public int Id { get; set; }
        [MinLength(3)] public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public CategoryEnum? Category { get; set; }

        public int? DiscountPrecent { get; set; }
    }
}
