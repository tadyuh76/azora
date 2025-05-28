using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using AvaloniaAzora.ViewModels;
using AvaloniaAzora.Views.Auth;
using AvaloniaAzora.Views;
using AvaloniaAzora.Views.Admin;
using AvaloniaAzora.Views.Teacher;
using AvaloniaAzora.Services;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaAzora.Commands;
using Avalonia.Styling;

namespace AvaloniaAzora;

public partial class App : Application
{
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Explicitly set the application to use the Light theme
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;

            // Initialize services
            ServiceProvider.Initialize();

            // Initialize the database with EF Core
            _ = Task.Run(async () => await EfCoreMigrationManager.InitializeDatabaseAsync());

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Start with the authentication window
            ShowAuthenticationWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowAuthenticationWindow()
    {
        var authWindow = new AuthenticationWindow();
        authWindow.AuthenticationSuccessful += OnAuthenticationSuccessful;
        authWindow.Show();
    }

    private void ShowMainWindow()
    {
        var mainWindow = new AvaloniaAzora.Views.MainWindow();
        var mainViewModel = new MainWindowViewModel();

        // Subscribe to sign out request
        mainViewModel.SignOutRequested += OnSignOutRequested;

        mainWindow.DataContext = mainViewModel;
        _desktop!.MainWindow = mainWindow;
    }

    private async void OnAuthenticationSuccessful(object? sender, System.EventArgs e)
    {
        try
        {
            Console.WriteLine("🚀 Authentication successful, determining user role...");

            // Get the authenticated user ID from Supabase
            var authService = ServiceProvider.GetService<IAuthenticationService>();
            var currentUser = authService.GetCurrentUser();
            Console.WriteLine($"✅ Got current user: {currentUser?.Email}");

            if (currentUser != null && currentUser.Id != null)
            {
                // Convert the user ID to Guid
                if (Guid.TryParse(currentUser.Id, out var userId))
                {
                    Console.WriteLine($"🔑 Authenticated user ID: {userId}");

                    // Ensure user exists in our database first
                    await EnsureUserExistsInDatabase(userId, currentUser);

                    // Get user from database to check role
                    var dataService = ServiceProvider.GetService<IDataService>();
                    var dbUser = await dataService.GetUserByIdAsync(userId);

                    if (dbUser != null)
                    {
                        Console.WriteLine($"👤 User role: {dbUser.Role}");

                        // Route to appropriate dashboard based on role
                        switch (dbUser.Role?.ToLower())
                        {
                            case "admin":
                                ShowAdminDashboard(userId);
                                break;
                            case "teacher":
                                ShowTeacherDashboard(userId);
                                break;
                            case "student":
                            default:
                                ShowStudentDashboard(userId);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ User not found in database, defaulting to student dashboard");
                        ShowStudentDashboard(userId);
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Invalid user ID format: {currentUser.Id}");
                    ShowAuthenticationWindow();
                }
            }
            else
            {
                Console.WriteLine("❌ No authenticated user found");
                ShowAuthenticationWindow();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in authentication success handler: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            ShowAuthenticationWindow();
        }
        finally
        {
            // Hide authentication window
            if (sender is Window authWindow)
                authWindow.Close();
        }
    }

    private void ShowStudentDashboard(Guid userId)
    {
        try
        {
            Console.WriteLine("📱 Creating student dashboard window...");
            var dashboard = new StudentDashboardWindow(userId);

            // Set this as the main window to prevent app from closing
            _desktop!.MainWindow = dashboard;

            dashboard.Closed += (s, e) =>
            {
                Console.WriteLine("🚪 Student dashboard closed, showing auth window...");
                ShowAuthenticationWindow();
            };

            Console.WriteLine("🎯 Showing student dashboard...");
            dashboard.Show();
            Console.WriteLine("✅ Student dashboard should now be visible");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing student dashboard: {ex.Message}");
            ShowAuthenticationWindow();
        }
    }

    private void ShowTeacherDashboard(Guid userId)
    {
        try
        {
            Console.WriteLine("📱 Creating teacher dashboard window...");
            var dashboard = new TeacherDashboardWindow(userId);

            // Set this as the main window to prevent app from closing
            _desktop!.MainWindow = dashboard;

            dashboard.Closed += (s, e) =>
            {
                Console.WriteLine("🚪 Teacher dashboard closed, showing auth window...");
                ShowAuthenticationWindow();
            };

            Console.WriteLine("🎯 Showing teacher dashboard...");
            dashboard.Show();
            Console.WriteLine("✅ Teacher dashboard should now be visible");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing teacher dashboard: {ex.Message}");
            ShowAuthenticationWindow();
        }
    }

    private void ShowAdminDashboard(Guid userId)
    {
        try
        {
            Console.WriteLine("📱 Creating admin dashboard window...");
            var dashboard = new AdminDashboardWindow(userId);

            // Set this as the main window to prevent app from closing
            _desktop!.MainWindow = dashboard;

            dashboard.Closed += (s, e) =>
            {
                Console.WriteLine("🚪 Admin dashboard closed, showing auth window...");
                ShowAuthenticationWindow();
            };

            Console.WriteLine("🎯 Showing admin dashboard...");
            dashboard.Show();
            Console.WriteLine("✅ Admin dashboard should now be visible");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing admin dashboard: {ex.Message}");
            ShowAuthenticationWindow();
        }
    }

    private void OnSignOutRequested(object? sender, System.EventArgs e)
    {
        // Close main window and show authentication window
        _desktop!.MainWindow?.Close();
        ShowAuthenticationWindow();
    }

    private async Task EnsureUserExistsInDatabase(Guid userId, Supabase.Gotrue.User authUser)
    {
        try
        {
            Console.WriteLine("📊 Getting data service...");
            var dataService = ServiceProvider.GetService<IDataService>();

            Console.WriteLine($"🔍 Checking if user {userId} exists...");
            var existingUser = await dataService.GetUserByIdAsync(userId);

            if (existingUser == null)
            {
                Console.WriteLine($"👤 Creating user record in database for: {authUser.Email}");

                // Create user record in our database
                var newUser = new AvaloniaAzora.Models.User
                {
                    Id = userId,
                    Email = authUser.Email ?? "unknown@example.com",
                    FullName = GetFullNameFromMetadata(authUser),
                    Role = "student" // Default role
                };

                Console.WriteLine("💾 Saving new user to database...");
                await dataService.CreateUserAsync(newUser);
                Console.WriteLine($"✅ User created successfully: {newUser.Email}");
            }
            else
            {
                Console.WriteLine($"✅ User already exists in database: {existingUser.Email}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to ensure user exists in database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Don't throw - dashboard can still work without this
        }
    }

    private string? GetFullNameFromMetadata(Supabase.Gotrue.User authUser)
    {
        try
        {
            if (authUser.UserMetadata?.ContainsKey("full_name") == true)
            {
                return authUser.UserMetadata["full_name"]?.ToString();
            }

            // Try other common metadata keys
            if (authUser.UserMetadata?.ContainsKey("name") == true)
            {
                return authUser.UserMetadata["name"]?.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}