using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmtlisBack.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelIdToSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Subscriptions");
        }
    }
}
