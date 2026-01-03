using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]

public class StudentController : Controller
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentCourseRepository _studentCourseRepository;

    public StudentController(IStudentRepository studentRepository, ICourseRepository courseRepository, IStudentCourseRepository studentCourseRepository)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _studentCourseRepository = studentCourseRepository;
    }

    public void LoadCourses()
    {
        var courses = _courseRepository.GetAll();
        ViewBag.Courses = new SelectList(courses, "Id", "Name");
    }


    public IActionResult Index(string searchInp)
    {
        // جلب جميع الطلاب مع الكورسات التي سجلوا فيها
        var students = _studentRepository.GetAll()

            .Include(s => s.StudentCourses)

            .ThenInclude(sc => sc.Course)  // تضمين الكورسات التي سجل فيها الطلاب
            .ToList();

        // إذا تم إدخال نص في البحث، تطبيق الفلترة
        if (!string.IsNullOrEmpty(searchInp))
        {
            students = students.Where(s => s.Name.Contains(searchInp, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(students);
    }



    public IActionResult Create()
    {
        LoadCourses();
        return View();
    }

    // معالجة إضافة طالب جديد
    [HttpPost]
    public IActionResult Create(Student student, int[] selectedCourses)
    {
        try
        {
            if (ModelState.IsValid)
            {
                student.CreateAt = DateTime.Now;
                _studentRepository.Add(student); // إضافة الطالب إلى قاعدة البيانات

                // إضافة الكورسات التي تم اختيارها للطالب
                if (selectedCourses != null && selectedCourses.Any())
                {
                    foreach (var courseId in selectedCourses)
                    {
                        var studentCourse = new StudentCourse
                        {
                            StudentId = student.Id,
                            CourseId = courseId
                        };
                        _studentCourseRepository.Add(studentCourse); // إضافة العلاقة بين الطالب والكورس
                    }
                }

                return RedirectToAction(nameof(Index)); // إعادة التوجيه إلى صفحة عرض الطلاب
            }

            // في حالة فشل التحقق من النموذج، نعيد تحميل الكورسات في الـ View
            var courses = _courseRepository.GetAll();
            ViewBag.Courses = new SelectList(courses, "Id", "Name");
            return View(student);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("CourseError", ex.Message);
            var courses = _courseRepository.GetAll();
            ViewBag.Courses = new SelectList(courses, "Id", "Name");
            return View(student);
        }
    }
    public IActionResult Details(int id, string viewName = "Details")
    {
        var student = _studentRepository.GetById(id);

        if (student == null)
            return NotFound("NotFoundPage");

        return View(viewName, student);
    }

    
    [HttpGet]
    public IActionResult Update(int id)
    {
        var student = _studentRepository.GetById(id);
        if (student == null)
        {
            return NotFound();
        }



        LoadCourses();
        return Details(id, "Update");


    }

    [HttpPost]
    public IActionResult Update(Student student, int[] selectedCourses)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var existingStudent = _studentRepository.GetById(student.Id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                existingStudent.Name = student.Name;
                existingStudent.Password = student.Password;
                existingStudent.Email = student.Email;
                _studentRepository.Update(existingStudent);

                if (selectedCourses != null && selectedCourses.Any())
                {
                    foreach (var courseId in selectedCourses)
                    {
                        var studentCourse = new StudentCourse
                        {
                            StudentId = student.Id,
                            CourseId = courseId
                        };
                        _studentCourseRepository.Add(studentCourse);
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            LoadCourses();
            return View(student);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("UpdateError", ex.Message);
            LoadCourses();
            return View(student);
        }
    }

    public IActionResult Delete(int id)
    {
        var student = _studentRepository.GetById(id);
        if (student == null)
            return NotFound("NotFoundPage");

        _studentRepository.Delete(student);
        return RedirectToAction(nameof(Index));
    }
}
