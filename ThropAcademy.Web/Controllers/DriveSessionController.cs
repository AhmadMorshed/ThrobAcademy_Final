using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Service.Interfaces;
using Throb.Service.Services; // تأكد أن InputModel موجود هنا
using ThropAcademy.Web.Models;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin,Instructor,Student")]
    public class DriveSessionController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IDriveSessionService _driveSessionService;
        private readonly ILogger<DriveSessionController> _logger;
        private readonly ThrobDbContext _context;

        public DriveSessionController(
            ICourseService courseService,
            IDriveSessionService driveSessionService,
            ILogger<DriveSessionController> logger,
            ThrobDbContext context)
        {
            _courseService = courseService;
            _driveSessionService = driveSessionService;
            _logger = logger;
            _context = context;
        }

        // --- 1. Index (عرض قائمة الكورسات) ---
        public IActionResult Index()
        {
            var courses = _courseService.GetAll();
            return View(courses);
        }

        // --- 2. View (عرض محتوى الكورس - فيديوهات ومستندات) ---
        [HttpGet]
        public async Task<IActionResult> View(int courseId)
        {
            // 1. جلب الفيديوهات
            var videos = await _driveSessionService.GetByCourseId(courseId);

            // 2. جلب المستندات
            var documents = await _context.LectureResources
                                          .Where(r => r.CourseId == courseId)
                                          .ToListAsync();

            // 3. بناء نموذج العرض الموحد
            var viewModel = new CourseContentViewModel
            {
                CourseId = courseId,
                Videos = videos,
                Documents = documents
            };

            return View(viewModel);
        }

        // ------------------------------------------------------------------
        //                          دوال رفع الفيديو
        // ------------------------------------------------------------------

        // --- 3. UploadVideo (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public IActionResult UploadVideo(int courseId)
        {
            var courses = _courseService.GetAll();
            var model = new UploadVideoViewModel
            {
                Courses = courses.ToList(),
                CourseIds = courses.Any(c => c.Id == courseId) ? new int[] { courseId } : new int[] { }
            };
            return View(model);
        }

        // --- 4. UploadVideo (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [RequestSizeLimit(209715200)] // 200 MB limit
        public async Task<IActionResult> UploadVideo(UploadVideoInputModel inputModel)
        {
            if (inputModel == null)
            {
                var coursesOnFailure = _courseService.GetAll() ?? Enumerable.Empty<Course>();
                ModelState.AddModelError(string.Empty, "حدث خطأ في استقبال البيانات.");
                return View(new UploadVideoViewModel { Courses = coursesOnFailure.ToList() });
            }

            var model = new UploadVideoViewModel
            {
                Title = inputModel.Title,
                VideoFile = inputModel.VideoFile,
                CourseIds = inputModel.CourseIds,
                Courses = _courseService.GetAll()?.ToList() ?? new List<Course>()
            };

            if (!ModelState.IsValid || model.VideoFile == null || model.VideoFile.Length == 0)
            {
                if (model.VideoFile == null) ModelState.AddModelError("", "الرجاء رفع ملف فيديو.");
                return View(model);
            }

            if (!model.VideoFile.ContentType.StartsWith("video/"))
            {
                ModelState.AddModelError("", "يُسمح برفع ملفات الفيديو فقط.");
                return View(model);
            }

            try
            {
                await _driveSessionService.AddAsync(model.VideoFile, model.CourseIds, model.Title);
                TempData["Success"] = $"تم رفع الفيديو '{model.Title}' بنجاح.";
                var firstCourseId = model.CourseIds.FirstOrDefault();
                return RedirectToAction("View", new { courseId = firstCourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video: {Title}", model.Title);
                ModelState.AddModelError("", $"خطأ أثناء رفع الفيديو: {ex.Message}");
                return View(model);
            }
        }

        // ------------------------------------------------------------------
        //                          دوال رفع المستندات
        // ------------------------------------------------------------------

        // --- 5. UploadDocument (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public IActionResult UploadDocument(int courseId)
        {
            var courses = _courseService.GetAll();
            var model = new UploadDocumentViewModel
            {
                Courses = courses,
                CourseId = courseId > 0 && courses.Any(c => c.Id == courseId) ? courseId : 0
            };
            return View(model);
        }

        // --- 6. UploadDocument (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UploadDocument(UploadDocumentViewModel model)
        {
            // 1. التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                model.Courses = _courseService.GetAll() ?? Enumerable.Empty<Course>();
                TempData["Error"] = "الرجاء ملء جميع الحقول المطلوبة.";
                // 🟢 هنا الإصلاح المهم: نعود لصفحة UploadDocument وليس UploadVideo
                return View("UploadDocument", model);
            }

            var file = model.DocumentFile;
            var allowedExtensions = new[] { ".pdf", ".docx", ".pptx", ".doc" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (file == null || file.Length == 0 || string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("DocumentFile", "يُرجى اختيار ملف صالح (PDF, DOCX, PPTX).");
                model.Courses = _courseService.GetAll() ?? Enumerable.Empty<Course>();
                return View("UploadDocument", model);
            }

            try
            {
                var filePath = await _driveSessionService.SaveDocumentAsync(file);

                var resource = new LectureResource
                {
                    Title = model.Title,
                    FilePath = filePath,
                    MimeType = file.ContentType,
                    FileName = file.FileName,
                    CourseId = model.CourseId,
                    UploadDate = DateTime.Now
                };

                _context.LectureResources.Add(resource);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"تم رفع المستند '{model.Title}' بنجاح.";
                return RedirectToAction("View", new { courseId = model.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document: {Title}", model.Title);
                TempData["Error"] = $"خطأ أثناء رفع المستند: {ex.Message}";
                model.Courses = _courseService.GetAll() ?? Enumerable.Empty<Course>();
                return View("UploadDocument", model);
            }
        }

        // ------------------------------------------------------------------
        //                             دوال التنزيل والحذف
        // ------------------------------------------------------------------

        // --- 7. DownloadResource (تنزيل المستندات) ---
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> DownloadResource(int resourceId)
        {
            var resource = await _context.LectureResources.FindAsync(resourceId);

            if (resource == null) return NotFound("لم يتم العثور على الملف.");

            var projectRootPath = Directory.GetCurrentDirectory();
            var fileName = Path.GetFileName(resource.FilePath);
            var correctFolderName = "ProtectedDocuments"; // المجلد الصحيح
            var fullPath = Path.Combine(projectRootPath, correctFolderName, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogError("File missing at {Path}", fullPath);
                return NotFound("الملف غير موجود على الخادم.");
            }

            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, resource.MimeType, resource.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading resource {Id}", resourceId);
                return StatusCode(StatusCodes.Status500InternalServerError, "خطأ في التنزيل.");
            }
        }

        // --- 8. DeleteResource (حذف المستندات) ---
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> DeleteResource(int id, int courseId)
        {
            try
            {
                var resourceToDelete = await _context.LectureResources.FindAsync(id);
                if (resourceToDelete == null)
                {
                    TempData["Error"] = "لم يتم العثور على الملف.";
                    return RedirectToAction("View", new { courseId = courseId });
                }

                var projectRootPath = Directory.GetCurrentDirectory();
                var fileName = Path.GetFileName(resourceToDelete.FilePath);
                var fullPath = Path.Combine(projectRootPath, "ProtectedDocuments", fileName);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                _context.LectureResources.Remove(resourceToDelete);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم حذف المستند بنجاح.";
                return RedirectToAction("View", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resource {Id}", id);
                TempData["Error"] = "حدث خطأ أثناء الحذف.";
                return RedirectToAction("View", new { courseId = courseId });
            }
        }

        // --- 9. Delete (حذف الفيديو) ---
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int? courseId) // جعلنا courseId اختيارياً للمرونة
        {
            try
            {
                var sessionToDelete = await _driveSessionService.GetByIdAsync(id);
                if (sessionToDelete == null) return NotFound("Video not found.");

                _driveSessionService.Delete(sessionToDelete);

                // إذا تم تمرير courseId نعود إليه، وإلا نحاول استنتاجه
                int redirectId = courseId ?? (sessionToDelete.Courses?.FirstOrDefault()?.Id ?? 0);

                if (redirectId > 0)
                {
                    return RedirectToAction("View", new { courseId = redirectId });
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video {Id}", id);
                return StatusCode(500, "خطأ أثناء الحذف.");
            }
        }

        // --- 10. StreamVideo (مشاهدة الفيديو) ---
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> StreamVideo(int sessionId)
        {
            var session = await _driveSessionService.GetByIdAsync(sessionId);
            if (session == null) return NotFound();

            var projectRootPath = Directory.GetCurrentDirectory();
            var fileName = Path.GetFileName(session.FilePath);
            var correctFolderName = "ProtectedVideos"; // المجلد الصحيح للفيديوهات
            var fullPath = Path.Combine(projectRootPath, correctFolderName, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("ملف الفيديو غير موجود.");
            }

            try
            {
                var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return File(stream, session.Content_Type, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming video {Id}", sessionId);
                return StatusCode(500, "خطأ في تشغيل الفيديو.");
            }
        }
    }
}