using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialGymsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Gyms");

            migrationBuilder.CreateTable(
                name: "Gyms",
                schema: "Gyms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gyms", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "Gyms",
                table: "Gyms",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Gym Central", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Gym Norte", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Gym Sur", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_IsActive",
                schema: "Gyms",
                table: "Gyms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_Name",
                schema: "Gyms",
                table: "Gyms",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_Name_IsActive",
                schema: "Gyms",
                table: "Gyms",
                columns: new[] { "Name", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gyms",
                schema: "Gyms");
        }
    }
}
