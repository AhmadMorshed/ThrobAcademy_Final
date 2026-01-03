using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromCourseId : Migration
    {
        /// <inheritdoc />
        // داخل ملف الترحيل الجديد
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🟢 هذا السطر يزيل القيد الذي يمنع إنشاء جلسات متعددة لنفس الكورس
            migrationBuilder.DropIndex(
                name: "IX_LiveSessions_CourseId", // ⬅️ الاسم الذي يسبب التكرار
                table: "LiveSessions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 💡 يمكن ترك دالة Down فارغة أو وضع كود لإعادة إنشاء فهرس غير فريد
            // للحصول على أقصى قدر من التحكم (بدون فريد)، لا حاجة لإضافة شيء هنا.
        }
    }
}
