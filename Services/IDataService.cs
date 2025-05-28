using AvaloniaAzora.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaAzora.Services
{
    public interface IDataService
    {
        // User operations
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);

        // Class operations
        Task<List<Class>> GetAllClassesAsync();
        Task<Class?> GetClassByIdAsync(Guid id);
        Task<List<Class>> GetClassesByTeacherIdAsync(Guid teacherId);
        Task<Class> CreateClassAsync(Class classEntity);
        Task UpdateClassAsync(Class classEntity);

        // Class Enrollment operations
        Task<List<ClassEnrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId);
        Task<List<ClassEnrollment>> GetEnrollmentsByClassIdAsync(Guid classId);
        Task<List<ClassEnrollment>> GetClassEnrollmentsByUserIdAsync(Guid userId);
        Task<int> GetClassEnrollmentCountAsync(Guid classId);
        Task<ClassEnrollment> EnrollStudentAsync(Guid classId, Guid studentId);
        Task RemoveEnrollmentAsync(Guid enrollmentId);

        // Test operations
        Task<List<Test>> GetAllTestsAsync();
        Task<Test?> GetTestByIdAsync(Guid id);
        Task<List<Test>> GetTestsByCreatorIdAsync(Guid creatorId);
        Task<Test> CreateTestAsync(Test test);
        Task UpdateTestAsync(Test test);

        // ClassTest operations
        Task<List<ClassTest>> GetTestsByClassIdAsync(Guid classId);
        Task<List<ClassTest>> GetClassTestsByClassIdAsync(Guid classId);
        Task<ClassTest?> GetClassTestByIdAsync(Guid id);
        Task<ClassTest> AssignTestToClassAsync(ClassTest classTest);
        Task UpdateClassTestAsync(ClassTest classTest);
        Task RemoveClassTestAsync(Guid classTestId);

        // Question operations
        Task<List<Question>> GetQuestionsByTestIdAsync(Guid testId);
        Task<Question?> GetQuestionByIdAsync(Guid id);
        Task<Question> CreateQuestionAsync(Question question);
        Task UpdateQuestionAsync(Question question);

        // Attempt operations
        Task<List<Attempt>> GetAttemptsByStudentIdAsync(Guid studentId);
        Task<List<Attempt>> GetAttemptsByClassTestIdAsync(Guid classTestId);
        Task<List<Attempt>> GetAttemptsByStudentAndClassTestAsync(Guid studentId, Guid classTestId);
        Task<Attempt?> GetAttemptByIdAsync(Guid id);
        Task<Attempt> CreateAttemptAsync(Attempt attempt);
        Task UpdateAttemptAsync(Attempt attempt);

        // UserAnswer operations
        Task<List<UserAnswer>> GetAnswersByAttemptIdAsync(Guid attemptId);
        Task<List<UserAnswer>> GetAnswersByClassTestAndQuestionAsync(Guid classTestId, Guid questionId);
        Task<UserAnswer?> GetUserAnswerByIdAsync(Guid id);
        Task<UserAnswer> SaveUserAnswerAsync(UserAnswer userAnswer);
        Task UpdateUserAnswerAsync(UserAnswer userAnswer);
    }
}