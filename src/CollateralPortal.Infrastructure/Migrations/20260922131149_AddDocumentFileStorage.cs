using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollateralPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Content",
                table: "CollateralDocuments",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "CollateralDocuments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelativePath",
                table: "CollateralDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "CollateralDocuments");

            migrationBuilder.DropColumn(
                name: "RelativePath",
                table: "CollateralDocuments");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Content",
                table: "CollateralDocuments",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
