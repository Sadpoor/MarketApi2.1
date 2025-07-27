using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketApi.Migrations
{
    /// <inheritdoc />
    public partial class addtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UserID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "UserCartID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplyedDiscountCodeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Cart_Discounts_ApplyedDiscountCodeID",
                        column: x => x.ApplyedDiscountCodeID,
                        principalTable: "Discounts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserCartID",
                table: "Users",
                column: "UserCartID");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_ApplyedDiscountCodeID",
                table: "Cart",
                column: "ApplyedDiscountCodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Cart_UserCartID",
                table: "Users",
                column: "UserCartID",
                principalTable: "Cart",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Cart_UserCartID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserCartID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserCartID",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserID",
                table: "Products",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID");
        }
    }
}
