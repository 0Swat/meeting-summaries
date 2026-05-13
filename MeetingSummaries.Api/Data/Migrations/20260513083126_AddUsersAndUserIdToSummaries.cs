using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingSummaries.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndUserIdToSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeetingSummaries_Type_Date",
                table: "MeetingSummaries");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "MeetingSummaries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSummaries_UserId_Type_Date",
                table: "MeetingSummaries",
                columns: new[] { "UserId", "Type", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingSummaries_Users_UserId",
                table: "MeetingSummaries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingSummaries_Users_UserId",
                table: "MeetingSummaries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_MeetingSummaries_UserId_Type_Date",
                table: "MeetingSummaries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MeetingSummaries");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSummaries_Type_Date",
                table: "MeetingSummaries",
                columns: new[] { "Type", "Date" },
                unique: true);
        }
    }
}
