using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasksApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCalendarSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recurrences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CalendarTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RecurrenceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarTasks_Recurrences_RecurrenceId",
                        column: x => x.RecurrenceId,
                        principalTable: "Recurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Occurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CalendarTaskId = table.Column<int>(type: "int", nullable: false),
                    OriginalStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ModifiedEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCancelled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occurrences_CalendarTasks_CalendarTaskId",
                        column: x => x.CalendarTaskId,
                        principalTable: "CalendarTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarTasks_RecurrenceId",
                table: "CalendarTasks",
                column: "RecurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_CalendarTaskId_OriginalStartTime",
                table: "Occurrences",
                columns: new[] { "CalendarTaskId", "OriginalStartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Occurrences");

            migrationBuilder.DropTable(
                name: "CalendarTasks");

            migrationBuilder.DropTable(
                name: "Recurrences");
        }
    }
}
