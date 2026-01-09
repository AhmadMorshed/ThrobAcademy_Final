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

       
        public void LoadCourses()
        {
            var courses = _courseRepository.GetAll();
            ViewBag.Courses = new SelectList(courses, "Id", "Name");
        }

      
        public IActionResult Index(string searchInp)
        {
            if (!string.IsNullOrEmpty(searchInp))
            {
                
                var instructors = _instructorRepository.GetInstructorByName(searchInp)
                                                        
                                                        .ToList();
                return View(instructors);
            }

            
            var allInstructors = _instructorRepository.GetAll()
                                                     .Include(i => i.InstructorCourses)
                                                     .ThenInclude(ic => ic.Course)
                                                     .ToList();

            return View(allInstructors);
        }

    
        public IActionResult Create()
        {
            LoadCourses();
            return View();
        }

   
        [HttpPost]
        public IActionResult Create(Instructor instructor, int[] selectedCourses)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    instructor.CreateAt = DateTime.Now;
                    _instructorRepository.Add(instructor);

                   
                    if (selectedCourses != null && selectedCourses.Any())
                    {
                        foreach (var courseId in selectedCourses)
                        {
                            var instructorCourse = new InstructorCourse
                            {
                                InstructorId = instructor.Id,
                                CourseId = courseId
                            };
                            _instructorCourseRepository.Add(instructorCourse); 
                        }
                    }

                    return RedirectToAction(nameof(Index)); 
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

                    
                    if (selectedCourses != null && selectedCourses.Any())
                    {
                        foreach (var courseId in selectedCourses)
                        {
                            var instructorCourse = new InstructorCourse
                            {
                                InstructorId = instructor.Id,
                                CourseId = courseId
                            };
                            _instructorCourseRepository.Add(instructorCourse); 
                        }
                    }

                    return RedirectToAction(nameof(Index));
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

       
        public IActionResult Delete(int id)
        {
            var instructor = _instructorRepository.GetById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            _instructorRepository.Delete(instructor);
            return RedirectToAction(nameof(Index)); 
        }
    }
}
