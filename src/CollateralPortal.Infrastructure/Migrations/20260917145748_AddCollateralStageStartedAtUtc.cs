using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollateralPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollateralStageStartedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StageStartedAtUtc",
                table: "CollateralRegister",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(
                    1, 1, 1,
                    0, 0, 0, 0,
                    DateTimeKind.Unspecified));

            // Existing collateral records were already in their
            // current stage before StageStartedAtUtc was introduced.
            // Use CreatedAtUtc as the initial TAT starting point.
            migrationBuilder.Sql("""
                UPDATE CollateralRegister
                SET StageStartedAtUtc = CreatedAtUtc
                WHERE StageStartedAtUtc = '0001-01-01T00:00:00.0000000'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StageStartedAtUtc",
                table: "CollateralRegister");
        }
    }
}