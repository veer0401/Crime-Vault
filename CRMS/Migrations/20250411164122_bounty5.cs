        using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.Migrations                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
{
    /// <inheritdoc />
    public partial class bounty5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CredibilityRating",
                table: "Witnesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "Victims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CompensationClaimed",
                table: "Victims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InjurySustained",
                table: "Victims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyDamage",
                table: "Victims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statement",
                table: "Victims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredibilityRating",
                table: "Witnesses");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Victims");

            migrationBuilder.DropColumn(
                name: "CompensationClaimed",
                table: "Victims");

            migrationBuilder.DropColumn(
                name: "InjurySustained",
                table: "Victims");

            migrationBuilder.DropColumn(
                name: "PropertyDamage",
                table: "Victims");

            migrationBuilder.DropColumn(
                name: "Statement",
                table: "Victims");
        }
    }
}
