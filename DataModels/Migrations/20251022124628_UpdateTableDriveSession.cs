using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableDriveSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_DriveSessions_DriveSessionId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DriveSessionId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DriveSessionId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "EnrolledStudentsCount",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DriveSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DriveSessionCourses",
                columns: table => new
                {
                    CoursesId = table.Column<int>(type: "int", nullable: false),
                    DriveSessionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveSessionCourses", x => new { x.CoursesId, x.DriveSessionsId });
                    table.ForeignKey(
                        name: "FK_DriveSessionCourses_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriveSessionCourses_DriveSessions_DriveSessionsId",
                        column: x => x.DriveSessionsId,
                        principalTable: "DriveSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriveSessionCourses_DriveSessionsId",
                table: "DriveSessionCourses",
                column: "DriveSessionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriveSessionCourses");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DriveSessions");

            migrationBuilder.AddColumn<int>(
                name: "DriveSessionId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnrolledStudentsCount",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DriveSessionId",
                table: "Courses",
                column: "DriveSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_DriveSessions_DriveSessionId",
                table: "Courses",
                column: "DriveSessionId",
                principalTable: "DriveSessions",
                principalColumn: "Id");
        }
    }
}
