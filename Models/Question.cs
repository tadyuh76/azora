using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("test_id")]
        public Guid? TestId { get; set; }

        [Column("category_id")]
        public Guid? CategoryId { get; set; }

        [Required]
        [Column("text")]
        public string Text { get; set; } = string.Empty;

        [Column("type")]
        public string? Type { get; set; }

        [Column("points")]
        public int? Points { get; set; }

        [Column("difficulty")]
        public string? Difficulty { get; set; }

        [Column("answers")]
        public string[]? Answers { get; set; }

        [Column("correct_answer")]
        public string? CorrectAnswer { get; set; }

        // Navigation properties
        [ForeignKey("TestId")]
        public virtual Test? Test { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}