using System.ComponentModel.DataAnnotations;

namespace ThropAcademy.Web.Models
{
    // نموذج إدخال البيانات (Input Model)
    public class UploadVideoInputModel
    {
        [Required(ErrorMessage = "الرجاء إدخال عنوان المحاضرة.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار ملف فيديو.")]
        [Display(Name = "ملف الفيديو")]
        public IFormFile VideoFile { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار كورس واحد على الأقل.")]
        // 🛑 هذا يسبب المشكلة إذا كانت البيانات القادمة List<int>
        public int[] CourseIds { get; set; }
    }
}
