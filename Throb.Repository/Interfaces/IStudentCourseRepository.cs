using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Repository.Interfaces
{
    public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
    {
        IEnumerable<StudentCourse> GetAll();
    }
}
