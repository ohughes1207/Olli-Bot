using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace self_bot.Migrations
{
    /// <inheritdoc />
    public partial class RequiredOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuoteOrigin",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "MessageOrigin",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageOrigin",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "QuoteOrigin",
                table: "Messages",
                type: "TEXT",
                nullable: true);
        }
    }
}
