using Microsoft.AspNetCore.Mvc;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using Throb.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]

    public class InstructorController : Controller
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorCourseRepository _instructorCourseRepository;

        public InstructorController(IInstructorRepository instructorRepository, ICourseRepository courseRepository, IInstructorCourseRepository instructorCourseRepository)
        {
            _instructorRepository = instructorRepository;
            _courseRepository = courseRepository;
            _instructorCourseRepository = instructorCourseRepository;
        }

        // تحميل الكورسات لعرضها في الـ View
        public void LoadCourses()
        {
            var courses = _courseRepository.GetAll();
            ViewBag.Courses = new SelectList(courses, "Id", "Name");
        }

        // عرض قائمة المعلمين مع البحث
        public IActionResult Index(string searchInp)
        {
            if (!string.IsNullOrEmpty(searchInp))
            {
                // البحث عن المعلمين بناءً على النص المدخل
                var instructors = _instructorRepository.GetInstructorByName(searchInp)
                                                        
                                                        .ToList();
                return View(instructors);
            }

            // إذا لم يتم إدخال نص في مربع البحث، عرض جميع المعلمين
            var allInstructors = _instructorRepository.GetAll()
                                                     .Include(i => i.InstructorCourses)
                                                     .ThenInclude(ic => ic.Course)
                                                     .ToList();

            return View(allInstructors);
        }

        // عرض نموذج إضافة معلم جديد
        public IActionResult Create()
        {
            LoadCourses();
            return View();
        }

        // معالجة إضافة معلم جديد
        [HttpPost]
        public IActionResult Create(Instructor instructor, int[] selectedCourses)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    instructor.CreateAt = DateTime.Now;
                    _instructorRepository.Add(instructor); // إضافة المعلم إلى قاعدة البيانات

                    // إضافة الكورسات التي تم اختيارها للمعلم
                    if (selectedCourses != null && selectedCourses.Any())
                    {
                        foreach (var courseId in selectedCourses)
                        {
                            var instructorCourse = new InstructorCourse
                            {
                                InstructorId = instructor.Id,
                                CourseId = courseId
                            };
                            _instructorCourseRepository.Add(instructorCourse); // إضافة العلاقة بين المعلم والكورس
                        }
                    }

                    return RedirectToAction(nameof(Index)); // إعادة التوجيه إلى صفحة عرض المعلمين
                }

                LoadCourses();
                return View(instructor);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CourseError", ex.Message);
                LoadCourses();
                return View(instructor);
            }
        }

        // عرض نموذج تعديل المعلم
        public IActionResult Update(int id)
        {
            var instructor = _instructorRepository.GetById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            LoadCourses();
            return View(instructor);
        }

        // معالجة تعديل المعلم
        [HttpPost]
        public IActionResult Update(Instructor instructor, int[] selectedCourses)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingInstructor = _instructorRepository.GetById(instructor.Id);
                    if (existingInstructor == null)
                    {
                        return NotFound();
                    }

                    existingInstructor.Name = instructor.Name;
                    existingInstructor.Email = instructor.Email;
                    existingInstructor.Password = instructor.Password;
                    _instructorRepository.Update(existingInstructor);

                    // إضافة الكورسات التي تم اختيارها
                    if (selectedCourses != null && selectedCourses.Any())
                    {
                        foreach (var courseId in selectedCourses)
                        {
                            var instructorCourse = new InstructorCourse
                            {
                                InstructorId = instructor.Id,
                                CourseId = courseId
                            };
                            _instructorCourseRepository.Add(instructorCourse); // إضافة الكورس
                        }
                    }

                    return RedirectToAction(nameof(Index)); // إعادة التوجيه بعد التعديل
                }

                LoadCourses();
                return View(instructor);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("UpdateError", ex.Message);
                LoadCourses();
                return View(instructor);
            }
        }
        public IActionResult Details(int id, string viewName = "Details")
        {
            var instructor = _instructorRepository.GetById(id);

            if (instructor == null)
                return NotFound("NotFoundPage");

            return View(viewName, instructor);
        }

        // حذف المعلم
        public IActionResult Delete(int id)
        {
            var instructor = _instructorRepository.GetById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            _instructorRepository.Delete(instructor);
            return RedirectToAction(nameof(Index)); // إعادة التوجيه بعد الحذف
        }
    }
}
