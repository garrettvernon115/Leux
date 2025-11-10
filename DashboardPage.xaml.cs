using System.Collections.ObjectModel;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;

namespace Leux;

public partial class DashboardPage : ContentPage
{
    // Observable collection for the UI
    public ObservableCollection<ExpenseItem> Today { get; } = new();

    // 🔹 Access shared services through helper
    private Firebase.Auth.FirebaseAuthClient? Auth =>
        AppServiceHelper.Services.GetService<Firebase.Auth.FirebaseAuthClient>();

    private TodayEntriesService? EntriesSvc =>
        AppServiceHelper.Services.GetService<TodayEntriesService>();

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Default selection for the Quick Add dropdown
        CategoryPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTodaysExpensesAsync();
        RefreshTotals();
    }

    // ----------------- Firestore: Load Today's Entries -----------------
    private async Task LoadTodaysExpensesAsync()
    {
        try
        {
            // Ensure initialized
            

            Today.Clear();

            // Prefer signed-in user; else fallback uid
            var uid = Auth?.User?.Uid ?? "demo-user-1";

            var tz = TimeZoneInfo.Local;
            var startLocal = DateTime.Today;
            var endLocal = startLocal.AddDays(1);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

            var db = FirestoreDatabase.Database;

            Query q = db.Collection("users").Document(uid).Collection("entries")
                .WhereGreaterThanOrEqualTo("occurredAt",
                    Timestamp.FromDateTime(DateTime.SpecifyKind(startUtc, DateTimeKind.Utc)))
                .WhereLessThan("occurredAt",
                    Timestamp.FromDateTime(DateTime.SpecifyKind(endUtc, DateTimeKind.Utc)))
                .OrderByDescending("occurredAt");

            QuerySnapshot snap;
            try
            {
                snap = await q.GetSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception inner)
            {
                // Log more detail for gRPC failures
                System.Diagnostics.Debug.WriteLine("Firestore query failed: " + inner);
                throw; // rethrow to outer catch
            }

            foreach (var doc in snap.Documents)
            {
                var d = doc.ToDictionary();
                Today.Add(new ExpenseItem
                {
                    Id = doc.Id,
                    Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                    Description = d.TryGetValue("description", out var ds) ? (string)ds : "",
                    Amount = d.TryGetValue("amount", out var a) ? ToDouble(a) : 0d,
                    OccurredAt = d.TryGetValue("occurredAt", out var t) && t is Timestamp ts
                                  ? ts.ToDateTime().ToLocalTime()
                                  : DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            // Never crash; show the error once and use mock data so UI stays up
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Firestore error", ex.Message, "OK");
            });
            LoadMockData();
        }
    }


    private static double ToDouble(object v)
    {
        if (v is double d) return d;
        if (v is long l) return l;
        if (v is int i) return i;
        return double.TryParse(v?.ToString(), out var x) ? x : 0d;
    }

    // ----------------- Quick Add (UI-only) -----------------
    private async void OnQuickAdd(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DescEntry.Text) ||
            string.IsNullOrWhiteSpace(AmountEntry.Text) ||
            CategoryPicker.SelectedIndex < 0)
        {
            await DisplayAlert("Add Expense", "Please enter description, amount, and category.", "OK");
            return;
        }

        if (!double.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Add Expense", "Amount must be a positive number.", "OK");
            return;
        }

        Today.Insert(0, new ExpenseItem
        {
            Category = CategoryPicker.Items[CategoryPicker.SelectedIndex],
            Description = DescEntry.Text.Trim(),
            Amount = amount,
            OccurredAt = DateTime.Now
        });

        // reset inputs
        DescEntry.Text = string.Empty;
        AmountEntry.Text = string.Empty;
        CategoryPicker.SelectedIndex = 0;

        RefreshTotals();
    }

    // ----------------- Edit/Delete (UI-only) -----------------
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
            if (confirm)
            {
                Today.Remove(item);
                RefreshTotals();
            }
        }
    }

    // ----------------- Header totals -----------------
    void RefreshTotals()
    {
        var todayTotal = Today.Sum(x => x.Amount);
        TodayTotalLabel.Text = $"${todayTotal:F2}";
        WeekTotalLabel.Text = "$234.56";     // Placeholder for now
        MonthTotalLabel.Text = "$1,123.45";  // Placeholder for now
    }

    // ----------------- Mock data fallback -----------------
    void LoadMockData()
    {
        Today.Clear();
        var now = DateTime.Now;

        Today.Add(new ExpenseItem
        {
            Category = "Food & Drink",
            Description = "Lunch at Chipotle",
            Amount = 12.50,
            OccurredAt = now.AddHours(-2)
        });
        Today.Add(new ExpenseItem
        {
            Category = "Food & Drink",
            Description = "Coffee at Starbucks",
            Amount = 5.99,
            OccurredAt = now.AddHours(-5)
        });
        Today.Add(new ExpenseItem
        {
            Category = "Shopping",
            Description = "Amazon impulse buy",
            Amount = 29.33,
            OccurredAt = now.AddHours(-7)
        });
    }

    // ----------------- Header actions / tabs -----------------
    private void OnLogout(object sender, EventArgs e) =>
        Application.Current.MainPage = new LoginPage();

    private void OnTabDashboard(object sender, EventArgs e) { /* already here */ }
    private void OnTabExpenses(object sender, EventArgs e) { /* navigate later */ }
    private void OnTabBudget(object sender, EventArgs e) { /* navigate later */ }
    private void OnTabReports(object sender, EventArgs e) =>
        Application.Current.MainPage = new ReportPage();
    private void OnTabProfile(object sender, EventArgs e) =>
        Application.Current.MainPage = new ProfilePage();

    public bool IsEmpty => Today.Count == 0;
}
