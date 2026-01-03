using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditStudentIdToUserIdFromTableAttendanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Students_StudentId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ZoomParticipantId",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "ExamRequestModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExamType",
                table: "ExamRequestModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AttendanceRecords",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRequestModels_CourseId",
                table: "ExamRequestModels",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_UserId",
                table: "AttendanceRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_AspNetUsers_UserId",
                table: "AttendanceRecords",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRequestModels_Courses_CourseId",
                table: "ExamRequestModels",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_AspNetUsers_UserId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRequestModels_Courses_CourseId",
                table: "ExamRequestModels");

            migrationBuilder.DropIndex(
                name: "IX_ExamRequestModels_CourseId",
                table: "ExamRequestModels");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_UserId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "ExamRequestModels");

            migrationBuilder.DropColumn(
                name: "ExamType",
                table: "ExamRequestModels");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZoomParticipantId",
                table: "AttendanceRecords",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Students_StudentId",
                table: "AttendanceRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
