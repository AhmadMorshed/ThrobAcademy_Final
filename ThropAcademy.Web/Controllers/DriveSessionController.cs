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
using Throb.Service.Services; 
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


        public async Task<IActionResult> Index()
        {
            // 1. الحصول على اسم المستخدم الحالي (المدرب أو الطالب أو الأدمن)
            var currentUserName = User.Identity.Name;

            // 2. إذا كان المستخدم "Instructor" (مدرب)
            if (User.IsInRole("Instructor"))
            {
                // جلب معرفات الكورسات التي يدرسها هذا المدرب فقط
                var instructorCourseIds = await _context.InstructorCourses
                    .Include(ic => ic.Instructor)
                    .Where(ic => ic.Instructor.Name == currentUserName)
                    .Select(ic => ic.CourseId)
                    .ToListAsync();

                // جلب تفاصيل هذه الكورسات فقط لعرضها في الصفحة
                var instructorCourses = await _context.Courses
                    .Where(c => instructorCourseIds.Contains(c.Id))
                    .ToListAsync();

                return View(instructorCourses);
            }

            // 3. إذا كان المستخدم "Student" (طالب) - اختياري إذا أردت فلترة الطالب أيضاً
            if (User.IsInRole("Student"))
            {
                var studentCourseIds = await _context.StudentCourses
                    .Include(sc => sc.Student)
                    .Where(sc => sc.Student.Name == currentUserName)
                    .Select(sc => sc.CourseId)
                    .ToListAsync();

                var studentCourses = await _context.Courses
                    .Where(c => studentCourseIds.Contains(c.Id))
                    .ToListAsync();

                return View(studentCourses);
            }

            // 4. إذا كان "Admin"، يرى كل الكورسات كالمعتاد
            var allCourses = _courseService.GetAll();
            return View(allCourses);
        }


        [HttpGet]
        public async Task<IActionResult> View(int courseId)
        {
            
            var videos = await _driveSessionService.GetByCourseId(courseId);

            var documents = await _context.LectureResources
                                          .Where(r => r.CourseId == courseId)
                                          .ToListAsync();

           
            var viewModel = new CourseContentViewModel
            {
                CourseId = courseId,
                Videos = videos,
                Documents = documents
            };

            return View(viewModel);
        }

  

        
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

       
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [RequestSizeLimit(209715200)] 
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

        
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UploadDocument(UploadDocumentViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                model.Courses = _courseService.GetAll() ?? Enumerable.Empty<Course>();
                TempData["Error"] = "الرجاء ملء جميع الحقول المطلوبة.";
               
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


   
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> DownloadResource(int resourceId)
        {
            var resource = await _context.LectureResources.FindAsync(resourceId);

            if (resource == null) return NotFound("لم يتم العثور على الملف.");

            var projectRootPath = Directory.GetCurrentDirectory();
            var fileName = Path.GetFileName(resource.FilePath);
            var correctFolderName = "ProtectedDocuments"; 
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

       
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int? courseId) 
        {
            try
            {
                var sessionToDelete = await _driveSessionService.GetByIdAsync(id);
                if (sessionToDelete == null) return NotFound("Video not found.");

                _driveSessionService.Delete(sessionToDelete);

                
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
        [HttpGet("DriveSession/Details/{id}")]
        public IActionResult Details(int? id, string viewName = "Details")
        {

            // 1. هل الرقم يصل للأكشن أصلاً؟
            if (id == null)
            {
                return Content("الرقم (ID) لم يصل للأكشن، تحقق من الـ Routing");
            }

            var course = _courseService.GetById(id);

            // 2. هل الخدمة تجد البيانات؟
            if (course == null)
            {
                return Content($"تم استقبال المعرف {id} بنجاح، ولكن الـ Service لم تجد كورس بهذا الرقم في قاعدة البيانات.");
            }

            return View(viewName, course);
        }
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> StreamVideo(int sessionId)
        {
            var session = await _driveSessionService.GetByIdAsync(sessionId);
            if (session == null) return NotFound();

            var projectRootPath = Directory.GetCurrentDirectory();
            var fileName = Path.GetFileName(session.FilePath);
            var correctFolderName = "ProtectedVideos"; 
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