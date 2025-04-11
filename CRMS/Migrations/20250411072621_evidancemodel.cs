using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.Migrations
{
    /// <inheritdoc />
    public partial class evidancemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectedBy",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Evidence");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Evidence",
                newName: "FilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Evidence",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "CollectedBy",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
