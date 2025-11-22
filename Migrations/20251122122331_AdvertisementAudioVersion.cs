using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopify.Migrations
{
    public partial class AdvertisementAudioVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Advertisements",
                newName: "AdvertiserLink");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Advertisements",
                newName: "PosterUrl");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Advertisements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Advertisements");

            migrationBuilder.RenameColumn(
                name: "PosterUrl",
                table: "Advertisements",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "AdvertiserLink",
                table: "Advertisements",
                newName: "Link");
        }
    }
}
