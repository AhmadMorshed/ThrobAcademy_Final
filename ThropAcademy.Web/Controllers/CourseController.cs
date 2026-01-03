using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;
using UnitsNet;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IStudentCourseRepository _studentCourseRepository;

        public CourseController(ICourseService courseService,IStudentCourseRepository studentCourseRepository)
        {
            _courseService = courseService;
            _studentCourseRepository = studentCourseRepository;
        }

       
        public IActionResult Index()
        {

            var courses = _courseService.GetAll()
                        .Select(course => new
                        {
                            Course = course,
                            StudentCount = _studentCourseRepository.GetAll()
                                                                 .Count(sc => sc.CourseId == course.Id)
                        })
                        .ToList();

            // إرسال بيانات الكورسات وعدد الطلاب لكل كورس إلى الـ View
            var courseList = courses.Select(c => new Course
            {
                Id = c.Course.Id,
                Name = c.Course.Name,
                StartDate = c.Course.StartDate,
                EndDate = c.Course.EndDate,
                CoursePrice=c.Course.CoursePrice,
                Description=c.Course.Description,
                StudentCount = c.StudentCount,
                CreatedAt = DateTime.Now
            }).ToList();

            return View(courseList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Course course)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    _courseService.Add(course); // إضافة الدورة
                    return RedirectToAction(nameof(Index)); // إعادة التوجيه إلى صفحة قائمة الدورات
                }
                return View(course); // عرض النموذج في حالة وجود خطأ في المدخلات
            }
            catch (InvalidOperationException ex)
            {
                // في حال وجود دورة بنفس الاسم، يتم إضافة الخطأ إلى ModelState
                ModelState.AddModelError("CourseError", ex.Message);
                return View(course); // إعادة تحميل النموذج مع الرسالة
            }
            catch (Exception ex)
            {
                // في حال وجود خطأ آخر
                ModelState.AddModelError("CourseError", ex.Message);
                return View(course); // إعادة تحميل النموذج مع الرسالة
            }
        }
        
        public IActionResult Details(int? id, string viewName = "Details")
        {
            var course = _courseService.GetById(id);

            if (course == null)
                return NotFound("NotFoundPage");

            return View(viewName, course);
        }

        [HttpGet]
        public IActionResult Update(int? id)
        {
            return Details(id, "Update");
        }

        [HttpPost]
        public IActionResult Update(int? id, Course course)
        {
            if (course.Id != id.Value)
                return RedirectToAction("NotFoundPage", null, "Home");

            _courseService.Update(course);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var course = _courseService.GetById(id);

            if (course == null)
                return NotFound("NotFoundPage");

            _courseService.Delete(course);

            return RedirectToAction(nameof(Index));
        }

    }
}













