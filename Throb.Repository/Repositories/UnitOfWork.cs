using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ThrobDbContext _context;

        public UnitOfWork(ThrobDbContext context)
        {
            _context = context;
        }

        public ICourseRepository CourseRepository { get  ; set  ; }
        public IStudentRepository StudentRepository { get  ; set  ; }
        public IInstructorRepository InstructorRepository { get  ; set  ; }
        public ILiveSessionRepository LiveSessionRepository { get  ; set  ; }
        public IStudentCourseRepository StudentCourseRepository { get  ; set  ; }
        public IInstructorCourseRepository InstructorCourseRepository { get  ; set  ; }

        public int Complete()
        => _context.SaveChanges();
    }
}
