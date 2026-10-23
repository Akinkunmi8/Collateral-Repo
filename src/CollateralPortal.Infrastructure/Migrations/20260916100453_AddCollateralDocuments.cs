using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollateralPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollateralDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValuationReportReference",
                table: "CollateralValuations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValuationExpiryDate",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValuationDate",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpenMarketValue",
                table: "CollateralValuations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ForcedSaleValue",
                table: "CollateralValuations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAssigned",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Decision",
                table: "CollateralValuations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FacilityRef",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceValue",
                table: "CollateralValuations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landsize",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalGov",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManualEntry",
                table: "CollateralValuations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PropertyDescription",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredCoverageRatioPercent",
                table: "CollateralValuations",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SalesDate",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesLocation",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuationReportBase64",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuationReportName",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuersName",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "CollateralRegister",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BranchManagerName",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollateralAddress",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollateralOwner",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FacilityType",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipManagerName",
                table: "CollateralRegister",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CollateralValid",
                table: "CollateralPerfections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InvalidReason",
                table: "CollateralPerfections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OverdueNoticeSent",
                table: "CollateralInsurances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PremiumAmount",
                table: "CollateralInsurances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CollateralDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollateralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollateralDocuments_CollateralRegister_CollateralId",
                        column: x => x.CollateralId,
                        principalTable: "CollateralRegister",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollateralDocuments_CollateralId",
                table: "CollateralDocuments",
                column: "CollateralId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollateralDocuments");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "DateAssigned",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "Decision",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "FacilityRef",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "InsuranceValue",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "Landsize",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "LocalGov",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "ManualEntry",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "PropertyDescription",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "RequiredCoverageRatioPercent",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "SalesDate",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "SalesLocation",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "State",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "ValuationReportBase64",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "ValuationReportName",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "ValuersName",
                table: "CollateralValuations");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "BranchManagerName",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "BranchName",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "CollateralAddress",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "CollateralOwner",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "FacilityType",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "RelationshipManagerName",
                table: "CollateralRegister");

            migrationBuilder.DropColumn(
                name: "CollateralValid",
                table: "CollateralPerfections");

            migrationBuilder.DropColumn(
                name: "InvalidReason",
                table: "CollateralPerfections");

            migrationBuilder.DropColumn(
                name: "OverdueNoticeSent",
                table: "CollateralInsurances");

            migrationBuilder.DropColumn(
                name: "PremiumAmount",
                table: "CollateralInsurances");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValuationExpiryDate",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValuationDate",
                table: "CollateralValuations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpenMarketValue",
                table: "CollateralValuations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ForcedSaleValue",
                table: "CollateralValuations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuationReportReference",
                table: "CollateralValuations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
