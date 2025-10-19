using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nearest.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false),
                    CityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CityDistricts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CityId = table.Column<string>(type: "text", nullable: false),
                    DistrictId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityDistricts_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CityDistricts_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_ProvinceId",
                table: "Cities",
                column: "ProvinceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityDistricts_CityId",
                table: "CityDistricts",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CityDistricts_DistrictId",
                table: "CityDistricts",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictId",
                table: "Districts",
                column: "DistrictId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CityDistricts");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}
