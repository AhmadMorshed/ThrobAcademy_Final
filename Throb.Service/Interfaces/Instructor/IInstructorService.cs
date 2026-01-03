using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Service.Interfaces
{
    public interface IInstructorService
    {
        Instructor GetById(int id);
        IEnumerable<Instructor> GetAll();
        void Add (Instructor instructor);
        void Update (Instructor instructor);
        void Delete (Instructor instructor);
    }
}
