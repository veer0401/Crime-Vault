using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.Migrations
{
    /// <inheritdoc />
    public partial class evidencemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Cases_CaseId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Victims_Cases_CaseId",
                table: "Victims");

            migrationBuilder.DropForeignKey(
                name: "FK_Witnesses_Cases_CaseId",
                table: "Witnesses");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Evidence",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Evidence",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Cases_CaseId",
                table: "Evidence",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Victims_Cases_CaseId",
                table: "Victims",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Witnesses_Cases_CaseId",
                table: "Witnesses",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Cases_CaseId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Victims_Cases_CaseId",
                table: "Victims");

            migrationBuilder.DropForeignKey(
                name: "FK_Witnesses_Cases_CaseId",
                table: "Witnesses");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Evidence");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Cases_CaseId",
                table: "Evidence",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Victims_Cases_CaseId",
                table: "Victims",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Witnesses_Cases_CaseId",
                table: "Witnesses",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
