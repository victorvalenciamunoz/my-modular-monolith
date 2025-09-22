using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                schema: "Gyms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GymProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_GymProducts_GymProductId",
                        column: x => x.GymProductId,
                        principalSchema: "Gyms",
                        principalTable: "GymProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CreatedAt",
                schema: "Gyms",
                table: "Reservations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GymProductId",
                schema: "Gyms",
                table: "Reservations",
                column: "GymProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GymProductId_ReservationDateTime_Status",
                schema: "Gyms",
                table: "Reservations",
                columns: new[] { "GymProductId", "ReservationDateTime", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationDateTime",
                schema: "Gyms",
                table: "Reservations",
                column: "ReservationDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                schema: "Gyms",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                schema: "Gyms",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_Status",
                schema: "Gyms",
                table: "Reservations",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations",
                schema: "Gyms");
        }
    }
}
