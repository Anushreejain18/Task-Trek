using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApp.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters")]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string UserRole { get; set; } // "Teacher" or "Student"

        [StringLength(100)]
        public string Username { get; set; } // To specify who the notification is for

        public bool IsRead { get; set; } = false;

        // Foreign key to link notifications to tasks (optional)
        [ForeignKey("StudentTask")]
        public int? TaskId { get; set; }
        public virtual StudentTask StudentTask { get; set; }
    }
}
