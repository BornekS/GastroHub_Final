using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroHub.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityPerUnit = table.Column<double>(type: "REAL", nullable: false),
                    CaloriesPer100g = table.Column<int>(type: "INTEGER", nullable: false),
                    ProteinPer100g = table.Column<double>(type: "REAL", nullable: false),
                    FatPer100g = table.Column<double>(type: "REAL", nullable: false),
                    CarbohydratesPer100g = table.Column<double>(type: "REAL", nullable: false),
                    Allergens = table.Column<string>(type: "TEXT", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingredients");
        }
    }
}
