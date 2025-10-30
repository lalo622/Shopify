using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopify.Migrations
{
    public partial class UserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Albums_AlbumId1",
                table: "Songs");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Artists_ArtistId1",
                table: "Songs");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Genres_GenreId1",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_AlbumId1",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_ArtistId1",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_GenreId1",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "AlbumId1",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "ArtistId1",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "GenreId1",
                table: "Songs");

            migrationBuilder.AlterColumn<string>(
                name: "AudioUrl",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Songs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "ArtistId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "AudioUrl",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlbumId1",
                table: "Songs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtistId1",
                table: "Songs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenreId1",
                table: "Songs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumId1",
                table: "Songs",
                column: "AlbumId1");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ArtistId1",
                table: "Songs",
                column: "ArtistId1");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_GenreId1",
                table: "Songs",
                column: "GenreId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Artists_ArtistId",
                table: "Albums",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "ArtistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Albums_AlbumId1",
                table: "Songs",
                column: "AlbumId1",
                principalTable: "Albums",
                principalColumn: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Artists_ArtistId1",
                table: "Songs",
                column: "ArtistId1",
                principalTable: "Artists",
                principalColumn: "ArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Genres_GenreId1",
                table: "Songs",
                column: "GenreId1",
                principalTable: "Genres",
                principalColumn: "GenreId");
        }
    }
}
