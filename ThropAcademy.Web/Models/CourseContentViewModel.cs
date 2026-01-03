using System.Collections.Generic;
using Throb.Data.Entities;

namespace ThropAcademy.Web.Models
{
    public class CourseContentViewModel
    {
        public int CourseId { get; set; }

        // قائمة الفيديوهات (DriveSession)
        public IEnumerable<DriveSession> Videos { get; set; }

        // قائمة المستندات (LectureResource)
        public IEnumerable<LectureResource> Documents { get; set; }
    }
}