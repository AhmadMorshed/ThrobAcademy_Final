using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Service.Interfaces
{
    public interface ICourseService
    {

        Course GetById(int? id);
        IEnumerable<Course> GetAll();
       

        void Add(Course course);
        void Update(Course course);
        void Delete(Course course);
       
    }
}
