using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Throb.Data.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal CoursePrice { get; set; }
        public int StudentCount { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<InstructorCourse>? InstructorCourses { get; set; }
        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public LiveSession? LiveSession { get; set; }
        public int? LiveSessionId { get; set; }
        public ICollection<DriveSession> DriveSessions { get; set; } = new List<DriveSession>();
    }
}