using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableInstructorCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourse_Courses_CourseId",
                table: "InstructorCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourse_Instructors_InstructorId",
                table: "InstructorCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstructorCourse",
                table: "InstructorCourse");

            migrationBuilder.RenameTable(
                name: "InstructorCourse",
                newName: "InstructorCourses");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorCourse_CourseId",
                table: "InstructorCourses",
                newName: "IX_InstructorCourses_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstructorCourses",
                table: "InstructorCourses",
                columns: new[] { "InstructorId", "CourseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_Instructors_InstructorId",
                table: "InstructorCourses",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_Instructors_InstructorId",
                table: "InstructorCourses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstructorCourses",
                table: "InstructorCourses");

            migrationBuilder.RenameTable(
                name: "InstructorCourses",
                newName: "InstructorCourse");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorCourses_CourseId",
                table: "InstructorCourse",
                newName: "IX_InstructorCourse_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstructorCourse",
                table: "InstructorCourse",
                columns: new[] { "InstructorId", "CourseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourse_Courses_CourseId",
                table: "InstructorCourse",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourse_Instructors_InstructorId",
                table: "InstructorCourse",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
