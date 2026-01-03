using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableAttendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveSessionId = table.Column<int>(type: "int", nullable: false),
                    ZoomMeetingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceLogs");
        }
    }
}
