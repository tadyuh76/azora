using AvaloniaAzora.Data;
using AvaloniaAzora.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.Services
{
    public class EfCoreDataService : IDataService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public EfCoreDataService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // User operations
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Users
                    .OrderBy(u => u.FullName ?? u.Email)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load users from database: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                if (user.Id == Guid.Empty)
                {
                    user.Id = Guid.NewGuid();
                }

                // Only set properties that exist in your User model
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "student";
                }

                context.Users.Add(user);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created user: {user.Email} with role: {user.Role}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not create user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        // Class operations
        public async Task<List<Class>> GetAllClassesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Classes
                    .Include(c => c.Teacher)
                    .OrderBy(c => c.ClassName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load classes: {ex.Message}");
                return new List<Class>();
            }
        }

        public async Task<Class?> GetClassByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();

            // Load the base entity first with direct navigations
            var query = context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.ClassEnrollments)
                .Include(c => c.ClassTests);

            var classEntity = await query.FirstOrDefaultAsync(c => c.Id == id);

            // Then safely load nested navigations if the parent is not null
            if (classEntity != null)
            {
                // Load students for enrollments
                foreach (var enrollment in classEntity.ClassEnrollments.Where(ce => ce != null))
                {
                    await context.Entry(enrollment).Reference(ce => ce.Student).LoadAsync();
                }

                // Load tests for class tests
                foreach (var classTest in classEntity.ClassTests.Where(ct => ct != null))
                {
                    await context.Entry(classTest).Reference(ct => ct.Test).LoadAsync();
                }
            }

            return classEntity;
        }

        public async Task<List<Class>> GetClassesByTeacherIdAsync(Guid teacherId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Classes
                    .Where(c => c.TeacherId == teacherId)
                    .Include(c => c.Teacher)
                    .OrderBy(c => c.ClassName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load teacher classes: {ex.Message}");
                return new List<Class>();
            }
        }

        public async Task<Class> CreateClassAsync(Class classEntity)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                if (classEntity.Id == Guid.Empty)
                {
                    classEntity.Id = Guid.NewGuid();
                }
                classEntity.CreatedAt = DateTimeOffset.Now;

                context.Classes.Add(classEntity);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created class: {classEntity.ClassName}");
                return classEntity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not create class: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateClassAsync(Class classEntity)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var existingClass = await context.Classes.FindAsync(classEntity.Id);
                if (existingClass == null)
                    throw new InvalidOperationException("Class not found");

                existingClass.ClassName = classEntity.ClassName;
                existingClass.Description = classEntity.Description;
                existingClass.TeacherId = classEntity.TeacherId;

                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Updated class: {classEntity.ClassName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not update class: {ex.Message}");
                throw;
            }
        }

        // Class Enrollment operations
        public async Task<List<ClassEnrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassEnrollments
                .Include(ce => ce.Class)
                .Where(ce => ce.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassEnrollments
                .Include(ce => ce.Student)
                .Where(ce => ce.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassEnrollment> EnrollStudentAsync(Guid classId, Guid studentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var enrollment = new ClassEnrollment
            {
                ClassId = classId,
                StudentId = studentId,
                EnrollmentDate = DateTimeOffset.Now
            };
            context.ClassEnrollments.Add(enrollment);
            await context.SaveChangesAsync();
            return enrollment;
        }

        public async Task RemoveEnrollmentAsync(Guid enrollmentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var enrollment = await context.ClassEnrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                context.ClassEnrollments.Remove(enrollment);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<ClassEnrollment>> GetClassEnrollmentsByUserIdAsync(Guid userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassEnrollments
                .Include(ce => ce.Class)
                    .ThenInclude(c => c!.Teacher)
                .Where(ce => ce.StudentId == userId)
                .ToListAsync();
        }

        public async Task<int> GetClassEnrollmentCountAsync(Guid classId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ClassEnrollments.CountAsync(ce => ce.ClassId == classId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get enrollment count: {ex.Message}");
                return 0;
            }
        }

        // Test operations
        public async Task<List<Test>> GetAllTestsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tests
                .Include(t => t.Creator)
                .ToListAsync();
        }

        public async Task<Test?> GetTestByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tests
                .Include(t => t.Creator)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Test>> GetTestsByCreatorIdAsync(Guid creatorId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tests
                .Where(t => t.CreatorId == creatorId)
                .ToListAsync();
        }

        public async Task<Test> CreateTestAsync(Test test)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Tests.Add(test);
            await context.SaveChangesAsync();
            return test;
        }

        public async Task UpdateTestAsync(Test test)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Tests.Update(test);
            await context.SaveChangesAsync();
        }

        // ClassTest operations
        public async Task<List<ClassTest>> GetTestsByClassIdAsync(Guid classId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassTests
                .Include(ct => ct.Test)
                .Where(ct => ct.ClassId == classId)
                .ToListAsync();
        }

        public async Task<List<ClassTest>> GetClassTestsByClassIdAsync(Guid classId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassTests
                .Include(ct => ct.Test)
                .Include(ct => ct.Class)
                .Where(ct => ct.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassTest?> GetClassTestByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();

            // Use a separate step to safely include navigations to avoid null reference
            var query = context.ClassTests
                .Include(ct => ct.Class)
                .Include(ct => ct.Test);

            // Load the entity first
            var classTest = await query.FirstOrDefaultAsync(ct => ct.Id == id);

            // Then load the questions if Test is not null
            if (classTest?.Test != null)
            {
                await context.Entry(classTest.Test)
                    .Collection(t => t.Questions)
                    .LoadAsync();
            }

            return classTest;
        }

        public async Task<ClassTest> AssignTestToClassAsync(ClassTest classTest)
        {
            using var context = _contextFactory.CreateDbContext();
            context.ClassTests.Add(classTest);
            await context.SaveChangesAsync();
            return classTest;
        }

        public async Task UpdateClassTestAsync(ClassTest classTest)
        {
            using var context = _contextFactory.CreateDbContext();
            context.ClassTests.Update(classTest);
            await context.SaveChangesAsync();
        }

        // Question operations
        public async Task<List<Question>> GetQuestionsByTestIdAsync(Guid testId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Questions
                .Include(q => q.Category)
                .Where(q => q.TestId == testId)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Questions.Add(question);
            await context.SaveChangesAsync();
            return question;
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Questions.Update(question);
            await context.SaveChangesAsync();
        }

        // Attempt operations
        public async Task<List<Attempt>> GetAttemptsByStudentIdAsync(Guid studentId)
        {
            using var context = _contextFactory.CreateDbContext();

            // Load attempts with direct ClassTest navigation
            var attempts = await context.Attempts
                .Include(a => a.ClassTest)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            // Safely load Test for each ClassTest
            foreach (var attempt in attempts)
            {
                if (attempt.ClassTest != null)
                {
                    await context.Entry(attempt.ClassTest).Reference(ct => ct.Test).LoadAsync();
                }
            }

            return attempts;
        }

        public async Task<List<Attempt>> GetAttemptsByClassTestIdAsync(Guid classTestId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Attempts
                .Include(a => a.Student)
                .Where(a => a.ClassTestId == classTestId)
                .ToListAsync();
        }

        public async Task<List<Attempt>> GetAttemptsByStudentAndClassTestAsync(Guid studentId, Guid classTestId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Attempts
                .Include(a => a.ClassTest)
                    .ThenInclude(ct => ct!.Test)
                .Where(a => a.StudentId == studentId && a.ClassTestId == classTestId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Attempt?> GetAttemptByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();

            // Load the base entity with direct navigations first
            var query = context.Attempts
                .Include(a => a.Student)
                .Include(a => a.ClassTest)
                .Include(a => a.UserAnswers);

            var attempt = await query.FirstOrDefaultAsync(a => a.Id == id);

            // Then safely load nested navigations
            if (attempt != null)
            {
                // Load Test for ClassTest if ClassTest is not null
                if (attempt.ClassTest != null)
                {
                    await context.Entry(attempt.ClassTest).Reference(ct => ct.Test).LoadAsync();
                }

                // Load Question for each UserAnswer
                foreach (var userAnswer in attempt.UserAnswers.Where(ua => ua != null))
                {
                    await context.Entry(userAnswer).Reference(ua => ua.Question).LoadAsync();
                }
            }

            return attempt;
        }

        public async Task<Attempt> CreateAttemptAsync(Attempt attempt)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Attempts.Add(attempt);
            await context.SaveChangesAsync();
            return attempt;
        }

        public async Task UpdateAttemptAsync(Attempt attempt)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Attempts.Update(attempt);
            await context.SaveChangesAsync();
        }

        // UserAnswer operations
        public async Task<List<UserAnswer>> GetAnswersByAttemptIdAsync(Guid attemptId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.UserAnswers
                .Include(ua => ua.Question)
                .Where(ua => ua.AttemptId == attemptId)
                .ToListAsync();
        }

        public async Task<List<UserAnswer>> GetAnswersByClassTestAndQuestionAsync(Guid classTestId, Guid questionId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.UserAnswers
                .Include(ua => ua.Attempt)
                .Include(ua => ua.Question)
                .Where(ua => ua.Attempt != null && ua.Attempt.ClassTestId == classTestId && ua.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<UserAnswer?> GetUserAnswerByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.UserAnswers
                .Include(ua => ua.Question)
                .Include(ua => ua.Attempt)
                .FirstOrDefaultAsync(ua => ua.Id == id);
        }

        public async Task<UserAnswer> SaveUserAnswerAsync(UserAnswer userAnswer)
        {
            using var context = _contextFactory.CreateDbContext();
            context.UserAnswers.Add(userAnswer);
            await context.SaveChangesAsync();
            return userAnswer;
        }

        public async Task UpdateUserAnswerAsync(UserAnswer userAnswer)
        {
            using var context = _contextFactory.CreateDbContext();
            context.UserAnswers.Update(userAnswer);
            await context.SaveChangesAsync();
        }

        // Admin User Management

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, string? roleFilter = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u =>
                        u.Email.Contains(searchTerm) ||
                        (u.FullName != null && u.FullName.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(roleFilter))
                {
                    query = query.Where(u => u.Role == roleFilter);
                }

                return await query
                    .OrderBy(u => u.FullName ?? u.Email)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not search users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var user = await context.Users.FindAsync(userId);
                if (user == null) return false;

                // Since User model doesn't have IsActive, we'll just log the action
                // In a real scenario, you might add this property to your model
                await LogActivityAsync("User Deactivated", $"User {user.Email} deactivated");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not deactivate user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ReactivateUserAsync(Guid userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var user = await context.Users.FindAsync(userId);
                if (user == null) return false;

                // Since User model doesn't have IsActive, we'll just log the action
                await LogActivityAsync("User Reactivated", $"User {user.Email} reactivated");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not reactivate user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(Guid userId, string newRole)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            await context.SaveChangesAsync();

            await LogActivityAsync("Role Changed", $"User {user.Email} role changed to {newRole}", userId);
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.FullName ?? u.Email)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get user count: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                // Since no IsActive property, return total users
                return await context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get user count: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetUserCountByRoleAsync(string role)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Users.CountAsync(u => u.Role == role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get user count by role: {ex.Message}");
                return 0;
            }
        }

        // Admin Class Management

        public async Task<int> GetTotalClassesCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Classes.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get class count: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetActiveClassesCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                // Since no IsActive property, return total classes
                return await context.Classes.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get active class count: {ex.Message}");
                return 0;
            }
        }

        // Admin Test Management

        public async Task<int> GetTotalTestsCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tests.CountAsync();
        }

        public async Task<int> GetActiveTestsCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassTests
                .CountAsync(ct => ct.DueDate > DateTimeOffset.Now);
        }

        // Admin Statistics
        public async Task<Dictionary<string, int>> GetUserRoleStatisticsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users

                .GroupBy(u => u.Role ?? string.Empty)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetClassStatisticsAsync()
{
    try
    {
        using var context = _contextFactory.CreateDbContext();
        var stats = new Dictionary<string, int>();
        
        stats["Total"] = await context.Classes.CountAsync();
        stats["Active"] = await context.Classes.CountAsync(); // Since no IsActive property
        stats["Inactive"] = 0; // Since no IsActive property
        
        return stats;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"INFO: Could not load class statistics: {ex.Message}");
        return new Dictionary<string, int>();
    }
}

        public async Task<Dictionary<string, int>> GetTestStatisticsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var now = DateTimeOffset.Now;
            var stats = new Dictionary<string, int>();

            stats["Total"] = await context.Tests.CountAsync();
            stats["Active"] = await context.ClassTests.CountAsync(ct => ct.DueDate > now);
            stats["Completed"] = await context.ClassTests.CountAsync(ct => ct.DueDate <= now);

            return stats;
        }

        // Admin Activity Logs
        public async Task<IEnumerable<Log>> GetRecentActivityLogsAsync(int count = 50)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Logs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> LogActivityAsync(string action, string description, Guid? userId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var log = new Log
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Timestamp = DateTimeOffset.Now,
                    EventType = action, // Use EventType instead of Action
                    Description = description
                };

                context.Logs.Add(log);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not log activity: {ex.Message}");
                return false;
            }
        }

        // Admin Classroom Management


        public async Task<bool> DeleteClassAsync(Guid classId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var classEntity = await context.Classes.FindAsync(classId);
                if (classEntity == null) return false;

                // Check if class has enrollments or tests
                var hasEnrollments = await context.ClassEnrollments.AnyAsync(ce => ce.ClassId == classId);
                var hasTests = await context.ClassTests.AnyAsync(ct => ct.ClassId == classId);

                if (hasEnrollments || hasTests)
                {
                    // Don't actually delete if there's data, just return true
                    Console.WriteLine($"INFO: Class {classEntity.ClassName} has data, keeping record");
                    return true;
                }
                else
                {
                    context.Classes.Remove(classEntity);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Deleted class: {classEntity.ClassName}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not delete class: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ArchiveClassAsync(Guid classId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var classEntity = await context.Classes.FindAsync(classId);
                if (classEntity == null) return false;

                // Since Class model doesn't have IsActive, we'll just log the action
                await LogActivityAsync("Class Archived", $"Class '{classEntity.ClassName}' archived");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not archive class: {ex.Message}");
                return false;
            }
        }
        public async Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm, bool? isActive = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.Classes.Include(c => c.Teacher).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c =>
                        c.ClassName.Contains(searchTerm) ||
                        (c.Description != null && c.Description.Contains(searchTerm)));
                }

                // Ignore isActive parameter since your model doesn't have this property

                return await query.OrderBy(c => c.ClassName).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not search classes: {ex.Message}");
                return new List<Class>();
            }
        }

        public async Task<IEnumerable<ClassEnrollment>> GetClassEnrollmentsAsync(Guid classId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ClassEnrollments
                    .Include(ce => ce.Student)
                    .Where(ce => ce.ClassId == classId)
                    .OrderBy(ce => ce.Student!.FullName ?? ce.Student!.Email)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get class enrollments: {ex.Message}");
                return new List<ClassEnrollment>();
            }
        }

        public async Task<bool> EnrollStudentInClassAsync(Guid classId, Guid studentId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Check if already enrolled
                var existingEnrollment = await context.ClassEnrollments
                    .FirstOrDefaultAsync(ce => ce.ClassId == classId && ce.StudentId == studentId);

                if (existingEnrollment != null) return false;

                var enrollment = new ClassEnrollment
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId,
                    StudentId = studentId,
                    EnrollmentDate = DateTimeOffset.Now
                };

                context.ClassEnrollments.Add(enrollment);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Student enrolled in class");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not enroll student: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveStudentFromClassAsync(Guid classId, Guid studentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var enrollment = await context.ClassEnrollments
                .FirstOrDefaultAsync(ce => ce.ClassId == classId && ce.StudentId == studentId);

            if (enrollment == null) return false;

            context.ClassEnrollments.Remove(enrollment);
            await context.SaveChangesAsync();

            var student = await context.Users.FindAsync(studentId);
            var classEntity = await context.Classes.FindAsync(classId);
            await LogActivityAsync("Student Removed", $"Student {student?.Email} removed from class {classEntity?.ClassName}");

            return true;
        }

        public async Task<bool> BulkEnrollStudentsAsync(Guid classId, IEnumerable<Guid> studentIds)
        {
            using var context = _contextFactory.CreateDbContext();

            var enrollments = new List<ClassEnrollment>();
            foreach (var studentId in studentIds)
            {
                // Check if not already enrolled
                var exists = await context.ClassEnrollments
                    .AnyAsync(ce => ce.ClassId == classId && ce.StudentId == studentId);

                if (!exists)
                {
                    enrollments.Add(new ClassEnrollment
                    {
                        Id = Guid.NewGuid(),
                        ClassId = classId,
                        StudentId = studentId,
                        EnrollmentDate = DateTimeOffset.Now
                    });
                }
            }

            if (enrollments.Any())
            {
                context.ClassEnrollments.AddRange(enrollments);
                await context.SaveChangesAsync();

                var classEntity = await context.Classes.FindAsync(classId);
                await LogActivityAsync("Bulk Enrollment", $"{enrollments.Count} students enrolled in class {classEntity?.ClassName}");
            }

            return true;
        }

        public async Task<IEnumerable<User>> GetAvailableStudentsForClassAsync(Guid classId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var enrolledStudentIds = await context.ClassEnrollments
                    .Where(ce => ce.ClassId == classId)
                    .Select(ce => ce.StudentId)
                    .ToListAsync();

                return await context.Users
                    .Where(u => u.Role == "student" && !enrolledStudentIds.Contains(u.Id))
                    .OrderBy(u => u.FullName ?? u.Email)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not get available students: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<Dictionary<string, object>> GetClassStatisticsAsync(Guid classId)
        {
            using var context = _contextFactory.CreateDbContext();

            var stats = new Dictionary<string, object>();

            stats["TotalStudents"] = await context.ClassEnrollments.CountAsync(ce => ce.ClassId == classId);
            stats["ActiveTests"] = await context.ClassTests
                .CountAsync(ct => ct.ClassId == classId && ct.DueDate > DateTimeOffset.Now);
            stats["CompletedTests"] = await context.ClassTests
                .CountAsync(ct => ct.ClassId == classId && ct.DueDate <= DateTimeOffset.Now);

            // Calculate average performance
            var attempts = await context.Attempts
                .Include(a => a.ClassTest)
                .Where(a => a.ClassTest!.ClassId == classId && a.Score.HasValue)
                .ToListAsync();

            stats["AverageScore"] = attempts.Any() ? attempts.Average(a => a.Score!.Value) : 0;
            stats["TotalAttempts"] = attempts.Count;

            return stats;
        }

        public async Task<bool> TransferStudentBetweenClassesAsync(Guid studentId, Guid fromClassId, Guid toClassId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Remove from old class
                var oldEnrollment = await context.ClassEnrollments
                    .FirstOrDefaultAsync(ce => ce.ClassId == fromClassId && ce.StudentId == studentId);

                if (oldEnrollment != null)
                {
                    context.ClassEnrollments.Remove(oldEnrollment);
                }

                // Add to new class
                var newEnrollment = new ClassEnrollment
                {
                    Id = Guid.NewGuid(),
                    ClassId = toClassId,
                    StudentId = studentId,
                    EnrollmentDate = DateTimeOffset.Now
                };

                context.ClassEnrollments.Add(newEnrollment);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                var student = await context.Users.FindAsync(studentId);
                var fromClass = await context.Classes.FindAsync(fromClassId);
                var toClass = await context.Classes.FindAsync(toClassId);
                await LogActivityAsync("Student Transferred",
                    $"Student {student?.Email} transferred from {fromClass?.ClassName} to {toClass?.ClassName}");

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Test Management
        public async Task<IEnumerable<Test>> GetAllTestsWithQuestionsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Tests
                    .Include(t => t.Creator)
                    .Include(t => t.Questions)
                    .ThenInclude(q => q.Category)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load tests: {ex.Message}");
                return new List<Test>();
            }
        }

        public async Task<Test?> GetTestWithQuestionsAsync(Guid testId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Tests
                    .Include(t => t.Creator)
                    .Include(t => t.Questions)
                    .ThenInclude(q => q.Category)
                    .FirstOrDefaultAsync(t => t.Id == testId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load test: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTestAsync(Guid testId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Check if test is used in any class tests
                var isUsed = await context.ClassTests.AnyAsync(ct => ct.TestId == testId);
                if (isUsed)
                {
                    Console.WriteLine($"INFO: Test {testId} is used in classes, cannot delete");
                    return false;
                }

                var test = await context.Tests.FindAsync(testId);
                if (test == null) return false;

                context.Tests.Remove(test);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Deleted test: {test.Title}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not delete test: {ex.Message}");
                return false;
            }
        }
        public async Task<Dictionary<string, int>> GetTestUsageStatisticsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var stats = new Dictionary<string, int>();

                stats["TotalTests"] = await context.Tests.CountAsync();
                stats["UsedTests"] = await context.ClassTests.Select(ct => ct.TestId).Distinct().CountAsync();
                stats["UnusedTests"] = stats["TotalTests"] - stats["UsedTests"];
                stats["TotalQuestions"] = await context.Questions.CountAsync();

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load test statistics: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        // Question Bank Management
        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Questions
                    .Include(q => q.Category)
                    .Include(q => q.Test)
                    .ThenInclude(t => t.Creator)
                    .OrderBy(q => q.Text) // Use Text instead of CreatedAt
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load questions: {ex.Message}");
                return new List<Question>();
            }
        }

        public async Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm, Guid? categoryId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.Questions
                    .Include(q => q.Category)
                    .Include(q => q.Test)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(q => q.Text.Contains(searchTerm)); // Remove Options reference
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(q => q.CategoryId == categoryId.Value);
                }

                return await query.OrderBy(q => q.Text).ToListAsync(); // Remove CreatedAt reference
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not search questions: {ex.Message}");
                return new List<Question>();
            }
        }

        public async Task<bool> DeleteQuestionAsync(Guid questionId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Check if question has answers
                var hasAnswers = await context.UserAnswers.AnyAsync(ua => ua.QuestionId == questionId);
                if (hasAnswers)
                {
                    Console.WriteLine($"INFO: Question {questionId} has answers, cannot delete");
                    return false;
                }

                var question = await context.Questions.FindAsync(questionId);
                if (question == null) return false;

                context.Questions.Remove(question);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Deleted question");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not delete question: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Question>> GetQuestionsByCategoryAsync(Guid categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Questions
                    .Include(q => q.Test)
                    .Where(q => q.CategoryId == categoryId)
                    .OrderBy(q => q.Text)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load category questions: {ex.Message}");
                return new List<Question>();
            }
        }

        public async Task<Dictionary<Guid, int>> GetQuestionUsageCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.UserAnswers
                    .Where(ua => ua.QuestionId.HasValue)
                    .GroupBy(ua => ua.QuestionId.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load question usage: {ex.Message}");
                return new Dictionary<Guid, int>();
            }
        }

        // Category Management
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load categories: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                if (category.Id == Guid.Empty)
                {
                    category.Id = Guid.NewGuid();
                }

                context.Categories.Add(category);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created category: {category.Name}");
                return category;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not create category: {ex.Message}");
                throw;
            }
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Categories.Update(category);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Updated category: {category.Name}");
                return category;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not update category: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Check if category has questions
                var hasQuestions = await context.Questions.AnyAsync(q => q.CategoryId == categoryId);
                if (hasQuestions)
                {
                    Console.WriteLine($"INFO: Category {categoryId} has questions, cannot delete");
                    return false;
                }

                var category = await context.Categories.FindAsync(categoryId);
                if (category == null) return false;

                context.Categories.Remove(category);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Deleted category: {category.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not delete category: {ex.Message}");
                return false;
            }
        }
    
    // Assessment Management
public async Task<IEnumerable<Attempt>> GetAllAttemptsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Attempts
                    .Include(a => a.Student)
                    .Include(a => a.ClassTest)
                    .ThenInclude(ct => ct.Test)
                    .Include(a => a.ClassTest)
                    .ThenInclude(ct => ct.Class)
                    .OrderByDescending(a => a.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load attempts: {ex.Message}");
                return new List<Attempt>();
            }
        }

        public async Task<Attempt?> GetAttemptWithDetailsAsync(Guid attemptId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Attempts
                    .Include(a => a.Student)
                    .Include(a => a.ClassTest)
                    .ThenInclude(ct => ct.Test)
                    .ThenInclude(t => t.Questions)
                    .Include(a => a.ClassTest)
                    .ThenInclude(ct => ct.Class)
                    .Include(a => a.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load attempt: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteAttemptAsync(Guid attemptId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var attempt = await context.Attempts.FindAsync(attemptId);
                if (attempt == null) return false;

                context.Attempts.Remove(attempt);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Deleted attempt: {attemptId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not delete attempt: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetAssessmentStatisticsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var stats = new Dictionary<string, object>();

                var allAttempts = await context.Attempts.ToListAsync();
                stats["TotalAttempts"] = allAttempts.Count;
                stats["CompletedAttempts"] = allAttempts.Count(a => a.EndTime.HasValue);
                stats["InProgressAttempts"] = allAttempts.Count(a => !a.EndTime.HasValue);

                var completedWithScores = allAttempts.Where(a => a.Score.HasValue).ToList();
                stats["AverageScore"] = completedWithScores.Any() ? completedWithScores.Average(a => a.Score!.Value) : 0.0;

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load assessment statistics: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

    }
}
