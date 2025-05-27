using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AvaloniaAzora.ViewModels;
using AvaloniaAzora.Views.Auth;
using AvaloniaAzora.Views;
using AvaloniaAzora.Services;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaloniaAzora;

public partial class App : Application
{
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;

            // Initialize services
            ServiceProvider.Initialize();

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

    private void OnAuthenticationSuccessful(object? sender, System.EventArgs e)
    {
        // Show dashboard first
        ShowStudentDashboard();

        // Hide authentication window (don't close, so app stays alive)
        if (sender is Window authWindow)
            authWindow.Close();
    }

    private void ShowStudentDashboard()
    {
        var dashboard = new StudentDashboardWindow();
        dashboard.Closed += (s, e) =>
        {
            // When dashboard closes, show auth window again
            ShowAuthenticationWindow();
        };
        dashboard.Show();
    }

    private void OnSignOutRequested(object? sender, System.EventArgs e)
    {
        // Close main window and show authentication window
        _desktop!.MainWindow?.Close();
        ShowAuthenticationWindow();
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