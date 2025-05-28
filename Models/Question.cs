using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    /// <summary>
    /// Represents a test question in the database.
    /// </summary>
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

        /// <summary>
        /// The question text displayed to students
        /// </summary>
        [Required]
        [Column("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The question type: "multiple_choice", "true_false", or "short_answer"
        /// </summary>
        [Column("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Point value for this question (default: 5)
        /// </summary>
        [Column("points")]
        public int? Points { get; set; }

        /// <summary>
        /// Question difficulty: "easy", "medium", "hard", or "intense"
        /// </summary>
        [Column("difficulty")]
        public string? Difficulty { get; set; }

        /// <summary>
        /// For multiple_choice: Array of answer options
        /// For true_false: null or empty array
        /// For short_answer: null or empty array
        /// </summary>
        [Column("answers")]
        public string[]? Answers { get; set; }

        /// <summary>
        /// For multiple_choice: Index of correct option (1-based, as string)
        /// For true_false: "true" or "false"
        /// For short_answer: The expected answer text
        /// </summary>
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