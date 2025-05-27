using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("role")]
        public string? Role { get; set; }

        // Navigation properties
        public virtual ICollection<Class> TeacherClasses { get; set; } = new List<Class>();
        public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
        public virtual ICollection<Test> CreatedTests { get; set; } = new List<Test>();
        public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}