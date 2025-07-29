using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketApi.Models.Users;
using MarketApi.Models.Products;

public class initalDatas : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User { ID = 1, PhoneNumber = "09028648986", Password = "123456789" },
            new User { ID = 2, PhoneNumber = "09203695741", Password = "123456789", Role = RoleEnum.Admin }
        );
    }
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasData(
            new Product { ID = 1, Name = "tv", Category = CategoryEnum.TV, Description = "bla bla bla", DiscountPrecent = 10, Price = 1000, Sales = 5 },
            new Product { ID = 2, Name = "charger", Category = CategoryEnum.Accessory, Price = 1000, DiscountPrecent = 50, Description = "npt bad" }
        );
    }
}
