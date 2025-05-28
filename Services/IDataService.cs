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

        // Admin User Management

        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, string? roleFilter = null);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> ReactivateUserAsync(Guid userId);
        Task<bool> ChangeUserRoleAsync(Guid userId, string newRole);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetUserCountByRoleAsync(string role);

        // Admin Class Management

        Task<int> GetTotalClassesCountAsync();
        Task<int> GetActiveClassesCountAsync();

        // Admin Test Management

        Task<int> GetTotalTestsCountAsync();
        Task<int> GetActiveTestsCountAsync();

        // Admin Statistics
        Task<Dictionary<string, int>> GetUserRoleStatisticsAsync();
        Task<Dictionary<string, int>> GetClassStatisticsAsync();
        Task<Dictionary<string, int>> GetTestStatisticsAsync();

        // Admin Activity Logs
        Task<IEnumerable<Log>> GetRecentActivityLogsAsync(int count = 50);
        Task<bool> LogActivityAsync(string action, string description, Guid? userId = null);

        // Admin Classroom Management
        Task<bool> DeleteClassAsync(Guid classId);
        Task<bool> ArchiveClassAsync(Guid classId);
        Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm, bool? isActive = null);
        Task<IEnumerable<ClassEnrollment>> GetClassEnrollmentsAsync(Guid classId);
        Task<bool> EnrollStudentInClassAsync(Guid classId, Guid studentId);
        Task<bool> RemoveStudentFromClassAsync(Guid classId, Guid studentId);
        Task<bool> BulkEnrollStudentsAsync(Guid classId, IEnumerable<Guid> studentIds);
        Task<IEnumerable<User>> GetAvailableStudentsForClassAsync(Guid classId);
        Task<Dictionary<string, object>> GetClassStatisticsAsync(Guid classId);
        Task<bool> TransferStudentBetweenClassesAsync(Guid studentId, Guid fromClassId, Guid toClassId);
    
    
    // Test Management - Add these missing methods
        Task<IEnumerable<Test>> GetAllTestsWithQuestionsAsync();
        Task<Test?> GetTestWithQuestionsAsync(Guid testId);
        Task<bool> DeleteTestAsync(Guid testId);
        Task<Dictionary<string, int>> GetTestUsageStatisticsAsync();

        // Question Bank Management - Add these missing methods  
        Task<IEnumerable<Question>> GetAllQuestionsAsync();
        Task<IEnumerable<Question>> SearchQuestionsAsync(string searchTerm, Guid? categoryId = null);
        Task<bool> DeleteQuestionAsync(Guid questionId);
        Task<IEnumerable<Question>> GetQuestionsByCategoryAsync(Guid categoryId);
        Task<Dictionary<Guid, int>> GetQuestionUsageCountAsync();

        // Category Management - Add these missing methods
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Guid categoryId);

        // Assessment Management - Add these missing methods
        Task<IEnumerable<Attempt>> GetAllAttemptsAsync();
        Task<Attempt?> GetAttemptWithDetailsAsync(Guid attemptId);
        Task<bool> DeleteAttemptAsync(Guid attemptId);
        Task<Dictionary<string, object>> GetAssessmentStatisticsAsync();
    }
}