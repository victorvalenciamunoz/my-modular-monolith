using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                schema: "Gyms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RequiresSchedule = table.Column<bool>(type: "bit", nullable: false),
                    RequiresInstructor = table.Column<bool>(type: "bit", nullable: false),
                    HasCapacityLimits = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GymProducts",
                schema: "Gyms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GymId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MinCapacity = table.Column<int>(type: "int", nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true),
                    InstructorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InstructorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InstructorPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Equipment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymProducts_Gyms_GymId",
                        column: x => x.GymId,
                        principalSchema: "Gyms",
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GymProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Gyms",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Gyms",
                table: "Products",
                columns: new[] { "Id", "BasePrice", "CreatedAt", "Description", "HasCapacityLimits", "IsActive", "Name", "RequiresInstructor", "RequiresSchedule", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 35.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Acceso completo a la sala de musculación con equipos de última generación", true, true, "Acceso Sala de Musculación", false, false, null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 15.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Clase de spinning de alta intensidad con música motivadora", true, true, "Clase de Spinning", true, true, null },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 45.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sesión de entrenamiento personalizado con instructor especializado", false, true, "Entrenamiento Personal", true, true, null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), 50.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Consulta personalizada con nutricionista certificado", false, true, "Consulta Nutricional", true, true, null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), 12.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Clase de zumba divertida y energética para todos los niveles", true, true, "Clase de Zumba", true, true, null }
                });

            migrationBuilder.InsertData(
                schema: "Gyms",
                table: "GymProducts",
                columns: new[] { "Id", "CreatedAt", "DiscountPercentage", "Equipment", "GymId", "InstructorEmail", "InstructorName", "InstructorPhone", "IsActive", "MaxCapacity", "MinCapacity", "Notes", "Price", "ProductId", "Schedule", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-aaaa-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null, null, true, 50, null, null, 35.00m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "{}", null },
                    { new Guid("11111111-1111-1111-bbbb-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new Guid("33333333-3333-3333-3333-333333333333"), null, "Carlos Rodríguez", null, true, 20, 5, null, 15.00m, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "{\"monday\":[\"19:00-20:00\"],\"wednesday\":[\"19:00-20:00\"],\"friday\":[\"19:00-20:00\"]}", null },
                    { new Guid("22222222-2222-2222-aaaa-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10.0m, null, new Guid("44444444-4444-4444-4444-444444444444"), null, null, null, true, 40, null, null, 30.00m, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "{}", null },
                    { new Guid("22222222-2222-2222-eeee-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new Guid("44444444-4444-4444-4444-444444444444"), null, "María González", null, true, 25, 8, null, 12.00m, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "{\"tuesday\":[\"18:30-19:30\"],\"thursday\":[\"18:30-19:30\"],\"saturday\":[\"10:00-11:00\"]}", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GymProducts_GymId",
                schema: "Gyms",
                table: "GymProducts",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_GymProducts_GymId_IsActive",
                schema: "Gyms",
                table: "GymProducts",
                columns: new[] { "GymId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GymProducts_GymId_ProductId",
                schema: "Gyms",
                table: "GymProducts",
                columns: new[] { "GymId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymProducts_IsActive",
                schema: "Gyms",
                table: "GymProducts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GymProducts_ProductId",
                schema: "Gyms",
                table: "GymProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                schema: "Gyms",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "Gyms",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GymProducts",
                schema: "Gyms");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "Gyms");
        }
    }
}
