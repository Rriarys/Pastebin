using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pastebin.Migrations
{
    /// <inheritdoc />
    public partial class AddPostTitleToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostTitle",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostTitle",
                table: "Posts");
        }
    }
}
