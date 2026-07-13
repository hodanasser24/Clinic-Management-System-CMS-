using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDoctorNoteAndPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorNotes_DoctorId",
                table: "DoctorNotes");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorNotes_PatientId",
                table: "DoctorNotes",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorNotes_PatientId",
                table: "DoctorNotes");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorNotes_DoctorId",
                table: "DoctorNotes",
                column: "DoctorId");
        }
    }
}
