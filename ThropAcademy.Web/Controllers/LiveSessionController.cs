using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // لـ IConfiguration
using System;
using System.Linq;
using System.Threading.Tasks; // لـ async/await
using Throb.Data.Entities;
using Throb.Service.Interfaces;
using ThropAcademy.Web.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ThropAcademy.Web.Controllers
{
    public class LiveSessionController : Controller
    {
        private readonly ILiveSession _liveSessionService;
        private readonly ICourseService _courseService;
        private readonly IConfiguration _config;

        public LiveSessionController(ILiveSession liveSessionService, ICourseService courseService, IConfiguration config)
        {
            _liveSessionService = liveSessionService;
            _courseService = courseService;
            _config = config;
        }

        // 1. دالة عرض واجهة الإضافة (Create - GET)
        [HttpGet]
        public IActionResult Create() // 🟢 متزامنة (تستخدم GetAll)
        {
            ViewBag.Courses = _courseService.GetAll(); // ⬅️ نستخدم المتزامنة
            return View();
        }

        // 2. دالة معالجة إنشاء الاجتماع (Create - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Date,DurationMinutes,CourseId")] LiveSession session)
        {
            if (session.CourseId <= 0)
            {
                ModelState.AddModelError("CourseId", "الرجاء اختيار كورس صالح من القائمة.");
            }
            if (ModelState.ContainsKey("Course"))
            {
                ModelState.Remove("Course");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = _courseService.GetAll(); // ⬅️ نستخدم المتزامنة
                return View(session);
            }

            try
            {
                var masterEmail = _config["ZoomSettings:MasterUserId"];

                if (string.IsNullOrEmpty(masterEmail))
                {
                    TempData["ErrorMessage"] = "Master Zoom User ID is missing in appsettings.json.";
                    return RedirectToAction(nameof(Index));
                }

                var createdSession = await _liveSessionService.CreateZoomSessionAsync(session, masterEmail);

                TempData["SuccessMessage"] = $"تم إنشاء جلسة Zoom بنجاح: {createdSession.Title}. الرابط تم حفظه.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"خطأ في الاتصال بـ Zoom. تأكد من صحة المفاتيح والصلاحيات. التفاصيل: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء إنشاء الجلسة. {ex.Message}";
            }

            ViewBag.Courses = _courseService.GetAll(); // ⬅️ نستخدم المتزامنة
            return View(session);
        }

        // 3. دالة عرض الجلسات (Index)
        public async Task<IActionResult> Index()
        {
            var liveSessions = await _liveSessionService.GetAllAsync();
            var courses = _courseService.GetAll(); // ⬅️ نستخدم المتزامنة

            var model = new LiveSessionViewModel
            {
                LiveSessions = liveSessions,
                Courses = courses
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(model);
        }

        // 4. دالة عرض نموذج التعديل (Edit - GET)
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            // 1. جلب الجلسة من الخدمة
            var session = await _liveSessionService.GetByIdAsync(id);
            if (session == null) return NotFound();

            // 2. جلب الكورسات (استخدمنا GetAll لأن GetAllAsync غير موجودة لديك)
            ViewBag.Courses = _courseService.GetAll();

            return View(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(LiveSession session)
        {
            if (!ModelState.IsValid || session.CourseId <= 0)
            {
                ViewBag.Courses = _courseService.GetAll();
                return View(session);
            }

            try
            {
                // 1. جلب الكائن الأصلي (الذي يتم تتبعه الآن)
                var trackedSession = await _liveSessionService.GetByIdAsync(session.Id);

                if (trackedSession == null) return NotFound();

                // 2. تحديث القيم في الكائن "المُتتبع" حصراً
                // نحن نغير خصائص الكائن الأصلي بالقيم الجديدة من الفورم
                trackedSession.Title = session.Title;
                trackedSession.Date = session.Date;
                trackedSession.DurationMinutes = session.DurationMinutes;
                trackedSession.CourseId = session.CourseId;

                // ملاحظة: بيانات زووم (ID, Links) موجودة أصلاً في trackedSession 
                // ولن تضيع طالما لم نغيرها هنا.

                // 3. نرسل الكائن "المُتتبع" للدالة لكي يتم الحفظ
                await _liveSessionService.UpdateAsync(trackedSession);

                TempData["SuccessMessage"] = $"تم تحديث الجلسة '{trackedSession.Title}' بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء التحديث: {ex.Message}";
                ViewBag.Courses = _courseService.GetAll();
                return View(session);
            }
        }
        // 6. دالة معالجة الحذف (Delete - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _liveSessionService.DeleteAsync(new LiveSession { Id = id });
                TempData["SuccessMessage"] = "تم حذف الجلسة بنجاح من Zoom وقاعدة البيانات.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء محاولة حذف الجلسة: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")] // يجب أن يكون للمدراء فقط
        public async Task<IActionResult> ViewAttendance(int id)
        {
            var session = await _liveSessionService.GetByIdAsync(id);
            if (session == null)
            {
                TempData["ErrorMessage"] = "الجلسة غير موجودة.";
                return RedirectToAction(nameof(Index));
            }

           
            var attendanceRecords = await _liveSessionService.GetAttendanceRecordsBySessionIdAsync(id);

            var viewModel = new AttendanceReportViewModel
            {
                SessionTitle = session.Title,
                SessionDate = session.Date,
                SessionDuration = session.DurationMinutes,
                Records = attendanceRecords.ToList() // تحويل إلى قائمة لسهولة التعامل في View
            };

            return View(viewModel);
        }
        // 7. دالة جلب وحفظ سجل الحضور (RecordAttendance - POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // لزيادة الأمان ومنع هجمات CSRF
        public async Task<IActionResult> RecordAttendance(int sessionId)
        {
            if (sessionId <= 0)
            {
                TempData["ErrorMessage"] = "معرف الجلسة غير صالح.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // 1. التحقق من وجود الجلسة قبل البدء
                var session = await _liveSessionService.GetByIdAsync(sessionId);
                if (session == null)
                {
                    TempData["ErrorMessage"] = "الجلسة المطلوبة غير موجودة في قاعدة البيانات.";
                    return RedirectToAction(nameof(Index));
                }

                if (session.ZoomMeetingId == null)
                {
                    TempData["ErrorMessage"] = "هذه الجلسة لا تملك معرف Zoom صالح (ربما لم تُنشأ عبر النظام بشكل صحيح).";
                    return RedirectToAction(nameof(Index));
                }

                // 2. استدعاء الخدمة لجلب البيانات من Zoom والمطابقة مع الطلاب
                // ملاحظة: تأكد أن دالة RecordAttendanceAsync داخل الـ Service تقوم بـ SaveChangesAsync
                var recordsCount = await _liveSessionService.RecordAttendanceAsync(sessionId);

                if (recordsCount > 0)
                {
                    TempData["SuccessMessage"] = $"نجاح! تم جلب وتحديث سجل حضور ({recordsCount}) طالب/ـاً.";

                    // تحسين: التوجه لصفحة عرض التقرير لرؤية النتائج فوراً
                    return RedirectToAction(nameof(ViewAttendance), new { id = sessionId });
                }
                else
                {
                    // رسالة أكثر وضوحاً للمستخدم
                    TempData["ErrorMessage"] = "لم يتم العثور على سجلات حضور جديدة. الأسباب المحتملة: (1) الاجتماع لم ينتهِ بعد. (2) الطلاب لم يدخلوا بإيميلاتهم المسجلة. (3) التقرير قيد المعالجة في Zoom (انتظر 5 دقائق).";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                // التعامل مع أخطاء الاتصال أو الصلاحيات (Scopes)
                TempData["ErrorMessage"] = $"خطأ في الاتصال بـ Zoom API: تأكد من صلاحية (report:read:admin). التفاصيل: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // التعامل مع أي خطأ غير متوقع
                TempData["ErrorMessage"] = $"حدث خطأ فني أثناء معالجة التقرير: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}