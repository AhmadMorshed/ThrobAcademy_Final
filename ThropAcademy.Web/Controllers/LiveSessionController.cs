using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; 
using System;
using System.Linq;
using System.Threading.Tasks;
using Throb.Data.Entities;
using Throb.Service.Interfaces;
using ThropAcademy.Web.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Throb.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ThropAcademy.Web.Controllers
{
    public class LiveSessionController : Controller
    {
        private readonly ILiveSession _liveSessionService;
        private readonly ICourseService _courseService;
        private readonly IConfiguration _config;
        private readonly ThrobDbContext _context;

        public LiveSessionController(ILiveSession liveSessionService, ICourseService courseService, IConfiguration config,ThrobDbContext context)
        {
            _liveSessionService = liveSessionService;
            _courseService = courseService;
            _config = config;
            _context = context;
        }

        [Authorize(Roles = "Admin,Instructor")]


        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserName = User.Identity.Name;

            if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
            {
                // جلب معرفات الكورسات التي يشرف عليها هذا المدرب فقط بناءً على اسمه
                var instructorCourseIds = await _context.InstructorCourses
                    .Include(ic => ic.Instructor)
                    .Where(ic => ic.Instructor.Name == currentUserName)
                    .Select(ic => ic.CourseId)
                    .ToListAsync();

                // وضع الكورسات المفلترة فقط في ViewBag
                ViewBag.Courses = await _context.Courses
                    .Where(c => instructorCourseIds.Contains(c.Id))
                    .ToListAsync();
            }
            else
            {
                // إذا كان مسؤولاً (Admin)، تظهر له كل الكورسات
                ViewBag.Courses = _courseService.GetAll();
            }

            return View();
        }
        [Authorize(Roles = "Admin,Instructor")]

        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Date,DurationMinutes,CourseId")] LiveSession session)
        {
            var currentUserName = User.Identity.Name;

            // 1. التحقق من صحة اختيار الكورس
            if (session.CourseId <= 0)
            {
                ModelState.AddModelError("CourseId", "الرجاء اختيار كورس صالح من القائمة.");
            }

            // 2. حماية إضافية: التأكد أن المدرب يملك صلاحية على الكورس المختار
            if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
            {
                var isAuthorized = await _context.InstructorCourses
                    .Include(ic => ic.Instructor)
                    .AnyAsync(ic => ic.Instructor.Name == currentUserName && ic.CourseId == session.CourseId);

                if (!isAuthorized)
                {
                    ModelState.AddModelError("CourseId", "ليس لديك صلاحية لإنشاء جلسة في هذا الكورس.");
                }
            }

            // تنظيف ModelState من أي كائنات مرتبطة تلقائياً
            if (ModelState.ContainsKey("Course"))
            {
                ModelState.Remove("Course");
            }

            // 3. في حال وجود خطأ في البيانات، أعد تحميل الكورسات المفلترة فقط
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
                {
                    var instructorCourseIds = await _context.InstructorCourses
                        .Include(ic => ic.Instructor)
                        .Where(ic => ic.Instructor.Name == currentUserName)
                        .Select(ic => ic.CourseId)
                        .ToListAsync();

                    ViewBag.Courses = await _context.Courses
                        .Where(c => instructorCourseIds.Contains(c.Id))
                        .ToListAsync();
                }
                else
                {
                    ViewBag.Courses = _courseService.GetAll();
                }
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

                // إنشاء الجلسة عبر الخدمة المخصصة (Zoom API)
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

            // إعادة تعبئة القائمة في حال فشل الاتصال بـ Zoom
            if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
            {
                var instructorCourseIds = await _context.InstructorCourses
                    .Include(ic => ic.Instructor)
                    .Where(ic => ic.Instructor.Name == currentUserName)
                    .Select(ic => ic.CourseId)
                    .ToListAsync();

                ViewBag.Courses = await _context.Courses
                    .Where(c => instructorCourseIds.Contains(c.Id))
                    .ToListAsync();
            }
            else
            {
                ViewBag.Courses = _courseService.GetAll();
            }

            return View(session);
        }
        public async Task<IActionResult> Index()
        {
            var currentUserName = User.Identity.Name;
            IEnumerable<LiveSession> liveSessions;
            IEnumerable<Course> courses;

            if (User.IsInRole("Instructor"))
            {
                // 1. جلب الكورسات التي يشرف عليها المدرب
                var instructorCourseIds = await _context.InstructorCourses
                    .Include(ic => ic.Instructor)
                    .Where(ic => ic.Instructor.Name == currentUserName)
                    .Select(ic => ic.CourseId)
                    .ToListAsync();

                courses = await _context.Courses.Where(c => instructorCourseIds.Contains(c.Id)).ToListAsync();
                liveSessions = await _liveSessionService.GetAllAsync();
                liveSessions = liveSessions.Where(ls => instructorCourseIds.Contains(ls.CourseId)).ToList();
            }
            else if (User.IsInRole("Student"))
            {
                // 2. جلب الكورسات التي سجل فيها الطالب
                var studentCourseIds = await _context.StudentCourses
                    .Include(sc => sc.Student)
                    .Where(sc => sc.Student.Name == currentUserName)
                    .Select(sc => sc.CourseId)
                    .ToListAsync();

                courses = await _context.Courses.Where(c => studentCourseIds.Contains(c.Id)).ToListAsync();
                liveSessions = await _liveSessionService.GetAllAsync();
                liveSessions = liveSessions.Where(ls => studentCourseIds.Contains(ls.CourseId)).ToList();
            }
            else // Admin
            {
                liveSessions = await _liveSessionService.GetAllAsync();
                courses = _courseService.GetAll();
            }

            var model = new LiveSessionViewModel
            {
                LiveSessions = liveSessions,
                Courses = courses
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(model);
        }
        [Authorize(Roles = "Admin,Instructor")]

        
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            
            var session = await _liveSessionService.GetByIdAsync(id);
            if (session == null) return NotFound();

            
            ViewBag.Courses = _courseService.GetAll();

            return View(session);
        }
        [Authorize(Roles = "Admin,Instructor")]

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
             
                var trackedSession = await _liveSessionService.GetByIdAsync(session.Id);

                if (trackedSession == null) return NotFound();

               
                trackedSession.Title = session.Title;
                trackedSession.Date = session.Date;
                trackedSession.DurationMinutes = session.DurationMinutes;
                trackedSession.CourseId = session.CourseId;


             
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

       
        [Authorize(Roles = "Admin,Instructor")]

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
        [Authorize(Roles = "Admin")] 
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
                Records = attendanceRecords.ToList() 
            };

            return View(viewModel);
        }

        
        [Authorize(Roles = "Admin,Instructor")]

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> RecordAttendance(int sessionId)
        {
            if (sessionId <= 0)
            {
                TempData["ErrorMessage"] = "معرف الجلسة غير صالح.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                
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

            
                var recordsCount = await _liveSessionService.RecordAttendanceAsync(sessionId);

                if (recordsCount > 0)
                {
                    TempData["SuccessMessage"] = $"نجاح! تم جلب وتحديث سجل حضور ({recordsCount}) طالب/ـاً.";

                    
                    return RedirectToAction(nameof(ViewAttendance), new { id = sessionId });
                }
                else
                {
                    
                    TempData["ErrorMessage"] = "لم يتم العثور على سجلات حضور جديدة. الأسباب المحتملة: (1) الاجتماع لم ينتهِ بعد. (2) الطلاب لم يدخلوا بإيميلاتهم المسجلة. (3) التقرير قيد المعالجة في Zoom (انتظر 5 دقائق).";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                
                TempData["ErrorMessage"] = $"خطأ في الاتصال بـ Zoom API: تأكد من صلاحية (report:read:admin). التفاصيل: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
               
                TempData["ErrorMessage"] = $"حدث خطأ فني أثناء معالجة التقرير: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}