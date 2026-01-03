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
    public class InstructorCourseRepository : GenericRepository<InstructorCourse>, IInstructorCourseRepository
    {
        private readonly ThrobDbContext _context;
        public InstructorCourseRepository(ThrobDbContext context) : base(context)
        {
            _context = context;

        }
    }
}
