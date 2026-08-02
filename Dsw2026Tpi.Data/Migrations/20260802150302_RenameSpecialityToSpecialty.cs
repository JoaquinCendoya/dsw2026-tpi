using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameSpecialityToSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors");

            migrationBuilder.RenameTable(
                name: "Specialities",
                newName: "Specialties");

            migrationBuilder.RenameColumn(
                name: "SpecialityId",
                table: "Doctors",
                newName: "SpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Doctors_SpecialityId",
                table: "Doctors",
                newName: "IX_Doctors_SpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Specialities_Name",
                table: "Specialties",
                newName: "IX_Specialties_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialties_SpecialtyId",
                table: "Doctors",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialties_SpecialtyId",
                table: "Doctors");

            migrationBuilder.RenameIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                newName: "IX_Specialities_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Doctors_SpecialtyId",
                table: "Doctors",
                newName: "IX_Doctors_SpecialityId");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "Doctors",
                newName: "SpecialityId");

            migrationBuilder.RenameTable(
                name: "Specialties",
                newName: "Specialities");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialities_SpecialityId",
                table: "Doctors",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
