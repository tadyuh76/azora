using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaAzora.Models
{
    /// <summary>
    /// Represents a student's answer to a test question
    /// </summary>
    [Table("user_answers")]
    public class UserAnswer
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("attempt_id")]
        public Guid? AttemptId { get; set; }

        [Column("question_id")]
        public Guid? QuestionId { get; set; }

        /// <summary>
        /// For multiple_choice: Index of selected option (1-based, as string)
        /// For true_false: "true" or "false"
        /// For short_answer: The student's answer text
        /// </summary>
        [Column("answer_text")]
        public string? AnswerText { get; set; }

        [Column("answered_at")]
        public DateTimeOffset AnsweredAt { get; set; } = DateTimeOffset.Now;

        // Navigation properties
        [ForeignKey("AttemptId")]
        public virtual Attempt? Attempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }
    }
}