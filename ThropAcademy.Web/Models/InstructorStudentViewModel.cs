using System.Collections.Generic;
using Throb.Data.Entities; 

namespace ThropAcademy.Web.ViewModels
{
    public class InstructorStudentViewModel
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string CourseName { get; set; }
        public List<UserExamResult> Results { get; set; }
    }
}