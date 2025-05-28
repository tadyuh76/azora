using Microsoft.EntityFrameworkCore;
using AvaloniaAzora.Models;
using System;

namespace AvaloniaAzora.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Class> Classes { get; set; } = null!;
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; } = null!;
        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<ClassTest> ClassTests { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Attempt> Attempts { get; set; } = null!;
        public DbSet<UserAnswer> UserAnswers { get; set; } = null!;
        public DbSet<Log> Logs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Disable sensitive data logging for cleaner console output
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure based on your actual model properties only
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.FullName).IsRequired(false);
                entity.Property(e => e.Role).HasDefaultValue("student");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClassName).IsRequired();
                entity.HasOne(e => e.Teacher)
                      .WithMany()
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ClassEnrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Class)
                      .WithMany()
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClassTest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Class)
                      .WithMany()
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Test)
                      .WithMany()
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.HasOne(e => e.Test)
                      .WithMany(t => t.Questions)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Attempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ClassTest)
                      .WithMany()
                      .HasForeignKey(e => e.ClassTestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Attempt)
                      .WithMany(a => a.UserAnswers)
                      .HasForeignKey(e => e.AttemptId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Only configure properties that actually exist in your Log model
            });
        }
    }
}
