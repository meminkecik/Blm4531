using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nearest.Migrations
{
    /// <inheritdoc />
    public partial class EnforceTowTruckPlateUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TowTrucks_CompanyId_LicensePlate",
                table: "TowTrucks");

            migrationBuilder.CreateIndex(
                name: "IX_TowTrucks_CompanyId",
                table: "TowTrucks",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TowTrucks_LicensePlate",
                table: "TowTrucks",
                column: "LicensePlate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TowTrucks_CompanyId",
                table: "TowTrucks");

            migrationBuilder.DropIndex(
                name: "IX_TowTrucks_LicensePlate",
                table: "TowTrucks");

            migrationBuilder.CreateIndex(
                name: "IX_TowTrucks_CompanyId_LicensePlate",
                table: "TowTrucks",
                columns: new[] { "CompanyId", "LicensePlate" },
                unique: true);
        }
    }
}
