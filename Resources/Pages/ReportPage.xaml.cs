using Firebase.Auth;
using Leux.Resources.Models;
using Leux.Services;
using System.Diagnostics;

namespace Leux;

public partial class ReportPage : ContentPage
{
    private readonly IReportService _reportService;
    private readonly FirebaseAuthClient _authClient;

    public ReportPage(FirebaseAuthClient authClient)
    {
        InitializeComponent();
        // LoadUserReport();
    }

    private async void LoadUserReport()
    {
        Debug.WriteLine($"Loading User Report");
        try
        {
            string currentUserId = _authClient?.User?.Uid;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                await DisplayAlert("Not Logged In", "You must be logged in to view your report.", "OK");
                return;
            }

            var report = await _reportService.GetReportDataAsync(currentUserId);

            if (report == null)
            {
                await DisplayAlert("Error", "Failed to load report data.", "OK");
                return;
            }

            LoadReport(report);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading report: {ex.Message}");
            await DisplayAlert("Error", "An error occurred while loading the report.", "OK");
        }
    }

    private void LoadReport(ReportData report)
    {
        // Weekly
        WeeklyTitle.Text = report.WeeklyTitle;
        WeeklySpent.Text = $"${report.WeeklySpent:F2}";
        WeeklyTx.Text = report.WeeklyTx.ToString();
        WeeklyAvg.Text = $"${report.WeeklyAvg:F2}";
        WeeklyTopCat.Text = report.WeeklyTopCat;
        // Monthly
        MonthlyTitle.Text = report.MonthlyTitle;
        MonthlySpent.Text = $"${report.MonthlySpent:F2}";
        MonthlyTx.Text = report.MonthlyTx.ToString();
        MonthlyAvg.Text = $"${report.MonthlyAvg:F2}";
        MonthlyBudgetUsed.Text = report.MonthlyBudgetUsed;
    }
}
