using MarketApi.models;
using Microsoft.EntityFrameworkCore;

namespace MarketApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<DiscountCode> Discounts { get; set; }

    }
}
