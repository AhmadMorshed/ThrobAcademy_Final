using System;
using System.Collections.Generic;
using Throb.Data.Entities;

namespace Throb.Service.Interfaces
{
    public interface IStudentService
    {
        Student GetById(int id);  // لا حاجة للـ nullable هنا
        IEnumerable<Student> GetAll();
        void Add(Student student);
        void Update(Student student);
        void Delete(Student student);
    }
}
