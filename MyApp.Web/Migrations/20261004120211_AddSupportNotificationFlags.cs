using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportNotificationFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminViewed",
                table: "SupportTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UserViewed",
                table: "SupportTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminViewed",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "UserViewed",
                table: "SupportTickets");
        }
    }
}
