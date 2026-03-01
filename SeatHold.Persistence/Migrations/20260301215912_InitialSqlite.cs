using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatHold.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SeatId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    HeldBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holds_SeatId",
                table: "Holds",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Holds_SeatId_ExpiresAtUtc",
                table: "Holds",
                columns: new[] { "SeatId", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holds");
        }
    }
}
