using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Repository.Repositories;
using Throb.Service.Interfaces;

namespace Throb.Service.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;

        public InstructorService(IInstructorRepository instructorRepository)
        {
            _instructorRepository = instructorRepository;
        }

        public void Add(Instructor instructor)
        {
            _instructorRepository.Add(instructor);

        }

        public void Delete(Instructor instructor)
        {
            var existinginstructor = _instructorRepository.GetById(instructor.Id);
            if (existinginstructor == null)

            {
                throw new ArgumentException($"instructor with ID {instructor.Id} not found.");
            }

            // حذف الطالب
            _instructorRepository.Delete(existinginstructor);
        }

        public IEnumerable<Instructor> GetAll()
        {
            var instructors = _instructorRepository.GetAll();
            return instructors;
        }

        public Instructor GetById(int id)
        {
            var instructor = _instructorRepository.GetById(id);
            if (instructor == null)
            {
                throw new ArgumentException($"Instructor with ID {id} not found.");
            }
            return instructor;
        }

        public void Update(Instructor instructor)
        {
            var existingInstructor = _instructorRepository.GetById(instructor.Id);
            if (existingInstructor == null)
            {
                throw new ArgumentException($"instructor with ID {instructor.Id} does not exist.");
            }

            // تحديث البيانات
            existingInstructor.Name = instructor.Name;
            existingInstructor.Email = instructor.Email;
            existingInstructor.Password = instructor.Password;

            // تحديث الكورسات في حال وجود تعديلات
            existingInstructor.Courses = instructor.Courses;

            // حفظ التغييرات
            _instructorRepository.Update(existingInstructor);
        }
    }
}
