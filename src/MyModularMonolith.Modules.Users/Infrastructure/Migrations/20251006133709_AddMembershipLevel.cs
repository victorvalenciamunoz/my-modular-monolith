using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyModularMonolith.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipLevel",
                schema: "Users",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MembershipLevel",
                schema: "Users",
                table: "AspNetUsers");
        }
    }
}
