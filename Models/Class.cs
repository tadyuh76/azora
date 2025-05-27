using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("classes")]
    public class Class
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("teacher_id")]
        public Guid? TeacherId { get; set; }

        [Required]
        [Column("class_name")]
        public string ClassName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
        public virtual ICollection<ClassTest> ClassTests { get; set; } = new List<ClassTest>();
    }
}