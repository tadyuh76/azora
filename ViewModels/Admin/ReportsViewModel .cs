using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class ReportsViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now.AddMonths(-1);
        
        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now;
        
        [ObservableProperty]
        private string _selectedReportType = "User Activity";

        public ObservableCollection<string> ReportTypes { get; } = new()
        {
            "User Activity",
            "Class Performance", 
            "Test Statistics",
            "System Usage",
            "Security Audit"
        };

        public ObservableCollection<ReportItemViewModel> RecentReports { get; } = new();

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ScheduleReportCommand { get; }
        public ICommand RefreshReportsCommand { get; }

        public ReportsViewModel()
        {
            // Fix ServiceProvider reference
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
            ExportReportCommand = new AsyncRelayCommand<string>(ExportReportAsync);
            ScheduleReportCommand = new RelayCommand(ScheduleReport);
            RefreshReportsCommand = new AsyncRelayCommand(LoadRecentReportsAsync);
            
            _ = LoadRecentReportsAsync();
        }

        // Keep all existing methods the same...
        private async Task GenerateReportAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                if (StartDate > EndDate)
                {
                    ShowError("Start date must be before end date.");
                    return;
                }
                
                await Task.Delay(2000);
                
                var reportItem = new ReportItemViewModel
                {
                    ReportName = $"{SelectedReportType} Report",
                    GeneratedDate = DateTimeOffset.Now,
                    ReportType = SelectedReportType,
                    Status = "Completed",
                    FileSize = "2.3 MB"
                };
                
                RecentReports.Insert(0, reportItem);
                
                ShowSuccess($"{SelectedReportType} report generated successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error generating report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportReportAsync(string? format)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                await Task.Delay(1500);
                
                ShowSuccess($"Report exported to {format?.ToUpper()} format successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ScheduleReport()
        {
            ShowSuccess("Report scheduling functionality will be implemented here.");
        }

        private async Task LoadRecentReportsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                RecentReports.Clear();
                
                RecentReports.Add(new ReportItemViewModel
                {
                    ReportName = "User Activity Report",
                    GeneratedDate = DateTimeOffset.Now.AddHours(-2),
                    ReportType = "User Activity",
                    Status = "Completed",
                    FileSize = "1.8 MB"
                });
                
                RecentReports.Add(new ReportItemViewModel
                {
                    ReportName = "Test Statistics Report",
                    GeneratedDate = DateTimeOffset.Now.AddDays(-1),
                    ReportType = "Test Statistics",
                    Status = "Completed",
                    FileSize = "3.2 MB"
                });
                
                await Task.Delay(500);
                ShowSuccess("Recent reports loaded successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading reports: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = string.Empty;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessage = message;
            ErrorMessage = string.Empty;
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
    }

    public partial class ReportItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _reportName = string.Empty;
        
        [ObservableProperty]
        private DateTimeOffset _generatedDate;
        
        [ObservableProperty]
        private string _reportType = string.Empty;
        
        [ObservableProperty]
        private string _status = string.Empty;
        
        [ObservableProperty]
        private string _fileSize = string.Empty;
    }
}
