using Microsoft.EntityFrameworkCore;

namespace Throb.Data.Entities
{
    public class LiveSession
    {
        public int Id { get; set; }
        public string Title { get; set; }  // عنوان الجلسة
        public DateTime Date { get; set; }  // تاريخ ووقت الجلسة (لإرسالها لـ Zoom)
        public int DurationMinutes { get; set; } // 💡 المدة المطلوبة بالدقائق

        public int CourseId { get; set; }  // ربط الجلسة بالكورس
        public Course? Course { get; set; }  // البيانات المتعلقة بالكورس

        // 🟢 بيانات Zoom API (التي يتم الحصول عليها بعد الإنشاء)
        public long? ZoomMeetingId { get; set; } // مُعرف الاجتماع في Zoom
        public string? StartUrl { get; set; } // رابط بدء المضيف/الأستاذ
        public string ?JoinUrl { get; set; }  // رابط انضمام الطالب
        public string ?Password { get; set; } // كلمة مرور الاجتماع (إذا وجدت)

    }
}