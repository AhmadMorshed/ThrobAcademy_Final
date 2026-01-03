using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Throb.Data.Entities;

namespace ThropAcademy.Web.Models
{


    public class UploadVideoViewModel
    {
        public string Title { get; set; }
        public IFormFile VideoFile { get; set; }
        [Required(ErrorMessage = "الرجاء اختيار كورس واحد على الأقل.")]
        public int[] CourseIds { get; set; }
        [BindNever]
        public IEnumerable<Course> Courses { get; set; }
    }
}
