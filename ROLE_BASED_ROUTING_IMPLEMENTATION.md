# Role-Based Authentication & Dashboard Routing Implementation

## Overview

This implementation adds role-based authentication and routing to the AvaloniaAzora desktop application. When users sign in, they are automatically routed to different dashboards based on their role stored in the database.

## Implementation Details

### 1. Database Structure

- **User Model**: The `User` model has a `Role` field that stores the user's role as a string
- **Supported Roles**:
  - `"student"` (default)
  - `"teacher"`
  - `"admin"`

### 2. Role-Based Routing Logic

Location: `App.axaml.cs` → `OnAuthenticationSuccessful` method

**Flow**:

1. Get authenticated user from Supabase
2. Ensure user exists in local database (create if needed with default "student" role)
3. Query user's role from database
4. Route to appropriate dashboard:
   - `"admin"` → `ShowAdminDashboard()`
   - `"teacher"` → `ShowTeacherDashboard()`
   - `"student"` (default) → `ShowStudentDashboard()`

### 3. Dashboard Windows Created

#### Admin Dashboard (`Views/Admin/AdminDashboardWindow.axaml`)

- **Features**: System management cards, user statistics, recent activity feed
- **UI Elements**:
  - Statistics cards (Users, Classes, Tests, System Status)
  - Management actions (User Management, Class Management, System Settings, Reports)
  - Recent activity sidebar
- **ViewModel**: `AdminDashboardViewModel` - Basic user loading and welcome message

#### Teacher Dashboard (`Views/Teacher/TeacherDashboardWindow.axaml`)

- **Features**: Classroom management, student statistics, teaching tools
- **UI Elements**:
  - Statistics cards (Active Classrooms, Total Students, Active Tests, Avg Performance)
  - Classroom cards grid with student/test counts
  - Quick actions (Create Test, Create Classroom, View Reports)
  - Recent activity feed
- **ViewModel**: `TeacherDashboardViewModel` - Full classroom loading, statistics calculation
- **Supporting ViewModels**:
  - `TeacherClassroomCardViewModel` - Individual classroom cards
  - `RecentActivityViewModel` - Activity feed items

#### Student Dashboard (Existing)

- Maintains existing functionality for students

### 4. ViewModels Implementation

#### AdminDashboardViewModel

```csharp
- LoadDashboardDataAsync(Guid userId)
- LoadUserDataAsync(Guid userId)
- WelcomeMessage property with user's name
```

#### TeacherDashboardViewModel

```csharp
- LoadDashboardDataAsync(Guid userId)
- LoadUserDataAsync(Guid userId)
- LoadTeachingClassesAsync(Guid userId)
- LoadStatisticsAsync(Guid userId)
- LoadRecentActivitiesAsync(Guid userId)
- Properties: TeachingClasses, Statistics counts, RecentActivities
```

### 5. Key Features

#### Dynamic User Creation

- If authenticated user doesn't exist in local database, automatically creates user record
- Uses Supabase user metadata for full name when available
- Assigns default "student" role to new users

#### Error Handling

- Graceful fallback to student dashboard if role determination fails
- Console logging for debugging authentication flow
- Try-catch blocks around all dashboard creation

#### Window Management

- Each dashboard becomes the main window to prevent app closure
- Proper cleanup when dashboard windows close
- Returns to authentication window on dashboard exit

## Testing Instructions

### 1. Test Default Student Flow

1. Run the application: `dotnet run`
2. Sign up/in with a new email
3. Should automatically route to student dashboard (default role)

### 2. Test Teacher Role

1. Note the email you used to sign in
2. Stop the application
3. Update user role: Use the helper script or manually update database
4. Restart application and sign in with same email
5. Should route to teacher dashboard

### 3. Test Admin Role

1. Update a user's role to "admin" in database
2. Sign in with that user
3. Should route to admin dashboard

### 4. Helper Script for Role Updates

A temporary helper script `SetUserRole.cs` is provided to easily update user roles:

```bash
# This is conceptual - you'd need to run it as a separate console app
dotnet run SetUserRole.cs user@example.com teacher
dotnet run SetUserRole.cs user@example.com admin
```

## Technical Notes

### XAML Parsing Issues Resolved

- Removed all emojis and special characters that caused XML parsing errors
- Replaced with simple text labels (e.g., "USERS", "CLASS", "SET", "RPT")
- Fixed ampersand encoding issues in XAML

### Constructor Warnings

- Avalonia shows warnings about missing parameterless constructors
- These are safe to ignore since we're using parameterized constructors with userId
- Dashboards are only created programmatically, not through XAML loader

### Performance Considerations

- Dashboard data loading is asynchronous
- Statistics are calculated on-demand from database
- Classroom cards are efficiently bound through ItemsControl

## Future Enhancements

1. **Permissions System**: Implement granular permissions within roles
2. **Role Management UI**: Admin interface to change user roles
3. **Activity Logging**: Real database-driven activity feeds
4. **Performance Optimization**: Cache frequently accessed data
5. **Role Validation**: Add role validation middleware/services

## Files Modified/Created

### New Files

- `Views/Admin/AdminDashboardWindow.axaml`
- `Views/Admin/AdminDashboardWindow.axaml.cs`
- `ViewModels/Admin/AdminDashboardViewModel.cs`
- `Views/Teacher/TeacherDashboardWindow.axaml`
- `Views/Teacher/TeacherDashboardWindow.axaml.cs`
- `ViewModels/Teacher/TeacherDashboardViewModel.cs`
- `ViewModels/Teacher/TeacherClassroomCardViewModel.cs`
- `ViewModels/Teacher/RecentActivityViewModel.cs`
- `SetUserRole.cs` (helper script)

### Modified Files

- `App.axaml.cs` - Added role-based routing logic

## Build Status

✅ **Project builds successfully** with no errors, only minor warnings about constructor visibility which are expected and safe to ignore.

The role-based authentication and dashboard routing system is now fully implemented and ready for testing!
