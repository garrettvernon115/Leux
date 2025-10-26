using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Models;
using Leux.Services;
using System.Diagnostics;
//using static Leux.ReportPage;
namespace Leux;

public class ReportData
{
    public string WeeklyTitle { get; set; }
    public double WeeklySpent { get; set; }
    public int WeeklyTx { get; set; }
    public double WeeklyAvg { get; set; }
    public string WeeklyTopCat { get; set; }
    public string BudgetUsed { get; set; }

    public string MonthlyTitle { get; set; }
    public double MonthlySpent { get; set; }
    public int MonthlyTx { get; set; }
    public double MonthlyAvg { get; set; }
    public string MonthlyBudgetUsed { get; set; }

    public List<CategoryData> Categories { get; set; }
}

public class CategoryData
{
    public string Name { get; set; }
    public double Amount { get; set; }
}




public partial class ReportPage : ContentPage
{
    private readonly IReportService _reportService;
    private readonly FirebaseAuthClient _authClient;
    public ReportPage(IReportService reportService, FirebaseAuthClient authClient)
    {
        InitializeComponent();
        _reportService = reportService;
        _authClient = authClient;
        LoadUserReport();
    }
    
    private async void LoadUserReport()
    {
        try
        {
            string currentUserId = _authClient?.User?.Uid;
            if(string.IsNullOrWhiteSpace(currentUserId))
            {
                await DisplayAlert("Not Logged In", "You must be logged in to view your report.", "OK");
                return;
            }

            var report = await _reportService.GetReportDataAsync(currentUserId);

            if(report == null)
            {
                await DisplayAlert("Error", "Failed to load report data.", "OK");
                return;
            }

            LoadReport(report);
            BuildCategoryBreakdown(report.Categories);
        }
        catch(Exception ex)
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

    private void BuildCategoryBreakdown(List<CategoryData> cats)
    {
        // Category color palette
        Color GetColor(string name) => name switch
        {
            "Food & Drink" => Color.FromArgb("#6A5ACD"), // purple
            "Shopping" => Color.FromArgb("#4CAF50"), // green
            "Entertainment" => Color.FromArgb("#2196F3"), // blue
            "Transport" => Color.FromArgb("#FF9800"), // orange
            _ => Color.FromArgb("#9E9E9E"),
        };
        CategoryStack.Children.Clear();
        double max = cats.Max(c => c.Amount);
        foreach (var c in cats)
        {
            double pct = max > 0 ? c.Amount / max : 0;
            // 3 columns: Category | Bar | Amount
            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12,
                Padding = new Thickness(0, 2)
            };
            // Category name
            row.Add(new Label
            {
                Text = c.Name,
                VerticalTextAlignment = TextAlignment.Center
            }, 0, 0);
            // Replace CornerRadius (Grid can't use it) → Frame for rounded look
            var track = new Frame
            {
                HeightRequest = 10,
                BackgroundColor = new Color(1f, 1f, 1f, 0.12f),
                VerticalOptions = LayoutOptions.Center,
                CornerRadius = 5,
                Padding = 0,
                HasShadow = false
            };
            // Bar fill (ProgressBar with proper float Progress)
            var bar = new ProgressBar
            {
                Progress = (float)Math.Clamp(pct, 0, 1), // 👈 cast to float
                HeightRequest = 10,
                VerticalOptions = LayoutOptions.Center,
#if ANDROID || WINDOWS || MACCATALYST || IOS
                ProgressColor = GetColor(c.Name)
#endif
            };

            track.Content = bar;
            row.Add(track, 1, 0);

            row.Add(new Label
            {
                Text = $"${c.Amount:F2}",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End
            }, 2, 0);
            CategoryStack.Children.Add(row);
        }
    }

    //void LoadDemo()
    //{
    //    // Weekly
    //    WeeklyTitle.Text = "Weekly Report (Sep 15–21, 2025)";
    //    WeeklySpent.Text = "$234.56";
    //    WeeklyTx.Text = "12";
    //    WeeklyAvg.Text = "$33.51";
    //    WeeklyTopCat.Text = "Food & Drink";

    //    // Monthly
    //    MonthlyTitle.Text = "Monthly Report (September 2025)";
    //    MonthlySpent.Text = "$1,123.45";
    //    MonthlyTx.Text = "45";
    //    MonthlyAvg.Text = "$53.50";
    //    MonthlyBudgetUsed.Text = "75%";
    //}

    
}
