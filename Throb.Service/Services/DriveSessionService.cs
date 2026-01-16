using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;

namespace Throb.Service.Services
{
    public class DriveSessionService : IDriveSessionService
    {
        private readonly IDriveSessionRepository _driveSessionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IConfiguration _configuration;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedVideos");
        private readonly ILogger<DriveSessionService> _logger;

        public DriveSessionService(IDriveSessionRepository driveSessionRepository, ICourseRepository courseRepository, ILogger<DriveSessionService> logger, IConfiguration configuration)
        {
            _driveSessionRepository = driveSessionRepository;
            _courseRepository = courseRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task AddAsync(IFormFile file, int[] courseIds, string title)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (courseIds == null || !courseIds.Any())
                throw new ArgumentException("At least one course ID is required.");

            // 1. جلب الدورات
            var courses = await _courseRepository.GetAll()
                .Where(c => courseIds.Contains(c.Id))
                .ToListAsync();
            if (!courses.Any())
                throw new ArgumentException("Invalid Course IDs.");

            // 2. التحقق من مسار التخزين وإنشائه
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                _logger.LogInformation("Created storage directory: {StoragePath}", _storagePath);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePathOnDisk = Path.Combine(_storagePath, fileName);
            bool fileCreated = false; // متغير تتبع حالة إنشاء الملف على القرص

            // 3. حفظ الملف على القرص
            try
            {
                using (var stream = new FileStream(filePathOnDisk, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                fileCreated = true; // تم إنشاء الملف بنجاح
                _logger.LogInformation("File saved successfully at: {FilePath}", filePathOnDisk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file at: {FilePath}", filePathOnDisk);
                throw;
            }

            // 4. إعداد سجل قاعدة البيانات
            var driveSession = new DriveSession
            {
                Title = title,
                UploadDate = DateTime.UtcNow,
                // ملاحظة: يُفضل تغيير التسمية في Entity إلى ContentType
                Content_Type = file.ContentType,
                // ✅ حفظ اسم الملف الفريد فقط (الكنترولر سيبني المسار الكامل)
                FilePath = fileName,
                Courses = courses
            };

            // 5. حفظ السجل في قاعدة البيانات مع منطق التراجع (Rollback)
            try
            {
                _driveSessionRepository.Add(driveSession);
                // ⚠️ ملاحظة: يجب أن يتبع هذا الإجراء استدعاء لـ SaveChangesAsync في UnitOfWork/Repository
                _logger.LogInformation("DriveSession added to database with ID: {Id}", driveSession.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add DriveSession to database with title: {Title}", title);

                // 🛑 منطق التراجع (Rollback): حذف الملف إذا فشل حفظ DB
                if (fileCreated && File.Exists(filePathOnDisk))
                {
                    File.Delete(filePathOnDisk);
                    _logger.LogWarning("Orphan file deleted due to DB failure: {FilePathToDelete}", filePathOnDisk);
                }
                throw;
            }
        }

        // في DriveSessionService.cs
        public void Delete(DriveSession driveSession)
        {
            if (driveSession == null)
                throw new ArgumentNullException(nameof(driveSession));

            // 1. تحديد المسار المحمي (حيث يتم حفظ الفيديوهات الجديدة)
            var protectedPath = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedVideos", driveSession.FilePath);

            // 2. تحديد المسار القديم (للفيديوهات القديمة في wwwroot)
            // نستخدم Path.GetFileName للتأكد من الحصول على اسم الملف فقط من البيانات القديمة
            var fileName = Path.GetFileName(driveSession.FilePath);
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", fileName);

            // 3. محاولة حذف الملف من المسار الجديد أولاً
            if (File.Exists(protectedPath))
            {
                File.Delete(protectedPath);
                _logger.LogInformation("Deleted file from ProtectedVideos: {Path}", protectedPath);
            }
            // 4. إذا لم يكن موجوداً في الجديد، نحذف الملف من المسار القديم
            else if (File.Exists(wwwrootPath))
            {
                File.Delete(wwwrootPath);
                _logger.LogWarning("Deleted file from OLD wwwroot path: {Path}", wwwrootPath);
            }
            else
            {
                _logger.LogWarning("File not found on disk for deletion: {FileName}", fileName);
            }

            // 5. حذف السجل من قاعدة البيانات
            _driveSessionRepository.Delete(driveSession);
        }

        public IEnumerable<DriveSession> GetAll()
        {
            return _driveSessionRepository.GetAll();
        }

        public DriveSession? GetById(int? id)
        {
            if (id == null)
                return null;

            return _driveSessionRepository.GetById(id.Value);
        }

        public async Task<IEnumerable<DriveSession>> GetByCourseId(int courseId)
        {
            return await _driveSessionRepository.GetByCourseIdAsync(courseId);
        }

        public void Update(DriveSession driveSession)
        {
            if (driveSession == null)
                throw new ArgumentNullException(nameof(driveSession));

            _driveSessionRepository.Update(driveSession);
        }

        public async Task<DriveSession?> GetByIdAsync(int id)
        {
            // استخدام دالة Repository المتزامنة التي تقوم بتحميل الكورسات
            return await _driveSessionRepository.GetByIdAsync(id);
            // ملاحظة: GetByIdAsync في الـ Repository الخاص بك كان يجلب الكورسات (Include(ds => ds.Courses))، وهو ما نحتاجه.
        }

        // 🟢 تنفيذ ميثود حفظ المستندات (المحدد في استفسارك)
        public async Task<string> SaveDocumentAsync(IFormFile file)
        {
            try
            {
                // 1. قراءة اسم المجلد الآمن من الإعدادات (مثلاً: "ProtectedDocuments")
                var folderName = _configuration["UploadPaths:Documents"];

                if (string.IsNullOrEmpty(folderName))
                {
                    _logger.LogError("UploadPaths:Documents configuration key is missing or empty.");
                    throw new InvalidOperationException("مسار رفع المستندات غير مُعرّف في إعدادات التطبيق.");
                }

                // 2. بناء المسار الكامل لحفظ الملف على الخادم (مسار نظام التشغيل)
                // Directory.GetCurrentDirectory() يعطي جذر المشروع (مثلاً: C:\Users\...\ThropAcademy.Web)
                var rootPath = Directory.GetCurrentDirectory();
                var fullUploadPath = Path.Combine(rootPath, folderName);

                // 3. إنشاء المجلد إذا لم يكن موجوداً
                if (!Directory.Exists(fullUploadPath))
                {
                    Directory.CreateDirectory(fullUploadPath);
                    _logger.LogInformation("Created document upload directory: {UploadPath}", fullUploadPath);
                }

                // 4. إنشاء اسم ملف فريد (للتجنب التضارب)
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var fullFilePath = Path.Combine(fullUploadPath, uniqueFileName);

                // 5. حفظ الملف فعليًا
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Document saved successfully at: {FullPath}", fullFilePath);

                // 6. إرجاع المسار النسبي (اسم المجلد + اسم الملف) للتخزين في قاعدة البيانات.
                // مثال: ProtectedDocuments/uniqueID.pdf. هذا المسار آمن ومحمول.
                // يجب أن يتم تعديل الشرطة المائلة (Slashes) لتكون متوافقة مع URL إذا لزم الأمر
                // (Path.Combine يستخدم الشرطة المائلة العكسية \ في Windows، لكن Path.Combine يعمل جيداً عند قراءته لاحقاً)
                return Path.Combine(folderName, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في حفظ المستند على الخادم.");
                // إطلاق خطأ جديد ليعرض رسالة واضحة للمستخدم النهائي
                throw new Exception("فشل في حفظ المستند على الخادم.", ex);
            }
        }
    }
}