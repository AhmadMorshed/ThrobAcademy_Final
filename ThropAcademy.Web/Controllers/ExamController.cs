using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class ExamController : Controller
    {
        private readonly IExamRequestService _examService;
        private readonly ThrobDbContext _context;
        private readonly IExamRequestRepository _examRequestRepository;

        public ExamController(IExamRequestService examService, ThrobDbContext context, IExamRequestRepository examRequestRepository)
        {
            _examService = examService;
            _context = context;
            _examRequestRepository = examRequestRepository;
        }

        // 1. عرض قائمة الاختبارات المتاحة مع التحقق من الحالات المكتملة
        [Authorize]
        public async Task<IActionResult> AvailableExams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب كل الاختبارات المتاحة
            var exams = await _examService.GetAllAsync();

            // جلب معرفات الاختبارات التي أتمها هذا الطالب لتظليلها أو قفلها في الواجهة
            var completedExamIds = await _context.UserExamResults
                .Where(r => r.UserId == userId)
                .Select(r => r.ExamRequestId)
                .ToListAsync();

            ViewBag.CompletedExamIds = completedExamIds;

            return View(exams);
        }

        // 2. دالة معاينة وبدء الاختبار (المدمجة والنهائية)
        [Authorize]
        public async Task<IActionResult> ExamPreview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // التحقق من المنع أولاً
            var isCompleted = await _context.UserExamResults
                .AnyAsync(r => r.UserId == userId && r.ExamRequestId == id);

            if (isCompleted)
            {
                TempData["ErrorMessage"] = "عذراً، لقد قمت بتقديم هذا الاختبار مسبقاً.";
                return RedirectToAction("AvailableExams");
            }

            // جلب البيانات مع التأكد من جلب Options [هنا الحل الجذري]
            var exam = await _context.ExamRequestModels
                .Include(er => er.Course)
                .Include(er => er.ExamRequestQuestions)
                    .ThenInclude(erq => erq.Question)
                        .ThenInclude(q => q.Options) // <--- إضافة هذا السطر لتحميل الخيارات
                .FirstOrDefaultAsync(er => er.ExamRequestId == id);

            if (exam == null) return NotFound();

            return View("~/Views/Transcription/ExamPreview.cshtml", exam);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SubmitExam(ExamSubmissionViewModel model)
        //{
        //    if (model == null || model.Answers == null) return RedirectToAction("AvailableExams");

        //    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    int score = 0;
        //    int totalQuestions = model.Answers.Count;

        //    // 1. حساب النتيجة بمقارنة الإجابات مع قاعدة البيانات
        //    foreach (var submittedAnswer in model.Answers)
        //    {
        //        var question = await _context.Questions
        //            .FirstOrDefaultAsync(q => q.QuestionId == submittedAnswer.QuestionId);

        //        if (question != null)
        //        {
        //            // التحقق من الإجابة (سواء كانت MCQ أو True/False)
        //            if (question.CorrectAnswer.Trim().Equals(submittedAnswer.SelectedOption?.Trim(), StringComparison.OrdinalIgnoreCase))
        //            {
        //                score++;
        //            }
        //        }
        //    }

        //    // 2. حساب النسبة المئوية
        //    double percentage = totalQuestions > 0 ? (double)score / totalQuestions * 100 : 0;
        //    percentage = Math.Round(percentage, 1);

        //    // 3. حفظ النتيجة في قاعدة البيانات (لمنع الدخول مرة أخرى)
        //    var result = new UserExamResult
        //    {
        //        UserId = currentUserId,
        //        ExamRequestId = model.ExamRequestId,
        //        CourseId = model.CourseId,
        //        CorrectAnswers = score,
        //        TotalQuestions = totalQuestions,

        //    };

        //    _context.UserExamResults.Add(result);
        //    await _context.SaveChangesAsync();

        //    // 4. تمرير البيانات لصفحة النتيجة عبر ViewBag
        //    ViewBag.Score = score;
        //    ViewBag.Total = totalQuestions;
        //    ViewBag.Percentage = percentage;

        //    // جلب موديل الاختبار لعرضه في صفحة النتيجة إذا لزم الأمر
        //    var examModel = await _context.ExamRequestModels.FindAsync(model.ExamRequestId);

        //    return View("ExamResult", examModel);
        //}
    }
}