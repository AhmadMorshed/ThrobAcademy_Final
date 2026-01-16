using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Throb.Data.DbContext; // تأكد من استدعاء سياق البيانات الخاص بك

namespace ThropAcademy.Web.ViewComponents
{
    public class ExamNotificationBadgeViewComponent : ViewComponent
    {
        private readonly ThrobDbContext _context;

        public ExamNotificationBadgeViewComponent(ThrobDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUserName = UserClaimsPrincipal.Identity.Name;

            // البحث عن الطالب بالاسم
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Name == currentUserName);

            if (student == null) return View(0);

            // جلب كورسات الطالب
            var studentCourseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == student.Id)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            if (!studentCourseIds.Any()) return View(0);

            // استبعاد الامتحانات التي تم حلها
            var identityUserId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var completedExamIds = await _context.UserExamResults
                .Where(r => r.UserId == identityUserId)
                .Select(r => r.ExamRequestId)
                .ToListAsync();

            int count = await _context.ExamRequestModels
                .CountAsync(e => studentCourseIds.Contains(e.CourseId) && !completedExamIds.Contains(e.ExamRequestId));

            return View(count);
        }
    }
}   