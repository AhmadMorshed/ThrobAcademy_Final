using System;
using System.Collections.Generic;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;

namespace Throb.Service.Services
{
    public class StudentServer : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentServer(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        // إضافة طالب
        public void Add(Student student)
        {
            // إضافة الطالب مباشرة دون الحاجة إلى تحويله
            _studentRepository.Add(student);
        }

        // حذف طالب
        public void Delete(Student student)
        {
            // التحقق إذا كان الطالب موجودًا
            var existingStudent = _studentRepository.GetById(student.Id);
            if (existingStudent == null)
            {
                throw new ArgumentException($"Student with ID {student.Id} not found.");
            }

            // حذف الطالب
            _studentRepository.Delete(existingStudent);
        }

        // الحصول على جميع الطلاب
        public IEnumerable<Student> GetAll()
        {
            var students = _studentRepository.GetAll();
            return students;
        }

        // الحصول على طالب بناءً على المعرف
        public Student GetById(int id)
        {
            // التحقق من وجود الطالب
            var student = _studentRepository.GetById(id);
            if (student == null)
            {
                throw new ArgumentException($"Student with ID {id} not found.");
            }
            return student;
        }

        public Task<IEnumerable<Student>> GetStudentsEnrolledInCourseAsync(int courseId)
        {
            throw new NotImplementedException();
        }



        // تحديث بيانات طالب
        public void Update(Student student)
        {
            // تحقق من وجود الطالب في المستودع
            var existingStudent = _studentRepository.GetById(student.Id);
            if (existingStudent == null)
            {
                throw new ArgumentException($"Student with ID {student.Id} does not exist.");
            }

            // تحديث البيانات
            existingStudent.Name = student.Name;
            existingStudent.Email = student.Email;
            existingStudent.Password = student.Password;

            // تحديث الكورسات في حال وجود تعديلات
            existingStudent.Courses = student.Courses;

            // حفظ التغييرات
            _studentRepository.Update(existingStudent);
        }
    }
}
