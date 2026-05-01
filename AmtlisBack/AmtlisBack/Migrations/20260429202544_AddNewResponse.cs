using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmtlisBack.Migrations
{
    /// <inheritdoc />
    public partial class AddNewResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                table: "WatchHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LikesCount",
                table: "WatchHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ViewsCount",
                table: "WatchHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "WatchHistories");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "WatchHistories");

            migrationBuilder.DropColumn(
                name: "ViewsCount",
                table: "WatchHistories");
        }
    }
}
