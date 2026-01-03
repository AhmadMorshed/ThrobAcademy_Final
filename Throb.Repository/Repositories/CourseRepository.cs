using Throb.Data.DbContext;
using Throb.Data.Entities;
using System.Linq;

using Throb.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Throb.Repository.Repositories
{
    public class CourseRepository :GenericRepository<Course>, ICourseRepository
    {
        private readonly ThrobDbContext _context;

        public CourseRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Course> GetAll()
        {
            return _context.Courses
                .Include(s =>s.InstructorCourses )
                ;
        }

        //public void Add(Course course)
        // => _context.Add(course);

        //public void Delete(Course course)
        // => _context.Remove(course);

        //public IEnumerable<Course> GetAll() => _context.Courses.ToList();

        //public Course GetById(int id)

        //=> _context.Courses.Find(id);

        //public void Update(Course course)
        //    => _context.Update(course);

    }
}
