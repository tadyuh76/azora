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
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.ToListAsync();
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
            using var context = _contextFactory.CreateDbContext();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Classes
                .Include(c => c.Teacher)
                .ToListAsync();
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Classes
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<Class> CreateClassAsync(Class classEntity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Classes.Add(classEntity);
            await context.SaveChangesAsync();
            return classEntity;
        }

        public async Task UpdateClassAsync(Class classEntity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Classes.Update(classEntity);
            await context.SaveChangesAsync();
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
            using var context = _contextFactory.CreateDbContext();
            return await context.ClassEnrollments
                .CountAsync(ce => ce.ClassId == classId);
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
    }
}