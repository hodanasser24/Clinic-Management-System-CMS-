using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");
        }
    }
}
