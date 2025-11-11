using System.Collections.ObjectModel;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System.Diagnostics;

namespace Leux;

public partial class ExpensesPage : ContentPage
{
    public ObservableCollection<ExpenseItem> AllExpenses { get; } = new();

    private Firebase.Auth.FirebaseAuthClient? Auth =>
        AppServiceHelper.Services.GetService<Firebase.Auth.FirebaseAuthClient>();

    private FirestoreDb? Db =>
        AppServiceHelper.Services.GetService<FirestoreDb>();

    private ExpenseItem? _itemToEdit;

    public ExpensesPage()
    {
        InitializeComponent();
        BindingContext = this;
        CategoryFilter.SelectedIndex = 0;
        RangeFilter.SelectedIndex = 1;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var userName = Auth?.User?.Info?.DisplayName;

        if (string.IsNullOrEmpty(userName))
        {
            userName = Auth?.User?.Info?.Email;
        }

        if (string.IsNullOrEmpty(userName))
        {
            userName = "User";
        }

        if (userName.Contains("@"))
        {
            userName = userName.Split('@')[0];
        }

        WelcomeLabel.Text = $"Welcome back, {userName}!";

        await LoadExpensesAsync();
    }

    async Task LoadExpensesAsync()
    {
        try
        {
            if (Db is null) return;

            var uid = Auth?.User?.Uid ?? "demo-user-1";

            var (startUtc, endUtc) = GetRangeUtc(RangeFilter.SelectedIndex);
            Query q = Db.Collection("users").Document(uid).Collection("entries");

            if (startUtc != null && endUtc != null)
            {
                q = q.WhereGreaterThanOrEqualTo("occurredAt",
                        Timestamp.FromDateTime(DateTime.SpecifyKind(startUtc.Value, DateTimeKind.Utc)))
                     .WhereLessThan("occurredAt",
                        Timestamp.FromDateTime(DateTime.SpecifyKind(endUtc.Value, DateTimeKind.Utc)));
            }

            q = q.OrderByDescending("occurredAt");
            var snap = await q.GetSnapshotAsync();

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

    private (DateTime? startUtc, DateTime? endUtc) GetRangeUtc(int rangeIndex)
    {
        if (rangeIndex == 3) return (null, null);
        int days = rangeIndex switch { 0 => 7, 1 => 30, 2 => 90, _ => 30 };
        var tz = TimeZoneInfo.Local;
        var endLocal = DateTime.Today.AddDays(1);
        var startLocal = endLocal.AddDays(-days);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
        return (startUtc, endUtc);
    }

    private async void OnFilterChanged(object sender, EventArgs e) => await LoadExpensesAsync();


    private void OnEditClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ExpenseItem item)
        {
            _itemToEdit = item;
            EditDescriptionEntry.Text = item.Description;
            EditAmountEntry.Text = item.Amount.ToString();
            EditCategoryPicker.SelectedItem = item.Category;

            EditDatePicker.Date = item.OccurredAt;

            EditPopupOverlay.IsVisible = true;
        }
    }

    private void OnCancelEditClicked(object sender, EventArgs e)
    {
        EditPopupOverlay.IsVisible = false;
        _itemToEdit = null;
    }

    private async void OnSaveEditClicked(object sender, EventArgs e)
    {
        if (_itemToEdit == null) return;

        if (string.IsNullOrWhiteSpace(EditDescriptionEntry.Text) ||
            !double.TryParse(EditAmountEntry.Text, out double newAmount) || newAmount <= 0 ||
            EditCategoryPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please fill in all fields with valid data.", "OK");
            return;
        }

        try
        {
            var uid = Auth?.User?.Uid ?? "demo-user-1";

            var updatedData = new Dictionary<string, object>
            {
                { "description", EditDescriptionEntry.Text },
                { "amount", newAmount },
                { "category", EditCategoryPicker.SelectedItem.ToString() },
                { "occurredAt", Timestamp.FromDateTime(EditDatePicker.Date.ToUniversalTime()) }
            };

            await Db.Collection("users").Document(uid)
                    .Collection("entries").Document(_itemToEdit.Id)
                    .UpdateAsync(updatedData);

            EditPopupOverlay.IsVisible = false;
            _itemToEdit = null;
            await LoadExpensesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ExpenseItem item)
        {
            bool confirm = await DisplayAlert("Delete", $"Delete '{item.Description}'?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    var uid = Auth?.User?.Uid ?? "demo-user-1";

                    await Db.Collection("users").Document(uid)
                            .Collection("entries").Document(item.Id)
                            .DeleteAsync();

                    AllExpenses.Remove(item);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }
    }

    private static double ToDouble(object v)
    {
        if (v is double d) return d;
        if (v is long l) return l;
        if (v is int i) return i;
        return double.TryParse(v?.ToString(), out var x) ? x : 0d;
    }
}