using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmtlisBack.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "WatchHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "WatchHistories");
        }
    }
}
