using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using DCMS.Infrastructure.Data;

#nullable disable

namespace DCMS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260712110000_AddPatientToDoctorNotes")]
public partial class AddPatientToDoctorNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PatientId",
            table: "DoctorNotes",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_DoctorNotes_DoctorId_PatientId_CreatedAt",
            table: "DoctorNotes",
            columns: new[] { "DoctorId", "PatientId", "CreatedAt" });

        migrationBuilder.AddForeignKey(
            name: "FK_DoctorNotes_Users_PatientId",
            table: "DoctorNotes",
            column: "PatientId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_DoctorNotes_Users_PatientId", table: "DoctorNotes");
        migrationBuilder.DropIndex(name: "IX_DoctorNotes_DoctorId_PatientId_CreatedAt", table: "DoctorNotes");
        migrationBuilder.DropColumn(name: "PatientId", table: "DoctorNotes");
    }
}
