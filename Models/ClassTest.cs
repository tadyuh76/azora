using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("class_tests")]
    public class ClassTest
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("class_id")]
        public Guid? ClassId { get; set; }

        [Column("test_id")]
        public Guid? TestId { get; set; }

        [Column("start_date")]
        public DateTimeOffset? StartDate { get; set; }

        [Column("due_date")]
        public DateTimeOffset? DueDate { get; set; }

        [Column("limit_attempts")]
        public int? LimitAttempts { get; set; }

        [Column("passing_score")]
        public float? PassingScore { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        [ForeignKey("TestId")]
        public virtual Test? Test { get; set; }

        public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    }
}