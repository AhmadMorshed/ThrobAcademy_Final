using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;
using UnitsNet;

namespace ThropAcademy.Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IStudentCourseRepository _studentCourseRepository;

        public CourseController(ICourseService courseService,IStudentCourseRepository studentCourseRepository)
        {
            _courseService = courseService;
            _studentCourseRepository = studentCourseRepository;
        }

    [Authorize(Roles ="Admin")]

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
        [Authorize(Roles = "Admin")]

        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]

        [HttpPost]
        public IActionResult Create(Course course)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    _courseService.Add(course); 
                    return RedirectToAction(nameof(Index)); 
                }
                return View(course); 
            }
            catch (InvalidOperationException ex)
            {
               
                ModelState.AddModelError("CourseError", ex.Message);
                return View(course); 
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("CourseError", ex.Message);
                return View(course); 
            }
        }
        [Authorize(Roles = "Admin,Instructor,Student")]

        public IActionResult Details(int? id, string viewName = "Details")
        {
            var course = _courseService.GetById(id);

            if (course == null)
                return NotFound("NotFoundPage");

            return View(viewName, course);
        }
        [Authorize(Roles = "Admin")]

        [HttpGet]
        public IActionResult Update(int? id)
        {
            return Details(id, "Update");
        }
        [Authorize(Roles = "Admin")]

        [HttpPost]
        public IActionResult Update(int? id, Course course)
        {
            if (course.Id != id.Value)
                return RedirectToAction("NotFoundPage", null, "Home");

            _courseService.Update(course);

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]

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













