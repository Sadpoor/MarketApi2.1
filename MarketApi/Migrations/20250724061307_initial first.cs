using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketApi.Migrations
{
    /// <inheritdoc />
    public partial class initialfirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "phoneNumber",
                table: "Users",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<int>(
                name: "DiscountPrecent",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPrecent",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Users",
                newName: "phoneNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
