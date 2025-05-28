using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("tests")]
    public class Test
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("creator_id")]
        public Guid? CreatorId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("time_limit")]
        public int? TimeLimit { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<ClassTest> ClassTests { get; set; } = new List<ClassTest>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}