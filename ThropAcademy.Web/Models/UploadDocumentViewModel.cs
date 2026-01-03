using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Throb.Data.Entities;
using Microsoft.AspNetCore.Http; // 🟢 تصحيح: ضروري لـ IFormFile

namespace ThropAcademy.Web.Models
{
    // هذا النموذج يستخدم لاستقبال البيانات من نموذج رفع المستندات (POST)
    public class UploadDocumentViewModel
    {
        [Required(ErrorMessage = "الرجاء إدخال عنوان للمستند.")]
        [StringLength(100, ErrorMessage = "يجب ألا يتجاوز العنوان 100 حرف.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار ملف للرفع.")]
        public IFormFile DocumentFile { get; set; } // 🟢 تصحيح: تغيير الاسم ليكون واضحاً

        [Required(ErrorMessage = "الرجاء اختيار الكورس لربط المستند به.")]
        public int CourseId { get; set; }

        // هذا الحقل يستخدم فقط لعرض قائمة الكورسات في واجهة المستخدم (GET)
        [BindNever]
        public IEnumerable<Course> Courses { get; set; }
    }
}