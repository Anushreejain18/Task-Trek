using System;
using System.ComponentModel.DataAnnotations;

namespace LoginApp.Models
{
    public class Resource
    {
        public int ResourceId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }

        public string UploadedBy { get; set; } // "Teacher" or "Student"

        public int? StudentId { get; set; }
        public virtual Student Student { get; set; }

        public string StudentComment { get; set; }

        public string Class { get; set; }
    }
}