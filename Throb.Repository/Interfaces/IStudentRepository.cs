using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Repository.Interfaces
{
    public interface IStudentRepository:IGenericRepository<Student>
    {
        //Student GetById(int id);
        IQueryable<Student> GetAll();
        //void Add(Student student);
        //void Update(Student student);
        //void Delete(Student student);
        IEnumerable<Student> GetStudentByName( string name);

    }
}
