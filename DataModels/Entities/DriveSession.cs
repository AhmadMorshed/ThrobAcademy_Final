using System;
using System.Collections.Generic;

namespace Throb.Data.Entities
{
    public class DriveSession
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public string Content_Type { get; set; }
        public string FilePath { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}