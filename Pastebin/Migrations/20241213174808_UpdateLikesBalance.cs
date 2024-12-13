using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pastebin.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLikesBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikesBalance",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikesBalance",
                table: "Users");
        }
    }
}
