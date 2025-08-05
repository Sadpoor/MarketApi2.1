//using MarketApi.Data.SeedData;
using MarketApi.Data.SeedData;
using MarketApi.Models.DiscountCode;
using MarketApi.Models.Products;
using MarketApi.Models.Rating;
using MarketApi.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace MarketApi.Data.MarketDb
{
    public class MarketDb : DbContext
    {
        public MarketDb(DbContextOptions<MarketDb> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<DiscountCode> Discounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cart>(entity =>
            {
                //entity.HasOne(c => c.User)
                //    .WithOne(u => u.UserCart)
                //    .HasForeignKey<Cart>(c => c.UserID);
               
                entity.HasOne(c => c.ApplyedDiscountCode)
                    .WithOne()
                    .HasForeignKey<Cart>(c => c.DiscountCodeID);

            });
            modelBuilder.Entity<CustomerRate>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Rates)
                    .HasForeignKey(r => r.UserID);
                entity.HasOne(r => r.ProductRate)
                    .WithMany(p => p.Rates)
                    .HasForeignKey(r => r.ProductRateID);
            });
            modelBuilder.Entity<ProductRate>(entity =>
            {
                entity.HasOne(p => p.Product)
                    .WithOne(a => a.Rate)
                    .HasForeignKey<ProductRate>(p => p.ProductID);


            });
            modelBuilder.Entity<User>(entity =>
            {   
                entity.HasOne(u => u.UserCart)
                    .WithOne(c => c.User)
                    .HasForeignKey<User>(u => u.CartID);
                entity.HasIndex(u => u.PhoneNumber)
                    .IsUnique();
            });

            //seedData
            
            //    modelBuilder.ApplyConfiguration(new UserSeedData());
            //modelBuilder.ApplyConfiguration(new ProductSeedData());

        }

    }

}
