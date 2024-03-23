using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace self_bot.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertyNamingDiscordMessageIdNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageOriginID",
                table: "Messages",
                newName: "MessageOriginId");

            migrationBuilder.RenameColumn(
                name: "DiscordMessageID",
                table: "Messages",
                newName: "DiscordMessageId");

            migrationBuilder.RenameColumn(
                name: "AuthorID",
                table: "Messages",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Messages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ServerID",
                table: "Messages",
                newName: "GuildId");

            migrationBuilder.AlterColumn<ulong>(
                name: "DiscordMessageId",
                table: "Messages",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageOriginId",
                table: "Messages",
                newName: "MessageOriginID");

            migrationBuilder.RenameColumn(
                name: "DiscordMessageId",
                table: "Messages",
                newName: "DiscordMessageID");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Messages",
                newName: "AuthorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Messages",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Messages",
                newName: "ServerID");

            migrationBuilder.AlterColumn<ulong>(
                name: "DiscordMessageID",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
