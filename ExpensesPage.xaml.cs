using System.Collections.ObjectModel;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;

namespace Leux;

public partial class ExpensesPage : ContentPage
{
    public ObservableCollection<ExpenseItem> AllExpenses { get; } = new();

    private Firebase.Auth.FirebaseAuthClient? Auth =>
        AppServiceHelper.Services.GetService<Firebase.Auth.FirebaseAuthClient>();

    public ExpensesPage()
    {
        InitializeComponent();
        BindingContext = this;

        // default filter values
        CategoryFilter.SelectedIndex = 0; 
        RangeFilter.SelectedIndex = 1;    
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadExpensesAsync();
    }

    async Task LoadExpensesAsync()
    {
        try
        {
            if (!FirestoreDatabase.IsInitialized)
                return; 

            var uid = Auth?.User?.Uid ?? "demo-user-1";
            var db = FirestoreDatabase.Database;

            // Build time range
            var (startUtc, endUtc) = GetRangeUtc(RangeFilter.SelectedIndex);
            Query q = db.Collection("users").Document(uid).Collection("entries");

            if (startUtc != null && endUtc != null)
            {
                q = q.WhereGreaterThanOrEqualTo("occurredAt",
                        Timestamp.FromDateTime(DateTime.SpecifyKind(startUtc.Value, DateTimeKind.Utc)))
                     .WhereLessThan("occurredAt",
                        Timestamp.FromDateTime(DateTime.SpecifyKind(endUtc.Value, DateTimeKind.Utc)));
            }

            q = q.OrderByDescending("occurredAt");

            var snap = await q.GetSnapshotAsync();

            // Category filter (client-side; Firestore compound indexes would be needed otherwise)
            string selectedCategory = CategoryFilter.SelectedItem?.ToString() ?? "All Categories";
            bool filterByCategory = !string.IsNullOrWhiteSpace(selectedCategory) &&
                                    selectedCategory != "All Categories";

            AllExpenses.Clear();
            foreach (var doc in snap.Documents)
            {
                var d = doc.ToDictionary();

                var item = new ExpenseItem
                {
                    Id = doc.Id,
                    Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                    Description = d.TryGetValue("description", out var ds) ? (string)ds : "",
                    Amount = ToDouble(d.TryGetValue("amount", out var a) ? a : 0d),
                    OccurredAt = d.TryGetValue("occurredAt", out var t) && t is Timestamp ts
                                 ? ts.ToDateTime().ToLocalTime()
                                 : DateTime.Now
                };

                if (!filterByCategory || item.Category == selectedCategory)
                    AllExpenses.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Expenses", $"Failed to load expenses.\n{ex.Message}", "OK");
        }
    }

    // Filters
    private async void OnFilterChanged(object sender, EventArgs e) => await LoadExpensesAsync();

    private (DateTime? startUtc, DateTime? endUtc) GetRangeUtc(int rangeIndex)
    {
        // 0: last 7d, 1: last 30d, 2: last 90d, 3: all time
        if (rangeIndex == 3) return (null, null);

        int days = rangeIndex switch
        {
            0 => 7,
            1 => 30,
            2 => 90,
            _ => 30
        };

        var tz = TimeZoneInfo.Local;
        var endLocal = DateTime.Today.AddDays(1);
        var startLocal = endLocal.AddDays(-days);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
        return (startUtc, endUtc);
    }

    // UI-only edit/delete
    private async void OnEditClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ExpenseItem item)
            await DisplayAlert("Edit", $"UI-only edit for: {item.Description}", "OK");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ExpenseItem item)
        {
            bool confirm = await DisplayAlert("Delete", $"Delete '{item.Description}'?", "Yes", "No");
            if (confirm) AllExpenses.Remove(item);
        }
    }

    // Top actions / navigation
    private void OnLogout(object sender, EventArgs e) =>
        Application.Current.MainPage = new LoginPage();

    private void OnTabDashboard(object sender, EventArgs e) =>
        Application.Current.MainPage = new DashboardPage();
    private void OnTabExpenses(object sender, EventArgs e) { /* already here */ }
    private void OnTabBudget(object sender, EventArgs e) { /* TODO */ }
    private void OnTabReports(object sender, EventArgs e) =>
        Application.Current.MainPage = new ReportPage();
    private void OnTabProfile(object sender, EventArgs e) =>
        Application.Current.MainPage = new ProfilePage();

    private static double ToDouble(object v)
    {
        if (v is double d) return d;
        if (v is long l) return l;
        if (v is int i) return i;
        return double.TryParse(v?.ToString(), out var x) ? x : 0d;
    }
}
