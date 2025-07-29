using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketApi.Models.Users;

public class UserSeedData : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User { ID = 1, PhoneNumber = "09028648986", Password = "123456789" },
            new User { ID = 2, PhoneNumber = "09203695741", Password = "123456789", Role = RoleEnum.Admin }
        );
        builder.Ignore(p => p.UserCart);
        builder.Ignore(p => p.Rates);

    }

}
