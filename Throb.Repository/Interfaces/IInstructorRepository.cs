using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Repository.Interfaces
{
    public interface IInstructorRepository:IGenericRepository<Instructor>
    {
        //Instructor GetById(int id);
        IQueryable<Instructor> GetAll();
        //void Add (Instructor instructor);
        //void Update (Instructor instructor);
        //void Delete (Instructor instructor);
        IEnumerable<Instructor> GetInstructorByName(string name);

    }
}
