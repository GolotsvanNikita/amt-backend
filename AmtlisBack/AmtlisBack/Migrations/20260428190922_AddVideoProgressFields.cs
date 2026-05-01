using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmtlisBack.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoProgressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "WatchHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "WatchHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastPositionSeconds",
                table: "WatchHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "WatchHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "WatchHistories");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "WatchHistories");

            migrationBuilder.DropColumn(
                name: "LastPositionSeconds",
                table: "WatchHistories");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "WatchHistories");
        }
    }
}
