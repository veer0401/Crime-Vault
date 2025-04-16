using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS.Migrations
{
    /// <inheritdoc />
    public partial class witness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseCriminals_Cases_CaseId",
                table: "CaseCriminals");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseCriminals_Criminal_CriminalId",
                table: "CaseCriminals");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseTeams_Cases_CaseId",
                table: "CaseTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseTeams_Teams_TeamId",
                table: "CaseTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_Victims_Cases_CaseId",
                table: "Victims");

            migrationBuilder.DropForeignKey(
                name: "FK_Witnesses_Cases_CaseId",
                table: "Witnesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Witnesses",
                table: "Witnesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Victims",
                table: "Victims");

            migrationBuilder.RenameTable(
                name: "Witnesses",
                newName: "Witness");

            migrationBuilder.RenameTable(
                name: "Victims",
                newName: "Victim");

            migrationBuilder.RenameIndex(
                name: "IX_Witnesses_CaseId",
                table: "Witness",
                newName: "IX_Witness_CaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Victims_CaseId",
                table: "Victim",
                newName: "IX_Victim_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Witness",
                table: "Witness",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Victim",
                table: "Victim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseCriminals_Cases_CaseId",
                table: "CaseCriminals",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseCriminals_Criminal_CriminalId",
                table: "CaseCriminals",
                column: "CriminalId",
                principalTable: "Criminal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseTeams_Cases_CaseId",
                table: "CaseTeams",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseTeams_Teams_TeamId",
                table: "CaseTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Victim_Cases_CaseId",
                table: "Victim",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Witness_Cases_CaseId",
                table: "Witness",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseCriminals_Cases_CaseId",
                table: "CaseCriminals");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseCriminals_Criminal_CriminalId",
                table: "CaseCriminals");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseTeams_Cases_CaseId",
                table: "CaseTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseTeams_Teams_TeamId",
                table: "CaseTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_Victim_Cases_CaseId",
                table: "Victim");

            migrationBuilder.DropForeignKey(
                name: "FK_Witness_Cases_CaseId",
                table: "Witness");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Witness",
                table: "Witness");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Victim",
                table: "Victim");

            migrationBuilder.RenameTable(
                name: "Witness",
                newName: "Witnesses");

            migrationBuilder.RenameTable(
                name: "Victim",
                newName: "Victims");

            migrationBuilder.RenameIndex(
                name: "IX_Witness_CaseId",
                table: "Witnesses",
                newName: "IX_Witnesses_CaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Victim_CaseId",
                table: "Victims",
                newName: "IX_Victims_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Witnesses",
                table: "Witnesses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Victims",
                table: "Victims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseCriminals_Cases_CaseId",
                table: "CaseCriminals",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseCriminals_Criminal_CriminalId",
                table: "CaseCriminals",
                column: "CriminalId",
                principalTable: "Criminal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseTeams_Cases_CaseId",
                table: "CaseTeams",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseTeams_Teams_TeamId",
                table: "CaseTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
    }
}
