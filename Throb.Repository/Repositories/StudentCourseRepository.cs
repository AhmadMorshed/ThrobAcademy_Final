using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class StudentCourseRepository : GenericRepository<StudentCourse>, IStudentCourseRepository
    {
        private readonly ThrobDbContext _context;
        public StudentCourseRepository(ThrobDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
