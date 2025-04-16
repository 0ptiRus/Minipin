using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace exam_api.Migrations
{
    /// <inheritdoc />
    public partial class CommentDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThumbnailFileId",
                table: "Files",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Files_ThumbnailFileId",
                table: "Files",
                column: "ThumbnailFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_ThumbnailFileId",
                table: "Files",
                column: "ThumbnailFileId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Files_ThumbnailFileId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ThumbnailFileId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ThumbnailFileId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Comments");
        }
    }
}
