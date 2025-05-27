using Microsoft.EntityFrameworkCore;
using AvaloniaAzora.Models;

namespace AvaloniaAzora.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<ClassTest> ClassTests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure existing database schema mappings (columns are already defined in the tables)

            // Configure arrays to be stored as Postgres native arrays
            modelBuilder.Entity<Question>()
                .Property(q => q.Answers)
                .HasColumnType("text[]");

            // Use this to configure entities to match existing schema
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Configure the entity to use the table name as defined in the database
                // Entity Framework Core by default will pluralize and possibly capitalize differently
                entity.SetTableName(entity.GetTableName()?.ToLower());

                // Configure each property to match column names in the database
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }
            }
        }
    }
}
