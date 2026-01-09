using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseNavigationToResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserExamResults_CourseId",
                table: "UserExamResults",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserExamResults_Courses_CourseId",
                table: "UserExamResults",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExamResults_Courses_CourseId",
                table: "UserExamResults");

            migrationBuilder.DropIndex(
                name: "IX_UserExamResults_CourseId",
                table: "UserExamResults");
        }
    }
}
