using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace exam_api.Migrations
{
    /// <inheritdoc />
    public partial class GalleryDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Galleries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Galleries");
        }
    }
}
