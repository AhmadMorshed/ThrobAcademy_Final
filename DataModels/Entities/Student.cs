using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Data.Entities
{
    public class Student
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
        //public string UserId { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime ModifyDate { get; set; }
        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();

    }
}
