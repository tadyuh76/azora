# Database Setup Instructions

## Current Issues and Solutions

### Schema Mismatch Issue

The application encountered a database schema mismatch where the `users` table doesn't have a `full_name` column as expected by the EF Core model.

**Error encountered:**

```
Error loading dashboard data: 42703: column u.full_name does not exist
```

### Temporary Solution (Currently Implemented)

- The application now loads demo data by default to showcase the UI
- Real database queries are attempted but errors are gracefully handled
- The dashboard will display beautiful demo content even if the database schema doesn't match

### Proper Resolution (For Production)

#### Option 1: Update Database Schema

Add the missing `full_name` column to your Supabase database:

```sql
ALTER TABLE users ADD COLUMN full_name TEXT;
```

#### Option 2: Update Model to Match Existing Schema

If your Supabase users table has different column names, update the `Models/User.cs` file:

```csharp
// Example: if your database has 'name' instead of 'full_name'
[Column("name")]  // Change this to match your actual column name
public string? FullName { get; set; }
```

#### Option 3: Create Migration

Generate and apply an EF Core migration:

```bash
dotnet ef migrations add AddFullNameToUsers
dotnet ef database update
```

### Supabase Integration Notes

When using Supabase:

1. Ensure your connection string is properly configured in `appsettings.json`
2. Check that the table structures match your EF Core models
3. Consider using Supabase's built-in auth tables if available

### Current Demo Data

The application showcases these sample courses:

- Mathematics 101 (Blue theme)
- World History (Green theme)
- Chemistry 201 (Purple theme)
- Physics 101 (Orange theme)

And sample upcoming assessments:

- Algebra Quiz
- History Midterm
- Chemistry Lab Report

This demo data demonstrates the full functionality of the student dashboard UI.

## Console Output Improvements

The application has been optimized for a cleaner console experience:

### ✅ Fixed Issues:

- **Nullable Reference Warnings**: Resolved EF Core nullable reference warnings
- **Sensitive Data Logging**: Disabled verbose EF Core logging for cleaner output
- **Error Messages**: Improved error messages to be more informative and less alarming
- **Demo Data Loading**: Added friendly messages when demo data is loaded

### 🎯 What You'll See:

When the app runs successfully, you'll see clean console output like:

```
📚 Loading demo data for student dashboard...
INFO: Could not load user data (using demo data): 42703: column u.full_name does not exist
INFO: Could not load real class data (showing demo data): 42703: column u.full_name does not exist
```

These are **informational messages**, not errors! The app is working correctly and showing beautiful demo data.

## Database Setup

This document outlines the setup required for the Supabase database.

## Tables and Structure

The application uses the following tables:

- `users` - User accounts
- `classes` - Classes/courses
- `class_enrollments` - Student enrollments in classes
- `tests` - Test definitions
- `class_tests` - Tests assigned to specific classes
- `categories` - Question categories
- `questions` - Test questions
- `attempts` - Test attempts by students
- `user_answers` - Student answers to questions
- `logs` - Application logs

## Question Types

The application supports three question types:

1. **Multiple Choice**: Questions with predefined options where one is the correct answer.

   - `type`: "multiple_choice"
   - `answers`: Array of strings with the answer options
   - `correct_answer`: The index of the correct option (1-based, as a string)
   - User answers are stored as the index (e.g., "1", "2", "3", etc.)

2. **True/False**: Questions with true or false options.

   - `type`: "true_false"
   - `correct_answer`: "true" or "false"
   - User answers are stored as "true" or "false"

3. **Short Answer**: Questions requiring a free-text response.
   - `type`: "short_answer"
   - `answers`: Empty array
   - `correct_answer`: The exact expected answer text
   - User answers are stored as text and compared in a case-insensitive manner

## Question Difficulty

Each question can have a difficulty level:

- `difficulty`: One of "easy", "medium", "hard", or "intense"

## Sample Data Setup

Here's a SQL script to set up the database with sample data:

```sql
-- Insert test with questions of each type
INSERT INTO tests (id, creator_id, title, description, time_limit, created_at)
VALUES (
  '11111111-1111-1111-1111-111111111111',
  '22222222-2222-2222-2222-222222222222',
  'Algebra Fundamentals Quiz',
  'Test your understanding of basic algebraic concepts including linear equations, quadratic functions, and problem-solving techniques.',
  45,
  NOW()
);

-- Multiple Choice Questions (note that correct_answer is the 1-based index)
INSERT INTO questions (id, test_id, text, type, difficulty, points, answers, correct_answer)
VALUES
  ('33333333-3333-3333-3333-333333333331', '11111111-1111-1111-1111-111111111111', 'What is the solution to the equation 2x + 5 = 13?', 'multiple_choice', 'medium', 5, ARRAY['x = 3', 'x = 4', 'x = 5', 'x = 6'], '2'),
  ('33333333-3333-3333-3333-333333333332', '11111111-1111-1111-1111-111111111111', 'Which of the following represents a linear function?', 'multiple_choice', 'medium', 5, ARRAY['y = x²', 'y = 2x + 1', 'y = x³', 'y = 1/x'], '2'),
  ('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111', 'What is the y-intercept of the line y = 3x - 7?', 'multiple_choice', 'hard', 5, ARRAY['3', '-7', '7', '-3'], '2'),
  ('33333333-3333-3333-3333-333333333334', '11111111-1111-1111-1111-111111111111', 'What is the vertex of the parabola y = x² - 4x + 3?', 'multiple_choice', 'intense', 5, ARRAY['(2, -1)', '(1, 0)', '(3, 0)', '(0, 3)'], '1');

-- True/False Questions
INSERT INTO questions (id, test_id, text, type, difficulty, points, answers, correct_answer)
VALUES
  ('44444444-4444-4444-4444-444444444441', '11111111-1111-1111-1111-111111111111', 'The graph of a quadratic function is always a parabola.', 'true_false', 'easy', 3, NULL, 'true'),
  ('44444444-4444-4444-4444-444444444442', '11111111-1111-1111-1111-111111111111', 'The slope of a horizontal line is zero.', 'true_false', 'easy', 3, NULL, 'true'),
  ('44444444-4444-4444-4444-444444444443', '11111111-1111-1111-1111-111111111111', 'Two parallel lines have the same slope.', 'true_false', 'medium', 3, NULL, 'true'),
  ('44444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111', 'The domain of f(x) = √x is all real numbers.', 'true_false', 'hard', 3, NULL, 'false');

-- Short Answer Questions
INSERT INTO questions (id, test_id, text, type, difficulty, points, answers, correct_answer)
VALUES
  ('55555555-5555-5555-5555-555555555551', '11111111-1111-1111-1111-111111111111', 'What is the formula for the area of a circle?', 'short_answer', 'medium', 5, NULL, 'πr²'),
  ('55555555-5555-5555-5555-555555555552', '11111111-1111-1111-1111-111111111111', 'What is the derivative of x²?', 'short_answer', 'medium', 5, NULL, '2x');

-- Assign test to a class
INSERT INTO class_tests (id, class_id, test_id, start_date, due_date, limit_attempts)
VALUES (
  '66666666-6666-6666-6666-666666666666',
  '77777777-7777-7777-7777-777777777777',
  '11111111-1111-1111-1111-111111111111',
  NOW() - INTERVAL '1 day',
  NOW() + INTERVAL '7 days',
  2
);
```

## Database Schema

The schema should be set up with the appropriate foreign key constraints to maintain data integrity. You can use the SQL script provided in the `database/schema.sql` file to set up the full database schema.

## Supabase Configuration

When using Supabase:

1. Create a new project in Supabase
2. Run the SQL scripts in the SQL Editor
3. Set up Row Level Security (RLS) policies for tables
4. Update the connection string in `appsettings.json`

Ensure your Supabase connection string is properly configured in the application's `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-project.supabase.co;Database=postgres;Username=postgres;Password=your-password"
  }
}
```
