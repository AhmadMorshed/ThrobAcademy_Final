using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;
using System;
using System.Linq;

namespace Throb.Service.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public void Add(Course course)
        {
            // تهيئة اسم الدورة للمقارنة
            var normalizedName = (course.Name ?? string.Empty).Trim().ToLowerInvariant();

            // استخدام AsEnumerable لإجراء التحقق غير الحساس لحالة الأحرف في الذاكرة
            var exists = _courseRepository.GetAll()
                                          .AsEnumerable() // التحويل إلى التقييم من جانب العميل
                                          .Any(c => ((c.Name ?? string.Empty).Trim().ToLowerInvariant()) == normalizedName);

            if (exists)
            {
                // إلقاء استثناء إذا كانت هناك دورة بنفس الاسم موجودة
                throw new InvalidOperationException($"دورة بنفس الاسم '{course.Name}' موجودة بالفعل.");
            }

            // إنشاء دورة جديدة
            var mappedCourse = new Course
            {
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CreatedAt = DateTime.Now,
                CoursePrice = course.CoursePrice,
            };

            _courseRepository.Add(mappedCourse);
        }

        public void Delete(Course course)
        {
            _courseRepository.Delete(course);
        }

        public IEnumerable<Course> GetAll()
        {
            var courses = _courseRepository.GetAll().ToList();
            return courses;
        }

      
        public Course GetById(int? id)
        {
            if (id is null)
                return null;

            var course = _courseRepository.GetById(id.Value);

            if (course is null)
                return null;

            return course;
        }

        public void Update(Course course)
        {
            _courseRepository.Update(course);
        }

    }
}