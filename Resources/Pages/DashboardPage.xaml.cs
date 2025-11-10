using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using Leux.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Leux;

public partial class DashboardPage : ContentPage
{
    public ObservableCollection<ExpenseItem> Today { get; } = new();

    private Firebase.Auth.FirebaseAuthClient? Auth =>
        AppServiceHelper.Services.GetService<Firebase.Auth.FirebaseAuthClient>();

    private TodayEntriesService? EntriesSvc =>
        AppServiceHelper.Services.GetService<TodayEntriesService>();

    private readonly IDashboardService _dashboardService;
    private readonly FirebaseAuthClient _authClient;
    private readonly INavigationService _navigationService;

    public DashboardPage(IDashboardService dashboardService, FirebaseAuthClient authClient, INavigationService navigationService)
    {
        InitializeComponent();
        BindingContext = this;
        _dashboardService = dashboardService;
        _authClient = authClient;
        _navigationService = navigationService;

        ExpenseDatePicker.Date = DateTime.Today;
        CategoryPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTodaysExpensesAsync();
        await RefreshTotals();
    }

    // ----------------- Firestore: Load Today's Entries -----------------
    private async Task LoadTodaysExpensesAsync()
    {
        string currentUserId = _authClient?.User?.Uid;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            Today.Clear();
            return;
        }

        LoadingSpinner.IsVisible = true;
        TodayList.IsVisible = false;

        try
        {
            var expenses = await _dashboardService.GetUserExpensesAsync(currentUserId);
            Debug.WriteLine("Total Expenses: " + expenses.Count);

            foreach (var expense in expenses)
            {
                Today.Insert(0, new ExpenseItem
                {
                    Category = expense.Category,
                    Description = expense.Name,
                    Amount = expense.Cost,
                    OccurredAt = expense.Date
                });

                Debug.WriteLine($"Count: {Today.Count} >= 6");
                if (Today.Count >= 6)
                {
                    Today.RemoveAt(5);
                }
            }

            LoadingSpinner.IsVisible = false;
            TodayList.IsVisible = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error on getting expenses from dashboard service {ex.Message}");
        }
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        try
        {
            string currentUserId = _authClient?.User?.Uid;

            if (!await OnQuickAdd(sender, e)) return;

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                await DisplayAlert("Not Logged In", "You must be logged in to add an expense.", "OK");
                return;
            }

            var description = DescEntry.Text;
            var amountText = AmountEntry.Text;
            var category = CategoryPicker.SelectedItem as string;

            DateTime selectedDate = ExpenseDatePicker.Date;

            if (string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(amountText) ||
                string.IsNullOrWhiteSpace(category) ||
                !double.TryParse(amountText, out double amount))
            {
                await DisplayAlert("Error", "Please fill in all fields correctly.", "OK");
                return;
            }

            var newExpense = new ExpenseEntry
            {
                Name = description,
                Category = category,
                Cost = amount,
                Date = Timestamp.FromDateTime(selectedDate.ToUniversalTime())
            };

            bool success = await _dashboardService.AddExpenseAsync(currentUserId, newExpense);

            if (success)
            {
                await DisplayAlert("Success!", "A new expense was added to your record.", "OK");
                DescEntry.Text = "";
                AmountEntry.Text = "";
                CategoryPicker.SelectedIndex = 0;
                ExpenseDatePicker.Date = DateTime.Today;
            }
            else
            {
                await DisplayAlert("Failure", "Could not add the expense.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            Debug.WriteLine($"FULL ERROR: {ex}");
        }
    }

    private async Task<bool> OnQuickAdd(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DescEntry.Text) ||
            string.IsNullOrWhiteSpace(AmountEntry.Text) ||
            CategoryPicker.SelectedIndex < 0)
        {
            await DisplayAlert("Add Expense", "Please enter description, amount, and category.", "OK");
            return false;
        }

        if (!double.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Add Expense", "Amount must be a positive number.", "OK");
            return false;
        }

        Today.Insert(0, new ExpenseItem
        {
            Category = CategoryPicker.Items[CategoryPicker.SelectedIndex],
            Description = DescEntry.Text.Trim(),
            Amount = amount,
            OccurredAt = Timestamp.FromDateTime(ExpenseDatePicker.Date)
        });

        if (Today.Count >= 6)
        {
            Today.RemoveAt(5);
        }

        RefreshTotals();
        return true;
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
    async Task RefreshTotals()
    {
        string currentUserId = _authClient?.User?.Uid;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            TodayTotalLabel.Text = "$0.00";
            WeekTotalLabel.Text = "$0.00";
            MonthTotalLabel.Text = "$0.00";
            return;
        }

        try
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var lastWeek = now.AddDays(-7);
            var lastMonth = now.AddDays(-30);
            var allExpenses = await _dashboardService.GetUserExpensesAsync(currentUserId);

            var todayTotal = allExpenses
                .Where(x => x.Date.ToDateTime() >= last24Hours)
                .Sum(x => x.Cost);

            var weekTotal = allExpenses
                .Where(x => x.Date.ToDateTime() >= lastWeek)
                .Sum(x => x.Cost);

            var monthTotal = allExpenses
                .Where(x => x.Date.ToDateTime() >= lastMonth)
                .Sum(x => x.Cost);

            TodayTotalLabel.Text = $"${todayTotal:F2}";
            WeekTotalLabel.Text = $"${weekTotal:F2}";
            MonthTotalLabel.Text = $"${monthTotal:F2}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error calculating totals: {ex.Message}");
            TodayTotalLabel.Text = "$0.00";
            WeekTotalLabel.Text = "$0.00";
            MonthTotalLabel.Text = "$0.00";
        }
    }

    public bool IsEmpty => Today.Count == 0;
}
