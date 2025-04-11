using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.Migrations
{
    /// <inheritdoc />
    public partial class suspectmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suspect",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageFilename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SuspectStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastKnownLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PossibleMotives = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipToVictim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alibi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSeenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPersonOfInterest = table.Column<bool>(type: "bit", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suspect", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suspect_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suspect_CaseId",
                table: "Suspect",
                column: "CaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Suspect");
        }
    }
}
