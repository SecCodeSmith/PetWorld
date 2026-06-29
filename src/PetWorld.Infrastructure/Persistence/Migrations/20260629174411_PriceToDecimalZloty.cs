using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetWorld.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PriceToDecimalZloty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename + retype in place (instead of drop/add) so existing rows are preserved,
            // then convert the stored value from grosze (int) to złoty (decimal). On a fresh
            // database the table is empty here (seeding runs after migrations), so the UPDATE
            // is a harmless no-op.
            migrationBuilder.RenameColumn(
                name: "PriceGr",
                table: "products",
                newName: "Price");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "products",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("UPDATE products SET Price = Price / 100;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE products SET Price = Price * 100;");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "products",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "products",
                newName: "PriceGr");
        }
    }
}
