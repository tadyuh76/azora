using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("class_enrollments")]
    public class ClassEnrollment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("class_id")]
        public Guid? ClassId { get; set; }

        [Column("student_id")]
        public Guid? StudentId { get; set; }

        [Column("enrollment_date")]
        public DateTimeOffset EnrollmentDate { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }
    }
}