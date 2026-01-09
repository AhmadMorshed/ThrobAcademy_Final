using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamModelsAndJunctionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRequestQuestions_ExamRequestModels_ExamRequestsExamRequestId",
                table: "ExamRequestQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRequestQuestions_Questions_QuestionsQuestionId",
                table: "ExamRequestQuestions");

            migrationBuilder.RenameColumn(
                name: "QuestionsQuestionId",
                table: "ExamRequestQuestions",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "ExamRequestsExamRequestId",
                table: "ExamRequestQuestions",
                newName: "ExamRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamRequestQuestions_QuestionsQuestionId",
                table: "ExamRequestQuestions",
                newName: "IX_ExamRequestQuestions_QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRequestQuestions_ExamRequestModels_ExamRequestId",
                table: "ExamRequestQuestions",
                column: "ExamRequestId",
                principalTable: "ExamRequestModels",
                principalColumn: "ExamRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRequestQuestions_Questions_QuestionId",
                table: "ExamRequestQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRequestQuestions_ExamRequestModels_ExamRequestId",
                table: "ExamRequestQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRequestQuestions_Questions_QuestionId",
                table: "ExamRequestQuestions");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "ExamRequestQuestions",
                newName: "QuestionsQuestionId");

            migrationBuilder.RenameColumn(
                name: "ExamRequestId",
                table: "ExamRequestQuestions",
                newName: "ExamRequestsExamRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamRequestQuestions_QuestionId",
                table: "ExamRequestQuestions",
                newName: "IX_ExamRequestQuestions_QuestionsQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRequestQuestions_ExamRequestModels_ExamRequestsExamRequestId",
                table: "ExamRequestQuestions",
                column: "ExamRequestsExamRequestId",
                principalTable: "ExamRequestModels",
                principalColumn: "ExamRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRequestQuestions_Questions_QuestionsQuestionId",
                table: "ExamRequestQuestions",
                column: "QuestionsQuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
