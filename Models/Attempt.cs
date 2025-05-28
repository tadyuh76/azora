using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("attempts")]
    public class Attempt
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("student_id")]
        public Guid? StudentId { get; set; }

        [Column("class_test_id")]
        public Guid? ClassTestId { get; set; }

        [Column("start_time")]
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

        [Column("end_time")]
        public DateTimeOffset? EndTime { get; set; }

        [Column("score")]
        public float? Score { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }

        [ForeignKey("ClassTestId")]
        public virtual ClassTest? ClassTest { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}