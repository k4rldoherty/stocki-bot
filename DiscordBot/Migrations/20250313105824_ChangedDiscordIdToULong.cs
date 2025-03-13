using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDiscordIdToULong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordUID",
                table: "StockNotificationSubscriptions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiscordUID",
                table: "StockNotificationSubscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
