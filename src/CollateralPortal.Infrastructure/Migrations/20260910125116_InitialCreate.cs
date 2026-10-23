using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollateralPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollateralRegister",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanContractId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Region = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApprovedLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiredCoverageRatioPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralRegister", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollateralInsurances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Insurer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolicyStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PolicyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RenewalCadence = table.Column<int>(type: "int", nullable: false),
                    RenewalReminderSent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralInsurances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollateralInsurances_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollateralPerfections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    QueryNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralPerfections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollateralPerfections_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollateralReleases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanFullyRepaid = table.Column<bool>(type: "bit", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollateralReleases_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollateralValuations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValuationReportReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenMarketValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForcedSaleValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValuationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValuationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualCoverageRatioPercent = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    CoverageAdequate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralValuations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollateralValuations_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CollateralId",
                table: "AuditLogs",
                column: "CollateralId");

            migrationBuilder.CreateIndex(
                name: "IX_CollateralInsurances_CollateralId",
                table: "CollateralInsurances",
                column: "CollateralId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollateralPerfections_CollateralId",
                table: "CollateralPerfections",
                column: "CollateralId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollateralRegister_LoanContractId",
                table: "CollateralRegister",
                column: "LoanContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CollateralReleases_CollateralId",
                table: "CollateralReleases",
                column: "CollateralId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollateralValuations_CollateralId",
                table: "CollateralValuations",
                column: "CollateralId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CollateralInsurances");

            migrationBuilder.DropTable(
                name: "CollateralPerfections");

            migrationBuilder.DropTable(
                name: "CollateralReleases");

            migrationBuilder.DropTable(
                name: "CollateralValuations");

            migrationBuilder.DropTable(
                name: "CollateralRegister");
        }
    }
}
