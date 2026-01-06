using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nearest.Migrations
{
    public partial class AddReviewsAbuseReportsAndKvkk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // KVKK alanlarını Companies tablosuna ekle
            migrationBuilder.AddColumn<bool>(
                name: "KvkkConsent",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "KvkkConsentDate",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KvkkConsentVersion",
                table: "Companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KvkkConsentIpAddress",
                table: "Companies",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            // Reviews tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TowTruckId = table.Column<int>(type: "integer", nullable: false),
                    ReviewerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReviewerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_TowTrucks_TowTruckId",
                        column: x => x.TowTruckId,
                        principalTable: "TowTrucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TowTruckId",
                table: "Reviews",
                column: "TowTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerPhone",
                table: "Reviews",
                column: "ReviewerPhone");

            // AbuseReports tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "AbuseReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TowTruckId = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    ReportType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReporterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReporterPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReporterEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdminNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbuseReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbuseReports_TowTrucks_TowTruckId",
                        column: x => x.TowTruckId,
                        principalTable: "TowTrucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AbuseReports_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AbuseReports_Admins_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_Status",
                table: "AbuseReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_CreatedAt",
                table: "AbuseReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_TowTruckId",
                table: "AbuseReports",
                column: "TowTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_CompanyId",
                table: "AbuseReports",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_ReviewedByAdminId",
                table: "AbuseReports",
                column: "ReviewedByAdminId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AbuseReports");
            migrationBuilder.DropTable(name: "Reviews");
            
            migrationBuilder.DropColumn(name: "KvkkConsent", table: "Companies");
            migrationBuilder.DropColumn(name: "KvkkConsentDate", table: "Companies");
            migrationBuilder.DropColumn(name: "KvkkConsentVersion", table: "Companies");
            migrationBuilder.DropColumn(name: "KvkkConsentIpAddress", table: "Companies");
        }
    }
}
