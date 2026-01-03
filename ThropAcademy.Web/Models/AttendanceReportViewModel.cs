
using System;
using System.Collections.Generic;
using Throb.Data.Entities; // تأكد من استيراد مساحة الاسم الصحيحة لـ AttendanceRecord

namespace ThropAcademy.Web.Models
{
    public class AttendanceReportViewModel
    {
        public string SessionTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public int SessionDuration { get; set; }
        // 💡 يفترض أن يكون لديك كيان AttendanceRecord في طبقة Entities
        public List<AttendanceRecord> Records { get; set; }
    }
}