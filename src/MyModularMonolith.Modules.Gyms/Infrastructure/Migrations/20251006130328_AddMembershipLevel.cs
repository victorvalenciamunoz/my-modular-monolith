using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumRequiredMembership",
                schema: "Gyms",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumRequiredMembership",
                schema: "Gyms",
                table: "Products");
        }
    }
}
