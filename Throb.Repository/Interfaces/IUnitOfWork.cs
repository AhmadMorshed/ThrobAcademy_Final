using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        public ICourseRepository CourseRepository { get; set; }
        public IStudentRepository StudentRepository { get; set; }
        public IInstructorRepository InstructorRepository { get; set; }
        public ILiveSessionRepository LiveSessionRepository { get; set; }

        public IStudentCourseRepository StudentCourseRepository { get; set; }
        public IInstructorCourseRepository InstructorCourseRepository { get; set; }

        
        int Complete();
    }
}
