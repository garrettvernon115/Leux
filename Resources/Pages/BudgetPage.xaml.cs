using Leux.Services;
using Leux.Resources.Models;
using Google.Cloud.Firestore;
using Firebase.Auth;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Leux.Resources.Firestore;

namespace Leux.Resources.Pages
{

    public partial class BudgetPage : ContentPage
    {
        private readonly IBudgetService _budgetService;
        private readonly FirebaseAuthClient _authClient;
        private readonly INavigationService _navigationService;

        public ObservableCollection<BudgetSummary> BudgetSummaries { get; set; }

        // This constructor matches your latest version
        public BudgetPage(INavigationService navigationService, IBudgetService budgetService, FirebaseAuthClient authClient)
        {
            InitializeComponent();

            _budgetService = budgetService;
            _navigationService = navigationService;
            _authClient = authClient;

            BudgetSummaries = new ObservableCollection<BudgetSummary>();
            BudgetsCollectionView.ItemsSource = BudgetSummaries;


            CategoryPicker.ItemsSource = new List<string>
            {
                "All Categories",
                "Food & Drink",
                "Entertainment",
                "Utilities",
                "Transport",
                "Shopping",
                "Other"

            };
            CategoryPicker.SelectedIndex = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadUserBudgetSummaries();
        }

        private async Task LoadUserBudgetSummaries()
        {
            string currentUserId = _authClient?.User?.Uid;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                BudgetSummaries.Clear();
                return;
            }


            LoadingSpinner.IsVisible = true;
            BudgetsCollectionView.IsVisible = false;

            try
            {

                // 1. Get all summaries from the service
                var summaries = await _budgetService.GetUserBudgetSummariesAsync(currentUserId);


                // 2. Populate the on-screen list
                BudgetSummaries.Clear();
                foreach (var summary in summaries.OrderBy(s => s.Name))
                {
                    BudgetSummaries.Add(summary);
                }

                // --- *** NEW: BUDGET ALERT LOGIC *** ---

                // 3. Check for any budgets over the 80% threshold
                var budgetsOverThreshold = summaries
                    .Where(s => s.PercentSpent >= 0.80)
                    .ToList();

                // 4. If any budgets are found, build and show the alert
                if (budgetsOverThreshold.Any())
                {
                    // Construct a list of budget names and their percentages
                    var budgetAlertStrings = budgetsOverThreshold
                        .Select(b => $"{b.Name} ({b.PercentSpent:P0})");

                    string budgetList = string.Join(", ", budgetAlertStrings);

                    string title = "Budget Alert";
                    string message = $"You have reached or exceeded 80% of your limit for the following budgets: {budgetList}.";

                    // Display the alert as requested
                    await DisplayAlert(title, message, "OK");
                }
                // --- *** END OF NEW LOGIC *** ---
            }
            catch (Exception ex)
            {
                await DisplayAlert("Loading Error", $"Failed to load budgets: {ex.Message}", "OK");
            }
            finally
            {

                LoadingSpinner.IsVisible = false;
                BudgetsCollectionView.IsVisible = true;
            }
        }

        private void OnAddBudgetClicked(object sender, EventArgs e)
        {
            BudgetNameEntry.Text = string.Empty;
            BudgetAmountEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = 0;
            BudgetDatePicker.Date = DateTime.Today;

            PopupOverlay.IsVisible = true;
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            PopupOverlay.IsVisible = false;
        }

        // SaveBudget now includes Category
        private async void OnSaveBudgetClicked(object sender, EventArgs e)
        {
            string currentUserId = _authClient?.User?.Uid;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                await DisplayAlert("Error", "You must be logged in to save a budget.", "OK");
                return;
            }

            string name = BudgetNameEntry.Text;
            string amountText = BudgetAmountEntry.Text;
            DateTime selectedDate = BudgetDatePicker.Date;
            string selectedCategory = CategoryPicker.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(amountText) ||
                !double.TryParse(amountText, out double amount) || amount <= 0 ||
                string.IsNullOrEmpty(selectedCategory))
            {
                await DisplayAlert("Validation Error", "Please enter valid budget details.", "OK");
                return;
            }

            string budgetCategory = (selectedCategory == "All Categories") ? null : selectedCategory;

            var newBudget = new Budget
            {
                Name = name,
                Category = budgetCategory,
                SpendingLimit = amount,
                TimePeriod = "Monthly",
                StartDate = Timestamp.FromDateTime(new DateTime(selectedDate.Year, selectedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                EndDate = Timestamp.FromDateTime(new DateTime(selectedDate.Year, selectedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1)),
            };

            try
            {
                bool success = await _budgetService.CreateBudgetAsync(currentUserId, newBudget);
                if (success)
                {
                    await DisplayAlert("Success", $"Budget '{name}' saved!", "OK");
                    PopupOverlay.IsVisible = false;


                    await LoadUserBudgetSummaries();
                }
                else
                {
                    await DisplayAlert("Failure", "Could not save the budget.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
                Console.WriteLine($"Save Budget Error: {ex.Message}");
            }
        }

        private async void OnSignOut(object sender, EventArgs e)
        {
            try
            {
                _authClient.SignOut();
                await _navigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to sign out: {ex.Message}", "OK");
            }
        }
    }
}