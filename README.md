# Azora - Educational Management System

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![AvaloniaUI](https://img.shields.io/badge/AvaloniaUI-11.3.0-blue)
![Platforms](https://img.shields.io/badge/Platforms-macOS%20%7C%20Windows-red)

Azora is a comprehensive educational management desktop application built with AvaloniaUI and .NET 8. It provides role-based dashboards for students, teachers, and administrators to manage classes, tests, and educational content with real-time data visualization and analytics.

## 🌟 Features

### Multi-Role Dashboard System

- **Student Dashboard**: View enrolled classes, take tests, track progress, and review results
- **Teacher Dashboard**: Manage classrooms, create tests, monitor student performance, and view analytics
- **Admin Dashboard**: System administration, user management, and comprehensive reporting

### Test Management System

- Support for multiple question types:
  - Multiple Choice
  - Short Answer
- Difficulty levels: Easy, Medium, Hard, Intense
- Time-limited test sessions
- Attempt limits and restrictions
- Real-time test taking interface

### Class Management

- Classroom creation and enrollment
- Class-specific test assignments
- Student progress tracking
- Performance analytics and visualizations

### Advanced Features

- **Real-time Data Visualization**: Charts and graphs using ScottPlot
- **Role-Based Authentication**: Secure login with automatic role detection
- **Database Integration**: PostgreSQL (Supabase) with Entity Framework Core
- **Supabase Integration**: Cloud authentication and data synchronization
- **Demo Mode**: Graceful fallback with sample data for showcasing

## 🚀 Technology Stack

- **Frontend**: AvaloniaUI 11.3.0 with MVVM pattern
- **Backend**: .NET 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: Supabase Auth
- **Cloud Services**: Supabase
- **Data Visualization**: ScottPlot.Avalonia
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Architecture**: MVVM with CommunityToolkit.Mvvm

## 📋 Prerequisites

- .NET 8.0 SDK
- PostgreSQL (or Supabase account)
- Visual Studio 2022 / VS Code

## ⚡ Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/tadyuh76/azora.git
cd azora
```

### 2. Configure Database

Update `appsettings.json` with your database connection:

```json
{
  "Supabase": {
    "Url": "your-supabase-url",
    "AnonKey": "your-anon-key",
    "ConnectionString": "your-postgresql-connection-string"
  }
}
```

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Database Setup

```bash
# Apply migrations
dotnet ef database update

# Or follow the detailed setup guide
# See DATABASE_SETUP.md for complete instructions
```

### 5. Run the Application

```bash
dotnet run
```

## 🏗️ Project Structure

```
AvaloniaAzora/
├── Models/                 # Data models and entities
│   ├── User.cs
│   ├── Class.cs
│   ├── Test.cs
│   ├── Question.cs
│   └── ...
├── Views/                  # UI Views (AXAML files)
│   ├── Auth/              # Authentication views
│   ├── Student/           # Student dashboard and features
│   ├── Teacher/           # Teacher dashboard and features
│   ├── Admin/             # Admin dashboard and features
│   └── MainWindow.axaml
├── ViewModels/            # MVVM ViewModels
│   ├── Auth/
│   ├── Student/
│   ├── Teacher/
│   ├── Admin/
│   └── ...
├── Services/              # Business logic and data services
│   ├── EfCoreDataService.cs
│   ├── SupabaseAuthenticationService.cs
│   ├── GroqApiService.cs
│   └── ...
├── Data/                  # Entity Framework context
├── Commands/              # Command implementations
├── Styles/                # UI styling and themes
└── Assets/                # Static resources
```

## 🎯 User Roles & Permissions

### Student

- View enrolled classes
- Take assigned tests
- Review test results and feedback
- Track personal progress and statistics

### Teacher

- Create and manage classrooms
- Design and assign tests
- Monitor student performance
- Access detailed analytics and reports
- Manage class enrollments

### Administrator

- System-wide user management
- Access all classes and tests
- Generate comprehensive reports
- Monitor system health and usage
- Configure system settings

## 🛠️ Development

### Adding New Features

1. Create models in `Models/`
2. Update the database context in `Data/`
3. Add migration: `dotnet ef migrations add FeatureName`
4. Create ViewModels in `ViewModels/`
5. Design Views in `Views/`
6. Implement services in `Services/`

### Code Architecture

- **MVVM Pattern**: Clean separation of concerns
- **Dependency Injection**: Configured in `ServiceProvider.cs`
- **Entity Framework**: Database-first approach 
- **Async/Await**: Throughout for responsive UI

## 🆘 Troubleshooting

### Common Issues

**Database Connection Errors**

- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Ensure database exists and migrations are applied

**Authentication Issues**

- Verify Supabase configuration
- Check API keys and URLs
- Ensure proper user roles are set

**UI Issues**

- Clear bin/obj folders: `dotnet clean`
- Rebuild: `dotnet build`
- Check for AXAML syntax errors

### Error Messages

The application provides informative error messages and graceful fallbacks. Check the console output for detailed debugging information.

## 🎨 Screenshots
<img width="1440" alt="image" src="https://github.com/user-attachments/assets/900ba941-f241-40ff-be53-0c865df9e8ec" />
<img width="1440" alt="image" src="https://github.com/user-attachments/assets/85a395a5-2bd2-4e62-a989-f438c7f8d03a" />
<img width="1440" alt="image" src="https://github.com/user-attachments/assets/54318b1b-759c-4030-8c60-83ae08087cd6" />

---

**Azora** - Empowering education through technology 🎓

Made with ❤️ by tadyuh76, ArthurHoang15, and thuanquang - DH49SE0001 - UEH
