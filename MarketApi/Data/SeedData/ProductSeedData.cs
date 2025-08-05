using MarketApi.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketApi.Data.SeedData
{
    public class ProductSeedData : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasData(
                new Product { ID = 1, Name = "tv", Category = CategoryEnum.TV, Description = "bla bla bla", DiscountPrecent = 10, Price = 1000, Sales = 5,Quantity= 5 },
                new Product { ID = 2, Name = "charger", Category = CategoryEnum.Accessory, Price = 1000, DiscountPrecent = 50, Description = "not bad" , Quantity = 10}
            );
            builder.Ignore(p => p.Rate);
        }

    }
}
