using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjusteConcurrenciaTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailabilitySlots_AvailabilityRules_AvailabilityRuleId",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_AvailabilityRuleId_SlotDate_StartTime",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AvailabilitySlots",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_AvailabilityRuleId_SlotDate_StartTime",
                table: "AvailabilitySlots",
                columns: new[] { "AvailabilityRuleId", "SlotDate", "StartTime" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[Status] = 'Booked' AND [Deleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AvailabilitySlots_AvailabilityRules_AvailabilityRuleId",
                table: "AvailabilitySlots",
                column: "AvailabilityRuleId",
                principalTable: "AvailabilityRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailabilitySlots_AvailabilityRules_AvailabilityRuleId",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_AvailabilityRuleId_SlotDate_StartTime",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_AvailabilityRuleId_SlotDate_StartTime",
                table: "AvailabilitySlots",
                columns: new[] { "AvailabilityRuleId", "SlotDate", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AvailabilitySlots_AvailabilityRules_AvailabilityRuleId",
                table: "AvailabilitySlots",
                column: "AvailabilityRuleId",
                principalTable: "AvailabilityRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
