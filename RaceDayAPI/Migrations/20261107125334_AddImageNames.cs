using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDayAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddImageNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureName",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerImageName",
                table: "Events",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannerImageName",
                table: "Events");
        }
    }
}
